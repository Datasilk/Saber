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
        if (!menu.hasClass('hide')) {
            $(document.body).off(hideMenu);
            menu.addClass('hide');
            return;
        }
        menu.removeClass('hide');
        $(document.body).off(hideMenu).on('click', hideMenu);

        function hideMenu(e) {
            if ($(e.target).parents('.tab-components').length <= 0) {
                menu.addClass('hide');
                $(document.body).off(hideMenu);
            }
        }
    },

    configure: {
        show: (key, name, params) => {
            var html = $('#template_htmlcomponent').html()
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
                                '<label for="' + 'param_' + key + '"' + title + '>' + param.Name + '</label>'));
                            break;
                        case 3: //list

                            break;
                        case 4: //date

                            break;
                        case 5: //datetime

                            break;
                        case 6: //currency

                            break;
                        case 7: //image

                            break;
                    }
                }
            }
            S.popup.show('Configure ' + name, html.replace('##fields##', fields.join('')));
        }
    }
};