S.editor.components = {

    load: () => {
        S('.components-menu .component-item').on('click', (e) => {
            var target = S(e.target);
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
        var menu = S('.components-menu');
        menu.removeClass('hide');
        S(document.body).off(hideMenu).on('click', hideMenu);

        function hideMenu(e) {
            if (S(e.target).parents('.tab-components').length <= 0 && 
                S(e.target).parents('.popup').length <= 0) {
                menu.addClass('hide');
                S('.component-configure').addClass('hide');
                S('.components-list').removeClass('hide');
                S(document.body).off(hideMenu);
            }
        }
    },

    configure: {
        show: (key, name, params) => {
            var html = S('#template_component_config').html()
                .replace('##key##', key).replace('##name##', name)
                .replace('##button-title##', 'Create ' + name);
            var htmlparam = S('#template_component_param').html();
            var fields = [];
            var idfield = S('#template_component_id').html();

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
                    var field = htmlparam.replace('##name##', param.Name)
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
                            fields.push(field.replace('##input##', '<select' + id + '>' +
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
                            fields.push(field.replace('##input##', '<div class="select-page">' +
                                '<div class="pad-right"><input type="text"' + id + '/></div>' +
                                '<div class="pad-top-sm"><button>Select Web Page...</button></div>' +
                                '</div>'));
                            break;
                    }
                }
            }
            S('.component-configure').html(html.replace('##fields##', fields.join(''))).removeClass('hide');
            S('.components-list').addClass('hide');
            S.editor.resizeWindow();

            //add event listeners
            S('.component-configure .button.cancel').on('click', () => {
                //hide component configuration and show component list
                hideConfigure();
            });
            S('.component-configure .select-image').on('click', (e) => {
                //show page references popup for image selection
            });
            S('.component-configure .select-page button').on('click', (e) => {
                //show file select popup for page selection
                S.editor.explorer.select('Select Web Page', 'Content/partials', '.html', (file) => {
                    S(e.target).parents('.select-page').first().find('input').val(file.replace('Content/', '').replace('content/', ''));
                });
            });
            S('.component-configure .button.apply').on('click', () => {
                //generate component instance in selected HTML page
                var inputs = S('.component-configure').find('input, select');
                var suffix = S('#component_id').length > 0 ? S('#component_id').val() : '';
                var componentId = S('#component_key').val();
                var mustache = '{{' + componentId +
                    (suffix && suffix != '' ? '-' + suffix : '');
                if (key == 'partial-view') {
                    //generate partial view
                    mustache = '{{' + suffix + ' "' + S('#param_page').val() + '"}}';
                } else if (key == 'special-vars') {
                    mustache = S('#param_var').val().replace(/\&qt\;/g, '"');
                } else {
                    //generate vendor HTML components
                    var paramlen = 0;
                    for (var x = 0; x < inputs.length; x++) {
                        var input = S(inputs[x]);
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
                S('.component-configure').addClass('hide');
                S('.components-list').removeClass('hide');
            }
        }
    }
};