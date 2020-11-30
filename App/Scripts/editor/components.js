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
        //if (!menu.hasClass('hide')) {
        //    $(document.body).off(hideMenu);
        //    menu.addClass('hide');
        //    return;
        //}
        menu.removeClass('hide');
        $(document.body).off(hideMenu).on('click', hideMenu);

        function hideMenu(e) {
            if ($(e.target).parents('.tab-components').length <= 0) {
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
            var fields = [];
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
                            fields.push(field.replace('##input##', '<select' + id + '>' +
                                (param.ListOptions ? param.ListOptions.map(a => '<option value="' + a + '">' + a + '</option>').join('') : '') +
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
                            fields.push(field.replace('##input##', '<button class="select-image" value="Select Image...">' +
                                '<input type="hidden"' + id + "/>"));
                            break;
                    }
                }
            }
            $('.component-configure').html(html.replace('##fields##', fields.join(''))).removeClass('hide');
            $('.components-list').addClass('hide');

            //add event listeners
            $('.component-configure .button.cancel').on('click', () => {
                //hide component configuration and show component list
                hideConfigure();
            });
            $('.component-configure .select-image').on('click', (e) => {
                //show page references popup for image selection
            });
            $('.component-configure .button.apply').on('click', () => {
                //generate component instance in selected HTML page
                var inputs = $('.component-configure').find('input, select');
                console.log(inputs);
                var suffix = $('#component_id').val();
                var mustache = '{{' + $('#component_key').val() +
                    (suffix && suffix != '' ? '-' + suffix : '');
                var paramlen = 0;
                for (var x = 0; x < inputs.length; x++) {
                    var input = $(inputs[x]);
                    console.log(input);
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