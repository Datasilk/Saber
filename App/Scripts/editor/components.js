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
        show: (key, name) => {
            var html = $('#template_htmlcomponent').html();
            S.popup.show('Configure ' + name, html);
        }
    }
};