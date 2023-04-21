S.editor.fields._load = function (file, show) {
    if (typeof file == 'object') { file = null; }
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
            $('.tab-sourcecode').show();
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
            S.editor.tabs.create('Content: ' + filepath, 'content-fields-' + fileid, { removeOnClose: true, selected: show !== false },
                () => { //onfocus
                    S.editor.tabs.show('content-fields-' + fileid);
                    $('ul.file-tabs > li.tab-content-fields').addClass('selected');
                    S.editor.filebar.update('Page Content for <a href="javascript:S.editor.explorer.open(\'' + file + '\')">' + file.replace('content/', '') + '</a>', 'icon-form-fields');
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
    S.editor.fields.render(file, lang, container, null, () => { });
};

S.editor.fields._popup = function (popup, partial, lang, title, fieldsdata, buttonTitle, submit, excludeFields, renderApi, callback) {
    //load content fields into popup modal
    S.editor.fields.render('content/' + partial, lang, '.box.popup.show', fieldsdata, () => {
        popup.find('form').append('<div class="row text-center"><div class="col"><button class="apply">' + buttonTitle + '</button></div>');
        S.popup.resize();
        popup.find('form').on('submit', (e) => {
            e.preventDefault();

            //make sure to presave all lists before saving
            S.editor.fields.presave();

            //populate form fields to submit
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
};

S.editor.fields.render = function (file, lang, container, fields, callback, ispopup, showlang, excludeFields, renderApi) {
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
                S.editor.resources.select(file ? 'wwwroot/images' : S.editor.path, 'images', true, "Select An Image", "Select Image", (results) => {
                    var parent = $(e.target).parents('.content-field');
                    var field = parent.find('.input-field');
                    var newpath = file ? '/images/' : '/' + S.editor.path + '/';
                    var src = newpath + results[0];
                    parent.find('.img').html('<div><img src="' + src + '"/></div>');
                    parent.find('.img').css({ 'background-image': 'url(' + src + ')' });
                    field.val(src);
                    S.editor.fields.change();
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

            if (callback) { callback(); }
        },
        function () { S.editor.error(); },
        true
    );
};

S.editor.fields.resize = function (e) {
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
};

S.editor.fields.resizeAll = function () {
    $('.has-content-fields form .input-field')
        .each(function (i, field) {
            S.editor.fields.resize({ target: field });
        });
};

S.editor.fields.change = function (e, file) {
    if (S.editor.visible == false) { return; }
    if (S.editor.fields.changed == false || file != null) {
        //enable save menu
        $('.item-save').removeClass('faded').removeAttr('disabled');
        if (!file) {
            S.editor.fields.changed = true;
        }
    }
    S.editor.fields.resize(e);
};

S.editor.fields.saveThenPreview = function () {
    S.editor.fields.save(S.editor.preview.show);
};

S.editor.fields.listeners = {
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
};

S.editor.fields.presave = function () {
    //update all input-fields in all content field forms before saving
    $('.has-content-fields').each((i, form) => {
        S.editor.fields.custom.list.datasource.save($(form));
    });
};

S.editor.fields.save = function (callback) {
    if (S.editor.fields.changed == false) { return; }
    var seltab = $('.tab-for-content-fields.selected > div');
    var pathid = seltab.attr('data-path');

    //execute all listener callbacks before saving
    S.editor.fields.listeners.save.items.forEach(a => a($('.' + pathid)));

    var fields = {};
    var path = seltab.attr('data-path-url') || S.editor.path;
    var texts = $('.' + pathid + ' form .input-field');
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
    S.ajax.post('ContentFields/Save', {
        path: path, fields: fields, language: $('.' + pathid + ' #lang').val()
    },
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
};

S.editor.fields.custom.list = {
    selected: null,
    init: function (container) {
        let section = $(container + ' form');
        S.accordion.load({}, () => { S.editor.resize.window(); });

        //event listener for all input fields
        section.find('input, select').on('input', (e) => {
            S.editor.fields.change();
        });

        //event listener for list drop down
        section.find('.list-lists select').off().on('input', S.editor.fields.custom.list.select);

        //event listener for single selection drop down
        if (section.find('.multi-selection').length == 0) {
            section.find('.single-selection select').off().on('input', S.editor.fields.custom.list.single.select);
            //event listener for list close button
            section.find('.list-items li .close-btn').off('click').on('click', S.editor.fields.custom.list.remove);
        } else {
            section.find('li[draggable="true"]').removeAttr('onclick').removeAttr('draggable');
            section.find('.list-items ul.list .close-btn').off('click').on('click', S.editor.fields.custom.list.multiselect.remove);
        }

        //drag & sort event listeners
        S.drag.sort.add(container + ' .list-items ul', container + ' .list-items li[draggable]', S.editor.fields.custom.list.drag);

        //S.drag.sort.add(container + ' .filter-settings .filter-groups, .filter-settings .sub-groups', container + ' .filter-settings .filter-group', S.editor.fields.custom.list.filters.sorted);
        //S.drag.sort.add(container + ' .filter-settings .filters', container + ' .filter-settings .filter', S.editor.fields.custom.list.filters.sorted);
        S.drag.sort.add(container + ' .orderby-settings .contents', container + ' .orderby', S.editor.fields.custom.list.orderby.sorted);

        //initialize all single selection select inputs
        S.editor.fields.custom.list.single.init();
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
        }
        field.find('.tab-list-items .icon-counter').html(data.length);

        //reset indexes
        var items = field.find('.list-items li');
        for (var y = 0; y < items.length; y++) {
            $(items[y]).attr('data-index', y + 1);
        }
        S.editor.fields.change();
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
                S.ajax.post('DataSources/AddRecord', { datasource: datasrc, columns: fields }, (response) => { });
                S.editor.fields.change();
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

                //add item to list in content fields tab
                var i = (index != null ? parseInt(index) : field.find('.list-items li').length) + 1;
                var ul = field.find('.list-items ul');
                var children = field.find('.list-items ul > li').map((i, a) => a.outerHTML);
                var child = $('#custom_field_list_item').html()
                    .replace('##onclick##', "S.editor.fields.custom.list.edit(event, '##title##', '##key##', '##partial##', '##lang##', '##container##')")
                    .replace(/\#\#label\#\#/g, key != '' && fields[key] && fields[key] != '' ? fields[key] : 'List Item #' + i)
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
            }
            S.popup.hide(popup);
            S.editor.fields.custom.list.init(container);
            return false;
        }, null, renderApi);
        return false;
    },
    drag: function (e) {
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
    },
    select: function (e) {
        //used when list component contains one or more list components within the partial view used by your list component
        //select list component from drop down and update content field sections based on selected list key
        var target = $(e.target);
        var container = target.parents('.content-field').first();
        var selected = container.find('.list-lists select').val();
        var key = S.editor.fields.custom.list.datasource.key(container);

        //save selected list settings before selecting a different list
        S.editor.fields.custom.list.datasource.save(container);
        container.find('.list-lists input').val(selected);

        //load settings for list
        var settings = S.editor.fields.custom.list.datasource.settings(container);
        var list = settings[selected] ?? { p: {}, f: [], o: [] }; //position, filters, orderby

        //populate posiion settings
        var pos = list.p;
        if (!pos) {
            pos = { s: 1, l: 10, sq: '', lq: '' };
        }
        container.find('.input-pos-start input').val(pos.s);
        container.find('.input-pos-start-query input').val(pos.sq);
        container.find('.input-pos-length input').val(pos.l);
        container.find('.input-pos-length-query input').val(pos.lq);

        //load filters, orderby, and position values
        S.ajax.post('DataSources/RenderFilters', { key: key, filters: list.f }, (form) => {
            //update list data with new data source
            container.find('.filter-settings .contents').html(form);
        });
        S.ajax.post('DataSources/RenderOrderByList', { key: key, orderby: list.o }, (response) => {
            var contents = container.find('.orderby-settings .contents');
            contents.html(response);
            S.drag.sort.add(contents, contents.find('.orderby'), S.editor.fields.custom.list.orderby.sorted);
        });
    },
    single: {
        select: function (e) {
            var target = $(e.target);
            var container = target.parents('.content-field').first();
            var selected = container.find('.single-selection select').val();
            var input = container.find('input.input-field');
            var parts = input.val().split('|!|');
            var newparts = [];
            for (var x = 0; x < parts.length; x++) {
                if (parts[x].indexOf('lists=') >= 0 ||
                    parts[x] == 'add' ||
                    parts[x].indexOf('selected=') == 0) {
                } else {
                    newparts.push(parts[x]);
                }
            }
            newparts.push('selected=' + selected);
            input.val(newparts.filter(a => a != '').join('|!|'));
            S.editor.fields.custom.list.datasource.save(container);
        },
        init: function () {
            $('.list-component-field .single-selection select').each((i, a) => {
                var container = $(a).parents('.content-field').first();
                if (container.find('.multi-selection').length > 0) { return; }
                a.dispatchEvent(new Event('input')); //force single.select(e) method call
            })

        }
    },
    multiselect: {
        add: function (e) {
            e.stopPropagation();
            e.preventDefault();
            var target = $(e.target);
            var container = target.parents('.content-field').first();
            var select = container.find('.single-selection select');
            var value = select.val();
            var input = container.find('input.input-field');
            var parts = input.val().split('|!|');
            var newparts = [];
            var allIds = [];
            for (var x = 0; x < parts.length; x++) {
                if (parts[x].indexOf('lists=') >= 0 ||
                    parts[x] == 'add') {
                } else if (parts[x].indexOf('selected=') == 0) {
                    allIds = parts[x].replace('selected=', '').split(',');
                } else {
                    newparts.push(parts[x]);
                }
            }
            allIds.push(value);
            newparts.push('selected=' + allIds.join(','));
            input.val(newparts.filter(a => a != '').join('|!|'));
            S.editor.fields.custom.list.datasource.save(container);
            //add selected item to list
            var template = container.find('li.template');
            var option = select.find('option[value="' + value + '"]');
            var name = option.html();
            template.find('span').first().html(name);
            var list = container.find('.list-items ul.list');
            list.append('<li data-index="' + value + '">' + template.html() + '</li>');
            list.find('.close-btn').off('click').on('click', S.editor.fields.custom.list.multiselect.remove);
            //finally, remove option
            container.find('.single-selection option[value="' + value + '"]').remove();
            container.find('.icon-counter').html(allIds.length);
        },
        remove: function (e) {
            var target = $(e.target);
            var container = target.parents('.content-field').first();
            var item = target.parents('li').first();
            var id = parseInt(item.attr('data-index'));
            var name = item.find('span').html();
            var options = container.find('.single-selection option');
            if (options.length > 0) {
                //add option in correct position within select list
                for (var x = 0; x < options.length; x++) {
                    if (parseInt($(options[x]).val()) > id) { break; }
                }
                if (x == options.length) {
                    $(options[x - 1]).after('<option value="' + id + '">' + name + '</option>');
                } else {
                    $(options[x]).before('<option value="' + id + '">' + name + '</option>');
                }
            } else {
                //add option into empty select
                container.find('.single-selection select').append('<option value="' + id + '">' + name + '</option>');
            }
            item.remove();

            //remove item from hidden field
            var input = container.find('input.input-field');
            var parts = input.val().split('|!|');
            var newparts = [];
            var allIds = [];
            for (var x = 0; x < parts.length; x++) {
                if (parts[x].indexOf('lists=') >= 0 ||
                    parts[x] == 'add') {
                } else if (parts[x].indexOf('selected=') == 0) {
                    allIds = parts[x].replace('selected=', '').split(',');
                } else {
                    newparts.push(parts[x]);
                }
            }
            var newIds = [];
            for (var x = 0; x < allIds.length; x++) {
                if (allIds[x] != id.toString()) { newIds.push(allIds[x]); }
            }
            newparts.push('selected=' + newIds.join(','));
            input.val(newparts.filter(a => a != '').join('|!|'));
            S.editor.fields.custom.list.datasource.save(container);
            container.find('.icon-counter').html(newIds.length);
        }
    },
    datasource: {
        list: function (e, title) {
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
                        field.find('.list-lists').hide();
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
                        //show data source UI elements
                        S.ajax.post('DataSources/Relationships', { key: key, filters: [] }, (response) => {
                            var relationships = JSON.parse(response);
                            var listname = field.find('.col.field').first().html().replace('List: ', '');
                            var html = '<option value="' + key + '">' + listname + '</option>\n';
                            for (var x = 0; x < relationships.length; x++) {
                                var item = relationships[x];
                                html += '<option value="' + item.ChildKey + '">' +
                                    S.util.str.Capitalize(item.ListComponent.replace('list-', '').replace('-', ' ')) + '</option>\n';
                            }
                            field.find('.list-lists select').html(html);
                            field.find('.list-lists input').val(key);
                            field.find('.list-lists').show();
                        });
                        S.ajax.post('DataSources/RenderFilters', { key: key, filters: [] }, (form) => {
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
        key: function (container) {
            var input = container.find('input.input-field').val();
            if (input.indexOf('data-src=') >= 0) {
                var parts = input.split('|!|');
                for (var x = 0; x < parts.length; x++) {
                    if (parts[x].indexOf('data-src=') >= 0) {
                        return parts[x].replace('data-src=', '');
                    }
                }
            }
            return {};
        },
        settings: function (container) {
            var input = container.find('input.input-field').val();
            if (input.indexOf('data-src=') >= 0) {
                var parts = input.split('|!|');
                for (var x = 0; x < parts.length; x++) {
                    if (parts[x].indexOf('lists=') >= 0) {
                        return JSON.parse(parts[x].replace('lists=', ''));
                    }
                }
            }
            return {};
        },
        save: function (form) {
            var list_elems = form.find('.list-component-field');

            function collectFilters(container, elem) {
                //generate FilterGroup object
                var result = {
                    m: parseInt(container.find('.match-type select').val() ?? '0'), //match
                    e: [], //elements
                    g: [] //groups
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
                    result.e.push({
                        c: filter.attr('data-column'), //column
                        m: parseInt(filter.find('.filter-match select').val() ?? '0'), //match
                        v: val, //value
                        qn: filter.find('.filter-queryname input').val() ?? '' //query name
                    });
                }
                for (var x = 0; x < subgroups.length; x++) {
                    result.g.push(collectFilters(container, $(subgroups[x]))); //groups
                }
                return result;
            }

            for (var i = 0; i < list_elems.length; i++) {
                //find lists that use data sources
                var container = $(list_elems[i]).parents('.content-field').first();
                if (container.find('.list-items')[0].style.display != 'none') { continue; }

                //generate serialized object for list hidden field
                var key = container.find('.list-lists input').val();
                var input = container.find('.input-field');
                var lists = {};
                var list = {};
                var singleselect = container.find('.single-selection select');
                var multiselect = container.find('.multi-selection').length > 0;

                //get list type
                if (singleselect.length == 0) {
                    //get filter data for list
                    var filters = [];
                    var groups = container.find('.filter-groups').children();
                    for (var y = 0; y < groups.length; y++) {
                        var filter = collectFilters(container, $(groups[y]));
                        if (filter.e.length > 0 || filter.g.length > 0) {
                            filters.push(filter);
                        }
                    }
                    list.f = filters;

                    //get orderby data
                    list.o = container.find('.orderby-settings .orderby').map((i, a) => {
                        var orderby = $(a);
                        return {
                            c: orderby.attr('data-column'), //column
                            d: parseInt(orderby.find('.orderby-direction select').val()) //direction
                        };
                    });

                    //get position data
                    list.p = {
                        s: parseInt(container.find('.input-pos-start input').val()), //start
                        sq: container.find('.input-pos-start-query input').val(), //start query
                        l: parseInt(container.find('.input-pos-length input').val()), //length
                        lq: container.find('.input-pos-length-query input').val() //length query
                    };
                    if (list.p.s == null) { list.p.s = 1; }
                    if (list.p.l == null) { list.p.l = 10; }
                }
                if (input.val().indexOf('data-src=') >= 0) {
                    //save parts to hidden input
                    var parts = input.val().split('|!|');
                    var selected = '';
                    var found = false;
                    newparts = [];
                    for (var x = 0; x < parts.length; x++) {
                        if (parts[x].indexOf('lists=') >= 0) {
                            lists = JSON.parse(parts[x].replace('lists=', ''));
                            found = true;
                            break;
                        } else if (parts[x].indexOf('data-src=') >= 0) {
                        } else if (parts[x].indexOf('selected=') >= 0) {
                            selected = parts[x].split('selected=')[1];
                            parts[x] = '';
                        } else { parts[x] = ''; }
                    }
                    if (singleselect.length > 0) {
                        //remove lists object
                        if (found) { parts[x] = ''; }
                        //add single select value
                        if (!multiselect) {
                            parts.push('single');
                            parts.push('selected=' + singleselect.val());
                        } else {
                            parts.push('selected=' + selected);
                        }
                    } else {
                        //add list settings for data source & all relationships
                        lists[key] = list;
                        if (found) {
                            parts[x] = 'lists=' + JSON.stringify(lists);
                        } else {
                            parts.push('lists=' + JSON.stringify(lists));
                        }
                    }
                    input.val(parts.filter(a => a != '').join('|!|'));
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
            S.ajax.post('DataSources/RenderFilterGroups', { key: key, groups: [{ e: [], g: [] }], depth: depth }, (response) => {
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
            var dskey = target.parents('.vendor-input').find('.list-lists select').val() ?? key;
            if (dskey == '') { dskey = key; }
            S.editor.fields.custom.list.datasource.select(dskey, "Select Column to Filter By", (response) => {
                if (response == true) {
                    S.ajax.post('DataSources/RenderFilter', { key: dskey, column: datasource_column.value }, (response) => {
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
            container.find('.no-orderby').remove();
            var dskey = target.parents('.vendor-input').find('.list-lists select').val() ?? key;
            S.editor.fields.custom.list.datasource.select(dskey, "Select Column to Sort By", (response) => {
                if (response == true) {
                    S.ajax.post('DataSources/RenderOrderBy', { key: dskey, column: datasource_column.value }, (response) => {
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
};

//add listener so when user saves content fields, it generates list data
S.editor.fields.listeners.save.add(S.editor.fields.custom.list.datasource.save);