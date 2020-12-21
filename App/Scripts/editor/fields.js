S.editor.fields = {
    clone: $('.editor .textarea-clone > div'),
    selected: '',
    changed: false,
    load: function (file) {
        if (typeof file == 'object') { file = null;}
        let lang = $('.content-fields-section #lang').val();
        let filepath = '';
        let fileid = '';
        var contentfields = '.sections .content-fields-section';
        if (!file) {
            S.editor.fields.changed = false;
            $('.content-fields-section form').html('');
        } else {
            //load content for partial view content fields
            filepath = file.replace('content/partials/', '');
            fileid = filepath.replace(/\./g, '_').replace(/\//g, '_');
            contentfields = '.sections .content-fields-' + fileid;
            $('.editor .sections > .tab').addClass('hide');

            if ($('.content-fields-' + fileid).length == 0) {
                //generate content fields section
                var div = document.createElement('div');
                div.className = 'tab content-fields-' + fileid;
                div.innerHTML = $('#template_contentfields').html();
                $('.editor .sections').append(div);
                S.editor.resizeWindow();
                $('.editor .sections > .content-fields-' + fileid).removeClass('hide');
                lang = window.language;

                //get list of languages
                S.ajax.post('Languages/Get', {},
                    function (d) {
                        var langs = d.split('|');
                        var sel = $(contentfields + ' #lang');
                        for (var x = 0; x < langs.length; x++) {
                            var l = langs[x].split(',');
                            sel.append('<option value="' + l[0] + '"' + (l[0] == lang ? ' selected="selected"' : '') + '>' + l[1] + '</option>');
                        }
                        sel.on('change', (e) => {
                            //changed selected language
                            S.editor.fields.load(file);
                        });
                    }
                );

                //render new tab
                S.editor.tabs.create('Content: ' + filepath, 'content-fields-' + fileid, { removeOnClose: true },
                    () => { //onfocus
                        $('.tab.content-fields-' + fileid).removeClass('hide');
                        $('ul.file-tabs > li').removeClass('selected');
                        $('ul.file-tabs > li.tab-content-fields').addClass('selected');
                        S.editor.filebar.update('Page Content for <a href="javascript:S.editor.explorer.open(\'' + file + '\')">' + file.replace('content/', '') + '</a>', 'icon-form-fields');
                        //TODO: check if associated HTML partial has changed, then reload content fields
                    },
                    () => { //onblur

                    },
                    () => { //onsave

                    }
                );

                $('.tab-content-fields-' + fileid).addClass('tab-for-content-fields');
                $('.tab-content-fields-' + fileid + ' > div').attr('data-path-url', file);
            } else {
                lang = $(contentfields + ' #lang').val();
                $('.edit-tabs li > div').removeClass('selected');
                $('.tab-content-fields-' + fileid + ' > div').addClass('selected');
                $('.editor .sections > .content-fields-' + fileid).removeClass('hide');
            }
        }

        S.ajax.post('ContentFields/Render', { path: file || S.editor.path, language: lang },
            function (d) {
                d.selector = contentfields + ' form';
                S.ajax.inject(d);

                //add language button
                $(contentfields + ' .add-lang a').on('click', S.editor.lang.add.show);

                //set up events for fields
                $(contentfields + ' form .input-field').on('keyup, keydown, change', (e) => { S.editor.fields.change(e, file) })
                    .each(function (field) {
                        S.editor.fields.change({ target: field }, file);
                    });

                //set up event for image selection buttons
                $(contentfields + ' .select-image button').on('click', (e) => {
                    e.preventDefault();
                    S.editor.resources.select(S.editor.path, '.jpg, .png, .gif', true, "Select An Image", "Select Image", (results) => {
                        var container = $(e.target).parents('.content-field');
                        var field = container.find('.input-field');
                        var newpath = S.editor.path.replace('content/', 'content/pages/') + '/';
                        var src = newpath + results[0];
                        container.find('.img').html('<div><img src="' + src + '"/></div>');
                        field.val(src);
                        S.editor.fields.save(file);
                    });
                });

                $(contentfields + ' .select-image .input-field').on('change', (e) => {
                    var container = $(e.target).parents('.content-field');
                    var field = container.find('.input-field');
                    container.find('.img').html('<div><img src="' + field.val() + '"/></div>');
                });

                //initialize any accordions
                S.accordion.load({}, () => { S.editor.resizeWindow(); }); 
            },
            function () { S.editor.error(); },
            true
        );
    },
    resize: function (e) {
        if (S.editor.visible == false) { return; }
        if (e) {
            //resize field
            var field = $(e.target);
            if (field.hasClass('text-field')) {
                var clone = S.editor.fields.clone;
                clone.html(field.val().replace(/\n/g, '<br/>') + '</br>');
                field.css({ height: clone.height() });
            }
        }
    },
    change: function (e, file) {
        if (S.editor.visible == false) { return; }
        if (S.editor.fields.changed == false || file != null) {
            //enable save menu
            $('.item-save').removeClass('faded').removeAttr('disabled');
            if (!file) {
                S.editor.fields.changed = true;
            }
        }
        S.editor.fields.resize(e);
    },
    save: function () {
        var fields = {};
        var seltab = $('.tab-for-content-fields.selected > div');
        var pathid = seltab.attr('data-path');
        var path = seltab.attr('data-path-url') || S.editor.path;
        var texts = $('.' + pathid + ' form .input-field');
        texts.each(function (txt) {
            if (!txt.id || (txt.id && txt.id.indexOf('field_') < 0)) { return;}
            var t = $(txt);
            var id = txt.id.replace('field_', '');
            switch (txt.tagName.toLowerCase()) {
                case 'textarea':
                    fields[id] = t.val();
                    break;
                case 'input':
                    var type = t.attr('type');
                    switch (type) {
                        case 'checkbox':
                            fields[id] = txt.checked == true ? '1' : '0';
                            break;
                        default:
                            fields[id] = t.val();
                            break;
                    }
                    break;
            }
        });
        S.ajax.post('ContentFields/Save', {
            path: path, fields: fields, language: $('.' + pathid + ' #lang').val() },
            function (d) {
                if (d == 'success') {
                    S.editor.fields.changed = false;
                    //html resource has changed because content fields have changed
                    if (pathid == S.editor.path) {
                        S.editor.files.html.changed = true;
                    } else {
                        S.editor.files.partials[path] = true;
                    }
                    S.editor.files.content.changed = true;
                    S.message.show('.' + pathid + ' .message', 'confirm', 'Content fields were saved.', false, 4000, true);
                } else { S.editor.error(); }
            },
            function () {
                S.editor.error();
            }
        );
    },
    custom: {
        list: {
            add: function (e, title, key, params) {
                e.preventDefault();
                var field = $(e.target).parents('.content-field').first();
                var hidden = field.find('input.input-field');

                //show popup modal to add new list item
                var params = params.split('|').map(a => {
                    var b = a.split(',')
                    return {key:b[0], title:b[0].replace(/\-/g, ' ').replace(/\_/g, ''), type:b[1]}
                });
                console.log(params);
                var param = $('#template_content_field').html();
                var html = '<div class="has-content-fields"><form>' +
                    params.map(a => {
                        var input = '';
                        var id = ' id="field_' + a.key + '"';
                        //determine which input to render
                        switch (a.type) {
                            case '0': //text
                                input = '<textarea' + id + ' class="input-field text-field"></textarea>';
                                break;
                            case '1': //block
                                input = '<input type="checkbox"' + id + ' class="input-field"></input>';
                                break;
                        }
                        return param.replace('##name##', a.title)
                            .replace('##input##', input);
                    }).join('') +
                    '<div class="row pad-top"><button class="apply">Add List Item</button></div>' +
                    '</form></div>';

                S.popup.show("Add List Item for " + title.substr(5), html);
                $('.popup').css({ width: '90%' });

                //add event listeners
                $('.popup textarea').on('keyup, keydown, change', S.editor.fields.resize)
                    .each(function (e) {
                        S.editor.fields.resize({ target: e });
                    });

                $('.popup form').on('submit', (e) => {
                    //save custom list item
                    e.preventDefault();
                    var fields = {};
                    var texts = $('.popup form .input-field');
                    texts.each(function (txt) {
                        if (!txt.id || (txt.id && txt.id.indexOf('field_') < 0)) { return; }
                        var t = $(txt);
                        var id = txt.id.replace('field_', '');
                        switch (txt.tagName.toLowerCase()) {
                            case 'textarea':
                                fields[id] = t.val();
                                break;
                            case 'input':
                                var type = t.attr('type');
                                switch (type) {
                                    case 'checkbox':
                                        fields[id] = txt.checked == true ? '1' : '0';
                                        break;
                                    default:
                                        fields[id] = t.val();
                                        break;
                                }
                                break;
                        }
                    });
                    var data = hidden.val();
                    if (data && data != '') {
                        data = JSON.parse(data);
                    } else {
                        data = [];
                    }
                    data.push(fields);
                    console.log(fields);
                    console.log(data);
                    hidden.val(JSON.stringify(data));
                    console.log(JSON.stringify(data));

                    //add item to list in content fields tab
                    var i = field.find('.list-items li').length + 1;
                    var index = 'List Item #' + i;
                    field.find('.list-items ul').append($('#custom_field_list_item').html()
                        .replace('##title##', key != '' ? fields[key] : index)
                        .replace('##index##', i)
                    );
                    field.find('.accordion').addClass('expanded');
                    S.editor.fields.save();
                    S.popup.hide();
                    return false;
                });

                return false;
            },
        }
    }
};