S.editor.dropmenu = {
    show: function () {
        $('.editor-drop-menu, .bg-overlay').removeClass('hide');
        $(document.body).on('click', S.editor.dropmenu.hide);
    },

    hide: function (e) {
        var hide = false;
        if (e) {
            if ($(e.target).parents('.editor-drop-menu > div').length == 0) {
                hide = true;
            }
        } else { hide = true; }
        if (hide == true) {
            $('.editor-drop-menu, .bg-overlay').addClass('hide');
            $(document.body).off('click', S.editor.dropmenu.hide);
        }
    }
};