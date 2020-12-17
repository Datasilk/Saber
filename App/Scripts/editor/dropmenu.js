S.editor.dropmenu = {
    show: function () {
        S('.editor-drop-menu, .bg-overlay').removeClass('hide');
        S(document.body).on('click', S.editor.dropmenu.hide);
    },

    hide: function (e) {
        var hide = false;
        if (e) {
            if (S(e.target).parents('.editor-drop-menu > div').length == 0) {
                hide = true;
            }
        } else { hide = true; }
        if (hide == true) {
            S('.editor-drop-menu, .bg-overlay').addClass('hide');
            S(document.body).off('click', S.editor.dropmenu.hide);
        }
    }
};