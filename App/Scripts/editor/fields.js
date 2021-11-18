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
                $(container + ' form .input-field').on('input', (e) => { S.editor.fields.change(e, file) });
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
                texts.each(function (i, txt) {
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
            .each(function (i, field) {
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
    presave: function () {
        //update all input-fields in all content field forms before saving
        $('.has-content-fields').each((i, form) => {
            S.editor.fields.custom.list.datasource.save($(form));
        });
    },

    save: function (callback) {
        if (S.editor.fields.changed == false) { return; }
        var seltab = $('.tab-for-content-fields.selected > div');
        var pathid = seltab.attr('data-path');

        //execute all listener callbacks before saving
        S.editor.fields.listeners.save.items.forEach(a => a($('.' + pathid)));

        var fields = {};
        var path = seltab.attr('data-path-url') || S.editor.path;
        var texts = $('.' + pathid + ' form .input-field');
        texts.each(function (i, txt) {
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
                    S.editor.fields.change();
                });

                //event listener for close button
                section.find('.list-items li .close-btn').off('click').on('click', S.editor.fields.custom.list.remove);

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
                    S.editor.fields.change();
                    S.editor.fields.save();
                });

                //S.drag.sort.add(container + ' .filter-settings .filter-groups, .filter-settings .sub-groups', container + ' .filter-settings .filter-group', S.editor.fields.custom.list.filters.sorted);
                //S.drag.sort.add(container + ' .filter-settings .filters', container + ' .filter-settings .filter', S.editor.fields.custom.list.filters.sorted);
                S.drag.sort.add(container + ' .orderby-settings .contents', container + ' .orderby', S.editor.fields.custom.list.orderby.sorted);
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
            remove: function (e) {
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
                field.find('.tab-list-items .icon-counter').html(data.length);

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
                        field.find('.tab-list-items .icon-counter').html(data.length);
                        hidden.val(JSON.stringify(data));
                        field.find('.list-items .accordion').addClass('expanded');
                    }

                    if (!hasDataSource) {
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
                        if (fieldsdata != null && index != null) {
                            //update existing element
                            children[index] = child;
                        } else {
                            //create new element
                            children.push(child);
                        }
                        ul.html('');
                        for (var x = 0; x < children.length; x++) {
                            ul.append(children[x]);
                        }
                        S.editor.fields.change();
                        S.editor.fields.save();
                    } else {
                        S.editor.fields.change();
                    }
                    S.popup.hide(popup);
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
                                field.find('.list-items .contents').html('<ul class="list"></ul>');
                                field.find('.list-items').show();
                                field.find('.filter-settings').hide();
                                field.find('.orderby-settings').hide();
                                field.find('.position-settings').hide();
                                field.find('.datasource-name').hide();
                                field.find('.tab-list-items').show();
                                field.find('.add-list-item').show();
                                field.find('.tab-filters').hide();
                                field.find('.tab-orderby').hide();
                                field.find('.tab-position').hide();
                                field.find('.tab-list-items .icon-counter').html('0');
                                hidden.val('');
                            } else {
                                //show data source filter
                                S.ajax.post('DataSources/RenderFilters', { key: key, filters:[] }, (form) => {
                                    //update list data with new data source
                                    field.find('.filter-settings .contents').html(form);
                                    field.find('.list-items').hide();
                                    field.find('.datasource-name').show();
                                    field.find('.datasource-name b').html(name);
                                    field.find('.tab-list-items').hide();
                                    field.find('.add-list-item').hide();
                                    field.find('.tab-filters').show();
                                    field.find('.tab-orderby').show();
                                    field.find('.tab-position').show();
                                    field.find('.input-pos-start input').val('1');
                                    field.find('.input-pos-start-query input').val('');
                                    field.find('.input-pos-length input').val('10');
                                    field.find('.input-pos-length-query input').val('');
                                    hidden.val('data-src=' + key);
                                    S.ajax.post('DataSources/RenderOrderByList', { key: key }, (form) => {
                                        //render order by form
                                        field.find('.orderby-settings .contents').html(form);
                                    });

                                }, (err) => {
                                        S.editor.error('', err.responseText);
                                });
                            }
                            S.editor.fields.change();
                        });

                    }, (err) => {
                            S.editor.error('', 'error');
                    }, true);
                    return false;
                },
                save: function (form, type) {
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
                                QueryName: filter.find('.filter-queryname input').val() ?? ''
                            });
                        }
                        for (var x = 0; x < subgroups.length; x++) {
                            result.Groups.push(collectFilters(container, $(subgroups[x])));
                        }
                        return result;
                    }

                    for (var i = 0; i < lists.length; i++) {
                        //find lists that use data sources
                        var container = $(lists[i]).parents('.content-field').first();
                        if (container.find('.list-items')[0].style.display != 'none') { continue; }

                        //generate content for list hidden field
                        var filters = [];
                        var groups = container.find('.filter-groups').children();
                        for (var y = 0; y < groups.length; y++) {
                            var filter = collectFilters(container, $(groups[y]));
                            if (filter.Elements.length > 0 || filter.Groups.length > 0) {
                                filters.push(filter);
                            }
                        }
                        //add filter to parts
                        var input = container.find('.input-field');
                        if (input.val().indexOf('data-src=') >= 0) {
                            var parts = input.val().split('|!|');
                            var json = 'filter=' + JSON.stringify(filters);
                            var found = false;
                            for (var x = 0; x < parts.length; x++) {
                                if (parts[x].indexOf('filter=') >= 0) {
                                    parts[x] = json;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) {
                                parts.push(json);
                            }

                            //add orderby to parts
                            json = "sort=" + JSON.stringify(container.find('.orderby-settings .orderby').map((i, a) => {
                                var orderby = $(a);
                                return {
                                    Column: orderby.attr('data-column'),
                                    Direction: parseInt(orderby.find('.orderby-direction select').val())
                                };
                            }));
                            found = false;
                            for (var x = 0; x < parts.length; x++) {
                                if (parts[x].indexOf('sort=') >= 0) {
                                    parts[x] = json;
                                    found = true;
                                    break;
                                }
                            }
                            if (!found) {
                                parts.push(json);
                            }

                            //add position to parts
                            function addPart(name, value, query) {
                                json = name + "=" + value + '|' + query;
                                found = false;
                                for (var x = 0; x < parts.length; x++) {
                                    if (parts[x].indexOf(name + '=') >= 0) {
                                        parts[x] = json;
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) {
                                    parts.push(json);
                                }
                            }

                            addPart('start', container.find('.input-pos-start input').val(), container.find('.input-pos-start-query input').val())
                            addPart('length', container.find('.input-pos-length input').val(), container.find('.input-pos-length-query input').val())

                            input.val(parts.join('|!|'));
                        }
                    }
                },
                select: function (key, title, callback, onload) {
                    var html = template_datasource_column.innerHTML;
                    S.ajax.post('DataSources/Columns', { key: key }, (response) => {
                        var columns = JSON.parse(response);
                        var options = '';
                        for (var x = 0; x < columns.length; x++) {
                            var c = columns[x].Name;
                            options += '<option value="' + c + '">' + c + '</options>';
                        }
                        html = html.replace('#options#', options);
                        S.editor.message.confirm(title, html, {}, (response) => {
                            callback(response);
                        });
                        if (onload) { onload(); }
                    });
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
                        var newgroup = $(children[children.length - 1]);
                        newgroup.find('input, select').on('input', (e) => {
                            S.editor.fields.change();
                        });
                        //S.drag.sort.add(container.find('.filter-settings .filter-groups, .filter-settings .sub-groups'), newgroup, S.editor.fields.custom.list.filters.sorted);
                    });
                },
                removeGroup: function (e) {
                    var target = $(e.target);
                    target.parents('.filter-group').first().remove();
                    S.editor.fields.change();
                },
                add: function (key, e) {
                    var target = $(e.target);
                    var container = target.parents('.filter-group').first();
                    S.editor.fields.custom.list.datasource.select(key, "Select Column to Filter By", (response) => {
                        if (response == true) {
                            S.ajax.post('DataSources/RenderFilter', { key: key, column: datasource_column.value }, (response) => {
                                var parent = container.find('.filters').first();
                                parent.append(response);
                                var children = parent.children();
                                var newfilter = $(children[children.length - 1]);
                                newfilter.find('input, select').on('input', (e) => {
                                    S.editor.fields.change();
                                });
                                //S.drag.sort.add(container.parents('.filter-settings').find('.filters'), newfilter, S.editor.fields.custom.list.filters.sorted);
                            });
                        }
                    });
                },
                remove: function (e) {
                    var target = $(e.target);
                    target.parents('.filter').first().remove();
                    S.editor.fields.change();
                },
                sorted: function () {
                    S.editor.fields.change();
                },
                toggleExtra: function (e) {
                    var target = $(e.target);
                    target.parents('.filter').find('.filter-queryname').toggle();
                }
            },
            orderby: {
                add: function (key, e) {
                    var target = $(e.target);
                    var container = target.parents('.orderby-settings .contents').first();
                    S.editor.fields.custom.list.datasource.select(key, "Select Column to Sort By", (response) => {
                        if (response == true) {
                            S.ajax.post('DataSources/RenderOrderBy', { key: key, column: datasource_column.value }, (response) => {
                                container.append(response);
                                var children = container.children();
                                var neworderby = $(children[children.length - 1]);
                                neworderby.find('input, select').on('input', (e) => {
                                    S.editor.fields.change();
                                });
                                S.drag.sort.add(container, container.find('.orderby'), S.editor.fields.custom.list.orderby.sorted);
                            });
                        }
                    }, () => {
                        //remove redundant options from datasource list
                        container.find('.orderby-input').each((i, a) => {
                            $('#datasource_column option[value="' + $(a).attr('data-column') + '"]').remove();
                        });
                    });
                },
                remove: function (e) {
                    var target = $(e.target);
                    target.parents('.orderby').first().remove();
                    S.editor.fields.change();
                },
                sorted: function () {
                    S.editor.fields.change();
                }
            },
            tab: function (id, e) {
                var classname = 'list-items';
                switch (id) {
                    case 'filters': classname = 'filter-settings'; break;
                    case 'orderby': classname = 'orderby-settings'; break;
                    case 'position': classname = 'position-settings'; break;
                }
                var container = $(e.target).parents('.content-field').first();
                var div = container.find('.' + classname);
                if (div.css('display') == 'none') {
                    //show tab
                    container.find('.tab-content').hide();
                    container.find('.tab-item > .row.hover').removeClass('selected');
                    container.find('.tab-' + id + ' > .row.hover').addClass('selected');
                    div.show();
                } else {
                    //hide tab
                    div.hide();
                    container.find('.tab-' + id + ' > .row.hover').removeClass('selected');
                }
            }
        }
    }
};

//add event listener for window resize stop to change height of all content field textarea inputs
S.editor.resize.stop.add('content-fields', S.editor.fields.resizeAll);

//add listener so when user saves content fields, it generates list data
S.editor.fields.listeners.save.add(S.editor.fields.custom.list.datasource.save);