S.editor.notifs = {
    //notifications
    init: function () {
        $('.notifs').on('click', S.editor.dropmenu.show);
    },

    update: function () {
        S.ajax.post('notifications/get', {}, (response) => {
            $('.notifs .drop-menu ul.menu').html(response);
        });
    }
};