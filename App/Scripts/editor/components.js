S.editor.components = {

    load: () => {
        $('.components-menu .component-item').on('click', (e) => {
            var target = $(e.target);
            if (!target.hasClass('component-item')) {
                target = target.parents('.component-item').first();
            }
            var key = target.attr('data-key');
            var name = target.find('h4').html().trim();
            S.editor.components.configure.show(key, name);
        });
    },

    show: (e) => {
        var target = $(e.target);
        if (target.parents('.tab-button').length > 0) {
            var menu = $('.components-menu');
            menu.removeClass('hide');
            S.editor.components.resize();
            $(document.body).off(S.editor.components.hide).on('click', S.editor.components.hide);
        }
    },

    hide: function (e) {
        if (e != null) {
            var target = $(e.target);
            if (target.parents('.components-menu').length > 0 ||
                target.hasClass('components-menu') ||
                target.parents('.popup').length > 0 ||
                target.hasClass('popup')) {
                return;
            }
        }
        var menu = $('.components-menu');
        menu.addClass('hide');
    },

    resize: function () {
        var menu = $('.components-menu');
        var win = S.window.pos();
        var pos = menu.offset();
        menu.find('.scroller').css({ 'max-height': (win.h - pos.top - 30) + 'px' });
    },

    configure: {
        show: (key, name) => {
            S.ajax.post('Components/GetParameters', { key: key }, (params) => {
                var html = $('#template_component_config').html()
                    .replace('##key##', key).replace('##name##', name)
                    .replace('##button-title##', 'Create ' + name);
                var htmlparam = $('#template_component_param').html();
                var htmlparamlist = $('#template_component_param_list').html();
                var fields = [];
                var idfield = $('#template_component_id').html();

                if (key == 'partial-view') {
                    html = html.replace('##id-field##', idfield).replace('##optional##', '');
                } else if (key == 'special-vars') {
                    html = html.replace('##id-field##', '');
                } else {
                    html = html.replace('##id-field##', idfield).replace('##optional##', '<span class="faded">optional</span>');
                }
                if (params != null && params.length > 0) {
                    for (var x = 0; x < params.length; x++) {
                        var param = params[x];
                        var id = ' id="param_' + param.Key + '"';
                        var clss = ' class="param' + (!param.List == true ? ' input-field' : '') + '"';
                        var spellchk = ' spellcheck="false"';
                        var defaultVal = param.DefaultValue ?? '';
                        var required = param.Required ?? false;
                        var title = ' title="' + param.Description + '"';
                        var field = param.List == true ? htmlparamlist.replace('##add-list-item##', param.AddItemJs != '' ? ' onclick="' + param.AddItemJs + '"' : '').replace('##id##', id)
                            : htmlparam;
                        field = field.replace('##name##', param.Name)
                            .replace('##required##', !required ? '<span class="faded">optional</span>' : '');
                        switch (param.DataType) {
                            case 0: //text
                                fields.push(field.replace('##input##', '<input type="text"' + id + clss + spellchk + ' ' +
                                    (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                                break;
                            case 1: //number
                                fields.push(field.replace('##input##', '<input type="number"' + id + clss + spellchk + ' ' +
                                    (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                                break;
                            case 2: //boolean
                                fields.push(field.replace(param.Name, '').replace('##input##', '<input type="checkbox"' + id + clss + '/>' +
                                    '<label for="' + 'param_' + param.Key + '"' + title + '>' + param.Name + '</label>'));
                                break;
                            case 3: //list
                                fields.push(field.replace('##input##', '<select' + id + clss + title + '>' +
                                    (param.ListOptions ? param.ListOptions.join('').replace(/\&q\;/g, '"') : '') +
                                    '</select>'));
                                break;
                            case 4: //date
                                fields.push(field.replace('##input##', '<input type="date"' + id + clss + ' ' +
                                    (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                                break;
                            case 5: //datetime
                                fields.push(field.replace('##input##', '<input type="datetime-local"' + id + clss + ' ' +
                                    (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                                break;
                            case 6: //currency
                                fields.push(field.replace('##input##', '<input type="text"' + id + clss + spellchk + ' ' +
                                    (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                                break;
                            case 7: //image
                                fields.push(field.replace('##input##', '<button class="select-image">Select Image...</button>' +
                                    '<input type="hidden"' + id + clss + "/>"));
                                break;
                            case 8: //web page
                                fields.push(field.replace('##input##', '<div class="row select-page">' +
                                    '<div class="col text-input"><input type="text"' + id + clss + spellchk + '/></div>' +
                                    '<div class="col right"><button>...</button></div>' +
                                    '</div>'));
                                break;
                            case 9: //partial view
                                fields.push(field.replace('##input##', '<div class="row select-partial">' +
                                    '<div class="col"><input type="text"' + id + clss + spellchk + '/></div>' +
                                    '<div class="col"><button>...</button></div>' +
                                    '</div>'));
                                break;
                        }
                    }
                }
                $('.component-configure').html(html.replace('##fields##', fields.join('<hr/>') + '<hr/>')).removeClass('hide');
                $('.components-list').addClass('hide');

                //set up accordion functionality (add/sort/remove list items)
                S.accordion.load({}, () => { S.editor.resize.window(); });
                $('.component-configure .accordion .title .add-list-item').filter((i, a) => $(a).attr('onclick') == '').on('click', (e) => {
                    //show new param form
                    e.cancelBubble = true;
                    var parent = $(e.target).parents('.component-param').first();
                    parent.find('.accordion').first().removeClass('expanded');
                    parent.find('.add-list-item, .expander').addClass('hide');
                    parent.find('.field-form, .accept-item, .cancel-item').removeClass('hide');
                });
                $('.component-configure .accordion .title .accept-item').on('click', (e) => {
                    S.editor.components.accordion.accept(e);
                });
                $('.component-configure .accordion .title .cancel-item').on('click', (e) => {
                    //cancel creating param value item
                    e.cancelBubble = true;
                    var parent = $(e.target).parents('.component-param').first();
                    parent.find('.add-list-item, .expander').removeClass('hide');
                    parent.find('.field-form, .accept-item, .cancel-item').addClass('hide');
                });

                //add event listeners
                $('.component-configure .button.cancel').on('click', () => {
                    //hide component configuration and show component list
                    hideConfigure();
                });
                $('.component-configure .select-image').on('click', (e) => {
                    //show page references popup for image selection
                });
                $('.component-configure .select-page button').on('click', (e) => {
                    //show file select popup for page selection
                    S.editor.explorer.select('Select HTML File', 'Content/partials', '.html', (file) => {
                        $(e.target).parents('.select-page').first().find('input').val(file.replace('Content/', '').replace('content/', ''));
                    });
                });
                $('.component-configure .select-partial button').on('click', (e) => {
                    //show file select popup for partial view selection
                    S.editor.explorer.select('Select Partial View', 'Content/partials', '.html', (file) => {
                        $(e.target).parents('.select-partial').first().find('input').val(file.replace('Content/', '').replace('content/', ''));
                    });
                });
                $('.component-configure .button.apply').on('click', () => {
                    //generate component instance in selected HTML page
                    var inputs = $('.component-configure').find('.input-field');
                    var suffix = $('#component_id').length > 0 ? $('#component_id').val() : '';
                    var componentId = $('#component_key').val();
                    var mustache = '{{' + componentId +
                        (suffix && suffix != '' ? '-' + suffix : '');
                    if (key == 'partial-view') {
                        //generate partial view
                        mustache = '{{' + suffix + ' "' + $('#param_page').val() + '"}}';
                    } else if (key == 'special-vars') {
                        if ($('#param_var').val() == '{{#}}') {
                            mustache = '{{# My Title}}';
                        } else {
                            mustache = $('#param_var').val().replace(/\&qt\;/g, '"');
                        }
                    } else {
                        //generate vendor HTML components
                        var paramlen = 0;
                        for (var x = 0; x < inputs.length; x++) {
                            var input = $(inputs[x]);
                            var id = input.attr('id');
                            if (!id || id == 'component_key' || id == 'component_id') { continue; }
                            id = id.replace('param_', '');
                            var value = input.val();
                            if (input.attr('type') == 'checkbox') {
                                value = input[0].checked ? '1' : '0';
                            }
                            if (id && id != '' && value && value != '') {
                                mustache += (paramlen > 0 ? ',' : '') + ' ' + id + ':"' + value + '"';
                            }
                            paramlen++;
                        }
                        mustache += '}}';
                    }
                    if (S.editor.type == 0) {
                        //monaco editor
                        var editor = S.editor.instance;
                        editor.trigger('keyboard', 'type', { text: mustache });
                    }
                    hideConfigure();
                });

                function hideConfigure() {
                    $('.component-configure').addClass('hide');
                    $('.components-list').removeClass('hide');
                }
            }, (err) => { }, true);
        }
    },

    accordion: {
        accept: function(e){
            //create param value item and add to list of param values
            e.cancelBubble = true;
            var parent = $(e.target).parents('.component-param').first();
            var param = parent.find('.field-form .param');
            var val = param.val();
            if (param.attr('type') == 'checkbox') {
                val = param[0].checked ? 'True' : 'False';
                param[0].checked = false;
            } else {
                param.val('');
            }

            //modify input field
            let input = parent.find('.input-field');
            var vals = input.val() != '' ? input.val().split('|') : [];
            vals.push(val);
            input.val(vals.join('|'));

            //add list item
            var customfield = $('#custom_field_list_item').html();
            var span = $('.component-configure .accordion .contents > span.faded');
            var contents = $('.component-configure .accordion .contents');
            let ul = contents.find('ul');
            if (span.length > 0) {
                span.remove();
                contents.append('<ul class="list"></ul>');
                ul = contents.find('ul');
            }
            //get all custom fields in list
            var items = ul.find('li').map((i, a) => a.outerHTML);
            ul.html('');
            ul.append(items.join('') + customfield
                .replace(/\#\#label\#\#/g, val)
                .replace('##index##', '')
                .replace('##onclick##', '')
            );

            //add event listeners for list items
            ul.find('li .close-btn').on('click', (e) => {
                e.cancelBubble = true;
                var vals2 = input.val() != '' ? input.val().split('|') : [];
                var li = $(e.target).parents('li').first();
                var lis = ul.find('li');
                for (var x = 0; x < lis.length; x++) {
                    if (lis[x] == li[0]) {
                        vals2.splice(x, 1);
                        break;
                    }
                }
                input.val(vals2.join('|'));
                li.remove();
            });

            //hide form
            var parent = $(e.target).parents('.component-param').first();
            parent.find('.add-list-item, .expander').removeClass('hide');
            parent.find('.field-form, .accept-item, .cancel-item').addClass('hide');
            parent.find('.accordion').addClass('expanded');
        }
    },

    partials: {
        show: function (e, callback) {
            e.cancelBubble = true;
            S.editor.explorer.select('Select Partial View', 'Content/partials', '.html', (file) => {
                $(e.target).parents('.component-param').find('.select-partial input').val(file.replace('Content/', '').replace('content/', ''));
                if (callback) {
                    callback(e);
                }
            });
        }
    }
};

$(window).on('resize', S.editor.components.resize);