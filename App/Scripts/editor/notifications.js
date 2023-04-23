S.editor.notifs = {
    lastchecked: null,

    //notifications
    init: function () {
        $('.top-bar .notifs').on('click', (e) => { S.editor.dropmenu.show(e, '.notifs'); });
        S.editor.notifs.update();
    },

    update: function (lastchecked) {
        S.ajax.post('Notifications/RenderList', { lastChecked: lastchecked }, (response) => {
            var parts = response.split('|!|');
            if (parseInt(parts[0]) < 1) {
                $('.top-bar .notif-count').hide();
            } else {
                $('.top-bar .notif-count').html(parts[0]).show();
            }
            S.editor.notifs.lastchecked = parts[1];
            $('.top-bar .notifs .drop-menu ul.menu').html(parts[2]);
        });
    }
};