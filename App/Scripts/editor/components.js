S.editor.components = {

    load: () => {
        $('.components-menu .component-item').on('click', (e) => {
            var target = $(e.target);
            if (!target.hasClass('component-item')) {
                target = target.parents('.component-item').first();
            }
            var key = target.attr('data-key');
            var name = target.find('h4').html().trim();
            var params = JSON.parse(target.attr('data-params'));
            S.editor.components.configure.show(key, name, params);
        });
    },

    show: () => {
        var menu = $('.components-menu');
        menu.removeClass('hide');
        $(document.body).off(hideMenu).on('click', hideMenu);

        function hideMenu(e) {
            if ($(e.target).parents('.tab-components').length <= 0 && 
                $(e.target).parents('.popup').length <= 0) {
                menu.addClass('hide');
                $('.component-configure').addClass('hide');
                $('.components-list').removeClass('hide');
                $(document.body).off(hideMenu);
            }
        }
    },

    configure: {
        show: (key, name, params) => {
            var html = $('#template_component_config').html()
                .replace('##key##', key).replace('##name##', name)
                .replace('##button-title##', 'Create ' + name);
            var htmlparam = $('#template_component_param').html();
            var htmlparamadd = $('#template_component_param_add').html();
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
                    var defaultVal = param.DefaultValue ?? '';
                    var required = param.Required ?? false;
                    var title = ' title="' + param.Description + '"';
                    var field = param.List == true ? htmlparamlist : htmlparam;
                    field = field.replace('##name##', param.Name)
                        .replace('##required##', !required ? '<span class="faded">optional</span>' : '');
                    switch (param.DataType) {
                        case 0: //text
                            fields.push(field.replace('##input##', '<input type="text"' + id + ' ' +
                                (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                            break;
                        case 1: //number
                            fields.push(field.replace('##input##', '<input type="number"' + id + ' ' +
                                (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                            break;
                        case 2: //boolean
                            fields.push(field.replace(param.Name, '').replace('##input##', '<input type="checkbox"' + id + '/>' +
                                '<label for="' + 'param_' + param.Key + '"' + title + '>' + param.Name + '</label>'));
                            break;
                        case 3: //list
                            console.log(param.ListOptions ? param.ListOptions.join('').replace(/\&q\;/g, '"') : '');
                            fields.push(field.replace('##input##', '<select' + id + title + '>' +
                                (param.ListOptions ? param.ListOptions.join('').replace(/\&q\;/g, '"') : '') +
                                '</select>'));
                            break;
                        case 4: //date
                            fields.push(field.replace('##input##', '<input type="date"' + id + ' ' +
                                (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                            break;
                        case 5: //datetime
                            fields.push(field.replace('##input##', '<input type="datetime-local"' + id + ' ' +
                                (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                            break;
                        case 6: //currency
                            fields.push(field.replace('##input##', '<input type="text"' + id + ' ' +
                                (defaultVal != '' ? 'value="' + defaultVal + '"' : '') + title + '/>'));
                            break;
                        case 7: //image
                            fields.push(field.replace('##input##', '<button class="select-image">Select Image...</button>' +
                                '<input type="hidden"' + id + "/>"));
                            break;
                        case 8: //web page
                            fields.push(field.replace('##input##', '<div class="row select-page">' +
                                '<div class="col"><input type="text"' + id + '/></div>' +
                                '<div class="col right pad-top-sm"><button>Select Web Page...</button></div>' +
                                '</div>'));
                            break;
                        case 9: //partial view
                            fields.push(field.replace('##input##', '<div class="row select-partial">' +
                                '<div class="row"><input type="text"' + id + '/></div>' +
                                '<div class="row text-right pad-top-sm"><button>Select Partial View...</button></div>' +
                                '</div>'));
                            break;
                    }
                }
            }
            $('.component-configure').html(html.replace('##fields##', fields.join(''))).removeClass('hide');
            $('.components-list').addClass('hide');
            S.accordion.load({}, () => { S.editor.resizeWindow(); });
            $('.component-configure .accordion .title .add-list-item').on('click', (e) => {
                //show new param form
                e.cancelBubble = true;
                var parent = $(e.target).parents('.component-param').first();
                parent.find('.accordion').first().removeClass('expanded');
                parent.find('.add-list-item, .expander').addClass('hide');
                parent.find('.field-form, .accept-item, .cancel-item').removeClass('hide');
            });
            $('.component-configure .accordion .title .accept-item').on('click', (e) => {
                //create param value item and add to list of param values
                e.cancelBubble = true;
                var parent = $(e.target).parents('.component-param').first();
                parent.find('.add-list-item, .expander').removeClass('hide');
                parent.find('.field-form, .accept-item, .cancel-item').addClass('hide');
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
                S.editor.explorer.select('Select Web Page', 'Content/partials', '.html', (file) => {
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
                var inputs = $('.component-configure').find('input, select');
                var suffix = $('#component_id').length > 0 ? $('#component_id').val() : '';
                var componentId = $('#component_key').val();
                var mustache = '{{' + componentId +
                    (suffix && suffix != '' ? '-' + suffix : '');
                if (key == 'partial-view') {
                    //generate partial view
                    mustache = '{{' + suffix + ' "' + $('#param_page').val() + '"}}';
                } else if (key == 'special-vars') {
                    mustache = $('#param_var').val().replace(/\&qt\;/g, '"');
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
        }
    }
};