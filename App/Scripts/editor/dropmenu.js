S.editor.dropmenu = {
    show: function (e) {
        $(e.target).parents('li').first().find('.drop-menu').removeClass('hide');
        $(document.body).on('click', S.editor.dropmenu.hide);
    },

    hide: function (e) {
        var hide = false;
        if (e) {
            if ($(e.target).parents('.drop-menu').length == 0) {
                hide = true;
            }
        } else { hide = true; }
        if (hide == true) {
            $('.menu-bar .drop-menu').addClass('hide');
            $(document.body).off('click', S.editor.dropmenu.hide);
        }
    }
};