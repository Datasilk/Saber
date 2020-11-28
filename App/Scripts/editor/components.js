S.editor.components = {
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
    }
};