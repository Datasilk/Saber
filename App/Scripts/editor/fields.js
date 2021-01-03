S.editor.fields = {
    clone: $('.editor .textarea-clone > div'),
    selected: '',
    changed: false,
    load: function (file, show) {
        if (typeof file == 'object') { file = null;}
        let lang = $('.content-fields-section #lang').val();
        let filepath = '';
        let fileid = '';
        let tabid = '';
        //let folder = '';
        var container = '.sections .content-fields-section';
        if (!file) {
            S.editor.fields.changed = false;
            $('.content-fields-section form').html('');
            tabid = 'content-fields-section';
        } else {
            //load content for partial view content fields
            filepath = file.replace('content/partials/', '');
            fileid = filepath.replace(/\./g, '_').replace(/\//g, '_');
            tabid = 'content-fields-' + fileid;
            //folder = file.split('/').splice(-1, 1).join('/');
            container = '.sections .content-fields-' + fileid;
            if (show !== false) {
                $('.editor .sections > .tab').addClass('hide');
            }

            if ($('.content-fields-' + fileid).length == 0) {
                //generate content fields section
                var div = document.createElement('div');
                div.className = 'tab content-fields-' + fileid;
                div.innerHTML = $('#template_contentfields').html();
                $('.editor .sections').append(div);
                S.editor.resizeWindow();
                if (show !== false) {
                    $('.editor .sections > .content-fields-' + fileid).removeClass('hide');
                }
                lang = window.language;

                //get list of languages
                S.ajax.post('Languages/Get', {},
                    function (d) {
                        var langs = d.split('|');
                        var sel = $(container + ' #lang');
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
                S.editor.tabs.create('Content: ' + filepath, 'content-fields-' + fileid, { removeOnClose: true, selected:show !== false },
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
                lang = $(container + ' #lang').val();
                if (show !== false) {
                    $('.edit-tabs li > div').removeClass('selected');
                    $('.tab-content-fields-' + fileid + ' > div').addClass('selected');
                    $('.editor .sections > .content-fields-' + fileid).removeClass('hide');
                }
            }
        }
        S.editor.fields.render(file, lang, container, null, () => {});
    },
    render: function (file, lang, container, fields, callback) {
        var data = {
            path: file || S.editor.path,
            language: lang,
            container: container,
            data: fields ?? {}
        };
        S.ajax.post('ContentFields/Render', data,
            function (d) {
                d.selector = container + ' form';
                S.ajax.inject(d);

                //add language button
                $(container + ' .add-lang a').on('click', S.editor.lang.add.show);

                //set up events for fields
                $(container + ' form .input-field').on('keyup, keydown, change', (e) => { S.editor.fields.change(e, file) })
                    .each(function (field) {
                        S.editor.fields.change({ target: field }, file);
                    });

                //set up event for image selection buttons
                $(container + ' .select-image button').on('click', (e) => {
                    e.preventDefault();
                    S.editor.resources.select(file ? 'wwwroot/images' : S.editor.path, '.jpg, .png, .gif', true, "Select An Image", "Select Image", (results) => {
                        var parent = $(e.target).parents('.content-field');
                        var field = parent.find('.input-field');
                        var newpath = file ? 'images/' : S.editor.path.replace('content/', 'content/pages/') + '/';
                        var src = newpath + results[0];
                        parent.find('.img').html('<div><img src="' + src + '"/></div>');
                        field.val(src);
                        S.editor.fields.save(file);
                    });
                });

                $(container + ' .select-image .input-field').on('change', (e) => {
                    var parent = $(e.target).parents('.content-field');
                    var field = parent.find('.input-field');
                    parent.find('.img').html('<div><img src="' + field.val() + '"/></div>');
                });

                //initialize all custom fields
                S.editor.fields.custom.list.init(container);
                

                //initialize any accordions
                S.accordion.load({}, () => { S.editor.resizeWindow(); });

                if (callback) { callback();}
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
            init: function (container) {
                //let seltab = $('.tab-for-content-fields.selected > div');
                let section = $(container + ' form');
                //event listener for close button
                section.find('.list-items li .close-btn').off('click').on('click', S.editor.fields.custom.list.close);

                //drag & sort event listeners
                S.drag.sort.add(container + ' .list-items ul', container + ' .list-items li', (e) => {
                    //update list
                    var target = $(e.target);
                    var field = target.parents('.content-field').first();
                    var items = field.find('.list-items li');
                    let hidden = field.find('input.input-field');
                    var data = S.editor.fields.custom.list.parse(hidden);
                    var newdata = [];
                    for (var x = 0; x < items.length; x++) {
                        var item = $(items[x]);
                        var i = parseInt(item.attr('data-index')) - 1;
                        newdata[x] = data[i];
                        item.attr('data-index', x + 1);
                    }
                    hidden.val(JSON.stringify(newdata));
                    S.editor.fields.save();
                });

                //reset indexes
                var lists = section.find('.list-items');
                for (var x = 0; x < lists.length; x++) {
                    var items =$(lists[x]).find('li');
                    for (var y = 0; y < items.length; y++) {
                        $(items[y]).attr('data-index', y + 1);
                    }
                }
            },
            parse: function (hidden) {
                var data = hidden.val();
                if (data && data != '') {
                    data = JSON.parse(data);
                } else {
                    data = [];
                }
                return data;
            },
            close: function (e) {
                e.cancelBubble = true;
                var target = $(e.target);
                var field = target.parents('.content-field').first();
                var li = target.parents('li').first();
                var index = li.attr('data-index') - 1;
                var hidden = field.find('input.input-field');
                var data = S.editor.fields.custom.list.parse(hidden);
                if (data.length >= index) {
                    data.splice(index, 1);
                    hidden.val(JSON.stringify(data));
                    li.remove();
                    S.editor.fields.save();
                }

                //reset indexes
                var items = field.find('.list-items li');
                for (var y = 0; y < items.length; y++) {
                    $(items[y]).attr('data-index', y + 1);
                }
            }, 
            add: function (e, title, key, partial, lang, container) {
                return S.editor.fields.custom.list.update(e, title, key, partial, lang, container);
            },
            edit: function (e, title, key, partial, lang, container) {
                //get data for field
                var li = $(e.target);
                if (e.target.tagName.toLowerCase() != 'li') {
                    li = li.parents('li').first();
                }
                var index = 0;
                var ul = li.parent();
                var lis = ul.children();
                for (var x = 0; x < lis.length; x++) {
                    if (lis[x] == li[0]) {
                        index = x; break;
                    }
                }
                var data = JSON.parse(li.parents('.content-field').find('input.input-field').val()) ?? [];
                S.editor.fields.custom.list.update(e, title, key, partial, lang, container, data[index], index);
            },
            update: function (e, title, key, partial, lang, container, fieldsdata, index) {
                e.preventDefault();
                var field = $(e.target).parents('.content-field').first();
                var hidden = field.find('input.input-field');
                var popup = $(S.popup.show((fieldsdata ? 'Update' : 'Add') + ' List Item for ' + title.substr(5), '<div class="has-content-fields"><form></form></div>'));
                popup.css({ width: '90%', 'max-width': '1200px' });

                //load content fields into popup modal
                S.editor.fields.render('content/' + partial, lang, '.box.popup', fieldsdata, () => {
                    popup.find('form').append('<div class="row pad-top text-center"><div class="col"><button class="apply">' + (fieldsdata ? 'Update' : 'Add') + ' List Item</button></div>');
                    S.popup.resize();

                    popup.find('form').on('submit', (e) => {
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
                        var data = S.editor.fields.custom.list.parse(hidden);

                        //add item to list in content fields tab
                        var i = (index != null ? parseInt(index) : field.find('.list-items li').length) + 1;
                        var ul = field.find('.list-items ul');
                        var children = field.find('.list-items ul > li').map((i, a) => a.outerHTML);
                        var child = $('#custom_field_list_item').html()
                            .replace('##onclick##', "S.editor.fields.custom.list.edit(event, '##title##', '##key##', '##partial##', '##lang##', '##container##')")
                            .replace(/\#\#label\#\#/g, key != '' ? fields[key] : 'List Item #' + i)
                            .replace(/\#\#index\#\#/g, i)
                            .replace(/\#\#title\#\#/g, title)
                            .replace(/\#\#key\#\#/g, key)
                            .replace(/\#\#partial\#\#/g, partial)
                            .replace(/\#\#lang\#\#/g, lang)
                            .replace(/\#\#container\#\#/g, container);
                        ul.html('');
                        if (fieldsdata != null && index != null) {
                            //update existing element in data list
                            data[index] = fields;
                            children[index] = child;
                        } else {
                            data.push(fields);
                            children.push(child);
                        }
                        for (var x = 0; x < children.length; x++) {
                            ul.append(children[x]);
                        }
                        hidden.val(JSON.stringify(data));
                        field.find('.accordion').addClass('expanded');
                        S.editor.fields.save();
                        S.popup.hide();
                        S.editor.fields.custom.list.init(container);
                        return false;
                    });
                });
                return false;
            }
        }
    }
};