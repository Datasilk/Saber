S.editor.fields = {
    clone: $('.editor .textarea-clone > div'),
    selected: '',
    changed: false,
    file: {},
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
            lang = $(contentfields + ' #lang').val();

            if ($('.content-fields-' + fileid).length == 0) {
                //generate content fields section
                var div = document.createElement('div');
                div.className = 'tab content-fields-' + fileid;
                div.innerHTML = $('#template_contentfields').html();
                $('.editor .sections').append(div);
                S.editor.fields.file[file] = true;
                S.editor.resizeWindow();
                $('.editor .sections > .tab').addClass('hide');
                $('.editor .sections > .content-fields-' + fileid).removeClass('hide');

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
                S.editor.tabs.create('Content: ' + filepath, 'content-fields-' + fileid, { removeOnClose:true },
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
            }
        }
        console.log(contentfields);

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
            if (file) {
                S.editor.fields.file[file]= true;
            } else {
                S.editor.fields.changed = true;
            }
        }
        S.editor.fields.resize(e);
    },
    save: function () {
        var fields = {};
        var seltab = $('.tab-for-content-fields.selected > div');
        var section = seltab.attr('data-path');
        var pathid = seltab.attr('data-path');
        var path = seltab.attr('data-path-url') || S.editor.path;
        var texts = $('.' + section + ' form .input-field');
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
                    S.message.show('.' + pathid + ' .message', 'confirm', 'Content fields were saved.', false, 4000, true);
                } else { S.editor.error(); }
            },
            function () {
                S.editor.error();
            }
        );
    }
};