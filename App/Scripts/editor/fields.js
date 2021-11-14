S.editor.fields = {
    clone: $('.editor .textarea-clone > div'),
    selected: '',
    changed: false,
    load: function (file, show) {
        if (typeof file == 'object') { file = null;}
        let lang = $('.content-fields-section #lang').val();
        let filepath = '';
        let fileid = '';
        var container = '.sections .content-fields-section';
        if (!file) {
            S.editor.fields.changed = false;
            $('.content-fields-section form').html('');
        } else {
            //load content for partial view content fields
            filepath = file.replace('content/partials/', '');
            fileid = filepath.replace(/\./g, '_').replace(/\//g, '_');
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
                S.editor.resize.window();
                if (show !== false) {
                    $('.editor .sections > .content-fields-' + fileid).removeClass('hide');
                }
                lang = window.language;

                //get list of languages
                S.editor.lang.load(container + ' #lang', lang, (e) => {
                    //changed selected language
                    S.editor.fields.load(file);
                });

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
    render: function (file, lang, container, fields, callback, ispopup, showlang, excludeFields, renderApi) {
        var data = {
            path: file || S.editor.path,
            language: lang,
            container: container,
            showlang: showlang || false,
            data: fields ?? {},
            exclude: excludeFields
        };
        S.ajax.post(renderApi ?? 'ContentFields/Render', data,
            function (d) {
                d.selector = container + ' form';
                S.ajax.inject(d);

                //add language button
                $(container + ' .add-lang a').on('click', S.editor.lang.add.show);

                //set up events for fields
                $(container + ' form .input-field').on('keyup, keydown, change', (e) => { S.editor.fields.change(e, file) });
                S.editor.fields.resizeAll();

                //set up event for image selection buttons
                $(container + ' .select-image button').on('click', (e) => {
                    e.preventDefault();
                    S.editor.resources.select(file ? 'wwwroot/images' : S.editor.path, '.jpg, .png, .gif', true, "Select An Image", "Select Image", (results) => {
                        var parent = $(e.target).parents('.content-field');
                        var field = parent.find('.input-field');
                        var newpath = file ? '/images/' : S.editor.path.replace('content/', 'content/pages/') + '/';
                        var src = newpath + results[0];
                        parent.find('.img').html('<div><img src="' + src + '"/></div>');
                        parent.find('.img').css({ 'background-image': 'url(' + src + ')' });
                        field.val(src);
                        if (!ispopup) { S.editor.fields.save(file); }
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
                S.accordion.load({}, () => { S.editor.resize.window(); });

                if (callback) { callback();}
            },
            function () { S.editor.error(); },
            true
        );
    },
    popup: function (partial, lang, title, fieldsdata, buttonTitle, submit, excludeFields, renderApi, callback) {
        //load content fields into popup modal
        var popup = S.popup.show(title, '<div class="has-content-fields"><form></form></div>');
        popup.css({ width: '90%', 'max-width': '1200px' });
        S.editor.fields.render('content/' + partial, lang, '.box.popup.show', fieldsdata, () => {
            popup.find('form').append('<div class="row text-center"><div class="col"><button class="apply">' + buttonTitle + '</button></div>');
            S.popup.resize();
            popup.find('form').on('submit', (e) => {
                e.preventDefault();
                var fields = {};
                var texts = popup.find('form .input-field');
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
                submit(e, fields);
            });
            if (typeof callback == 'function') {
                callback();
            }
        }, true, true, excludeFields, renderApi);
        return popup;
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
    resizeAll: function () {
        $('.has-content-fields form .input-field')
            .each(function (field) {
                S.editor.fields.resize({ target: field });
            });
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

    saveThenPreview: function () {
        console.log('save then preview');
        S.editor.fields.save(S.editor.filebar.preview.show);
        
    },
    listeners: {
        save: {
            items: [],
            add: function (callback) {
                S.editor.fields.listeners.save.items.push(callback);
            },
            call: function () {
                var calls = S.editor.fields.listeners.save.items;
                for (var x = 0; x < calls.length; x++) {
                    calls[x]();
                }
            }
        }
    },
    save: function (callback) {
        if (S.editor.fields.changed == false) { return; }
        var seltab = $('.tab-for-content-fields.selected > div');
        var pathid = seltab.attr('data-path');

        //execite all listener callbacks before saving
        S.editor.fields.listeners.save.items.forEach(a => a($('.' + pathid)));

        var fields = {};
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
                    S.message.show('.' + pathid + ' .messages', 'confirm', 'Content fields were saved.', false, 4000, true);
                    if (callback) { callback(); }
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
                let section = $(container + ' form');
                S.accordion.load({}, () => { S.editor.resize.window(); });

                //event listener for all input fields
                section.find('input, select').on('input', (e) => {
                    S.editor.fields.changed = true;
                });

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

                //initialize all data source filter forms
                section.find('.filter-settings .accordion .contents[data-init]').each((a) => {
                    var field = $(a).parents('.content-field').first();
                    var hidden = field.find('input.input-field');
                    var oninit = $(a).attr('data-init');
                    var obj = S.editor.objectFromString(oninit);
                    if (obj && typeof obj == 'function') {
                        obj(field.find('.filter-settings .accordion > .contents'), hidden);
                    }
                });
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
            add: function (e, title, key, partial, lang, container, renderApi) {
                return S.editor.fields.custom.list.update(e, title, key, partial, lang, container, null, null, renderApi);
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
            update: function (e, title, key, partial, lang, container, fieldsdata, index, renderApi) {
                e.preventDefault();
                var field = $(e.target).parents('.content-field').first();
                var hidden = field.find('input.input-field');
                var popup = S.editor.fields.popup(partial, lang, (fieldsdata ? 'Update' : 'Add') + ' List Item for ' + title.substr(5), fieldsdata, (fieldsdata ? 'Update' : 'Add') + ' List Item', (e, fields) => {
                    //save custom list item
                    var hasDataSource = hidden.val().indexOf('data-src=') >= 0;
                    if (hasDataSource) {
                        //add list item to data source
                        var datafields = hidden.val().split('|!|');
                        var datasrc = datafields.filter(a => a.indexOf('data-src=') == 0)[0].replace('data-src=', '');
                        S.ajax.post('DataSources/AddRecord', {datasource:datasrc, columns:fields}, (response) => {});
                    } else {
                        //add list item directly to list component hidden field
                        var data = S.editor.fields.custom.list.parse(hidden);
                        if (fieldsdata != null && index != null) {
                            data[index] = fields;
                        } else {
                            data.push(fields);
                        }
                        hidden.val(JSON.stringify(data));
                        field.find('.list-items .accordion').addClass('expanded');
                    }

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
                        //update existing element
                        children[index] = child;
                    } else {
                        children.push(child);
                    }
                    for (var x = 0; x < children.length; x++) {
                        ul.append(children[x]);
                    }
                    if (!hasDataSource) {
                        S.editor.fields.save();
                    }
                    S.popup.hide(popup);
                    S.editor.fields.changed = true;
                    S.editor.fields.custom.list.init(container);
                    return false;
                }, null, renderApi);
                return false;
            },
            datasource: {
                list: function(e, title) {
                    e.preventDefault();
                    var field = $(e.target).parents('.content-field').first();
                    var hidden = field.find('input.input-field');
                    S.ajax.post('DataSources/List', {}, (response) => {
                        var html = '';
                        if (response.length >= 0) {
                            response.unshift(['list-items', 'User-Defined List Items (default)']);
                            html += '<ul class="list">' + response.map(a => {
                                return '<li class="item" data-id="' + a[0] + '"><span>' + a[1] + '</span></li>'
                            }).join('') + '</ul>';
                        }
                        var popup = S.popup.show('Select A Data Source for "' + title + '"', html, { width: '100%', maxWidth: 420 });
                        $('.popup .list li').on('click', (e) => {
                            var target = $(e.target);
                            if (!target.hasClass('item')) {
                                target = target.parents('.item').first();
                            }
                            var key = target.attr('data-id');
                            var name = target.find('span').html();
                            S.popup.hide(popup);
                            if (key == 'list-items') {
                                //show list items
                                field.find('.list-items  .contents').html('<ul class="list"></ul>');
                                field.find('.add-list-item').removeClass('hide');
                                field.find('.list-items').show();
                                field.find('.filter-settings').hide();
                                field.find('.datasource-name').hide();
                                hidden.val('');
                            } else {
                                //show data source filter
                                S.ajax.post('DataSources/RenderFilters', { key: key }, (filterform) => {
                                    //update list data with new data source
                                    var oninit = filterform.split('|')[0];
                                    filterform = filterform.split('|')[1];
                                    hidden.val('data-src=' + key);
                                    field.find('.filter-settings > .contents').html(filterform);
                                    field.find('.add-list-item').addClass('hide');
                                    field.find('.list-items').hide();
                                    field.find('.filter-settings').show();
                                    field.find('.datasource-name').show();
                                    field.find('.datasource-name b').html(name);
                                    S.ajax.inject(filterform);
                                    field.find('.filter-settings .accordion').addClass('expanded');
                                    if (oninit && oninit != '') {
                                        var obj = S.editor.objectFromString(oninit);
                                        if (obj && typeof obj == 'function') {
                                            obj(field.find('.accordion > .contents'), hidden);
                                        }
                                    }
                                }, (err) => {
                                        S.editor.error('', err.responseText);
                                });
                            }
                            S.editor.fields.changed = true;
                        });

                    }, (err) => {
                            S.editor.error('', 'error');
                    }, true);
                    return false;
                },
                filter: {
                    save: function (inputfield, filter) {
                        var src = inputfield.val().split('|!|')[0];
                        inputfield.val(src + '|!|' + JSON.stringify(filter));
                    }
                }
            },
            filters: {
                addGroup: function (key, e) {
                    var target = $(e.target);
                    var container = target.parents('.filter-group').find('.sub-groups');
                    var depth = 0;
                    if (container.length > 0) {
                        depth = container.length;
                        container = container.first();
                    } else {
                        //add root group
                        container = target.parents('.filter-settings').find('.filter-groups').first();
                        target.parents('.filter-settings').find('.no-filtergroups').remove();
                    }
                    S.ajax.post('DataSources/RenderFilterGroups', { key: key, groups: [{ Elements: [], Groups: [] }], depth:depth }, (response) => {
                        container.append(response);
                        var children = container.children();
                        $(children[children.length - 1]).find('input, select').on('input', (e) => {
                            S.editor.fields.changed = true;
                        });
                    });
                },

                removeGroup: function (e) {
                    var target = $(e.target);
                    target.parents('.filter-group').first().remove();
                },

                add: function (key, e) {
                    var target = $(e.target);
                    var container = target.parents('.filter-group').first();
                    var html = template_datasource_column.innerHTML;
                    S.ajax.post('DataSources/Columns', { key: key }, (response) => {
                        var columns = JSON.parse(response);
                        var options = '';
                        for (var x = 0; x < columns.length; x++) {
                            var c = columns[x].Name;
                            options += '<option value="' + c + '">' + c + '</options>';
                        }
                        html = html.replace('#options#', options);
                        S.editor.message.confirm('Select Data Source Column to filter by', html, {}, (response) => {
                            if (response == true) {
                                S.ajax.post('DataSources/RenderFilter', { key: key, column: datasource_column.value }, (response) => {
                                    var parent = container.find('.filters').first();
                                    parent.append(response);
                                    var children = parent.children();
                                    console.log($(children[children.length - 1]));
                                    console.log($(children[children.length - 1]).find('input, select'));
                                    $(children[children.length - 1]).find('input, select').on('input', (e) => {
                                        S.editor.fields.changed = true;
                                    });
                                });
                            }
                        });
                    });
                    
                }
            },
            save: function (form) {
                var lists = form.find('.list-component-field');

                function collectFilters(container, elem) {
                    //generate FilterGroup object
                    var result = {
                        Match: parseInt(container.find('.match-type select').val() ?? '0'),
                        Elements: [],
                        Groups: []
                    };
                    var subgroups = elem.find('.sub-groups').first().children();
                    var filters = elem.find('.filters').first().children();
                    for (var x = 0; x < filters.length; x++) {
                        var filter = $(filters[x]);
                        var type = 'text';
                        if (filter.hasClass('filter-input-bool')) { type = 'bool'; }
                        else if (filter.hasClass('filter-input-datetime')) { type = 'datetime'; }
                        else if (filter.hasClass('filter-input-number')) { type = 'number'; }
                        var val = '';
                        switch (type) {
                            case 'text': case 'number':
                                val = filter.find('.filter-input input').val() ?? '';
                                break;
                            case 'datetime':
                                var dt = new Date(filter.find('.filter-input input').val());
                                if (dt) {
                                    dt.setMinutes(dt.getMinutes() - dt.getTimezoneOffset());
                                    val = dt.toISOString().slice(0, 16) ?? ''
                                }
                                break;
                            case 'bool':
                                val = filter.find('.filter-input input')[0].checked ? '1' : '0'
                                break;
                        }
                        var column = filter.attr('data-column');
                        if (column == null) { continue; }
                        result.Elements.push({
                            Column: filter.attr('data-column'),
                            Match: parseInt(filter.find('.filter-match select').val() ?? '0'),
                            Value: val,
                            QueryName: filter.find('.query-name').val() ?? ''
                        });
                    }
                    for (var x = 0; x < subgroups.length; x++) {
                        result.Groups.push(collectFilters(container, $(subgroups[x])));
                    }
                    return result;
                }

                for (var i = 0; i < lists.length; i++) {
                    //for each list in the content fields form
                    var container = $(lists[i]).parents('.content-field').first();
                    //generate content for list hidden field
                    var filters = [];
                    var groups = container.find('.filter-groups').children();
                    for (var y = 0; y < groups.length; y++) {
                        var filter = collectFilters(container, $(groups[y]));
                        console.log(filter);
                        if (filter.Elements.length > 0 || filter.Groups.length > 0) {
                            filters.push(filter);
                        }
                    }
                    var json = 'filter=' + JSON.stringify(filters);
                    var input = container.find('.input-field');
                    var inputval = input.val();
                    var parts = inputval.split('|!|');
                    var found = false;
                    for (var x = 0; x < parts.length; x++) {
                        if (parts[x].indexOf('filter=') == 0) {
                            parts[x] = json;
                            found = true;
                            break;
                        }
                    }
                    if (!found) {
                        parts.push(json);
                    }
                    //console.log(parts);
                    input.val(parts.join('|!|'));
                }
            }
        }
    }
};

//add event listener for window resize stop to change height of all content field textarea inputs
S.editor.resize.stop.add('content-fields', S.editor.fields.resizeAll);

//add listener so when user saves content fields, it generates list data
S.editor.fields.listeners.save.add(S.editor.fields.custom.list.save);