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
            var counter = $('.top-bar .notif-count');
            var count = parseInt(parts[0]);
            if (count < 1) {
                counter.hide();
            } else {
                counter.html(count).show();
            }
            S.editor.notifs.lastchecked = parts[1];
            $('.top-bar .notifs .drop-menu ul.menu').html(parts[2]);
            $('.top-bar .notifs ul.menu li > a').on('click', (e) => {
                //mark notification as read
                var target = $(e.target);
                if (target.attr('data-id') == null) {
                    target = target.parents('a').first();
                }
                console.log(target);
                var id = target.attr('data-id');
                S.ajax.post('Notifications/MarkAsRead', { id: id }, () => {
                    count -= 1;
                    if (count < 1) {
                        counter.hide();
                    } else {
                        counter.html(count).show();
                    }
                });
                target.parents('li').first().find('.unread').remove();
                S.editor.dropmenu.hide();
            });
        });
    }
};