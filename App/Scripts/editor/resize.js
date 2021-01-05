
S.editor.resize = {
    stop: {
        timer: null, 
        listeners: [],

        add: function (id, callback) {
            var listeners = S.editor.resize.stop.listeners;
            for (var x = 0; x < listeners.length; x++) {
                if (listeners[x].id == id) {
                    S.editor.resize.stop.listeners[x].callback = callback;
                }
            }
            S.editor.resize.stop.listeners.push({ id: id, callback: callback });
        },

        remove: function (id) {
            var listeners = S.editor.resize.stop.listeners;
            for (var x = 0; x < listeners.length; x++) {
                if (listeners[x].id == id) {
                    S.editor.resize.stop.listeners.splice(x, 1);
                }
            }
        },

        stopped: function () {
            var listeners = S.editor.resize.stop.listeners;
            for (var x = 0; x < listeners.length; x++) {
                listeners[x].callback();
            }
        }
    },

    window: function () {
        var win = S.window.pos();
        var sect = S.editor.sect;
        var div = S.editor.div;
        var browser = S.editor.divBrowser;
        var fields = S.editor.divFields;
        var pos = sect.offset();
        var pos2 = browser.offset();
        div.css({ height: '' });
        if (pos.top == 0) {
            pos = fields.offset();
        }
        $('.editor > div > .sections > .tab').css({ height: win.h - pos.top });
        $('.file-browser').css({ height: win.h - pos2.top });
        $('.component-configure .scroller').css({ 'max-height': (win.h - 200) + 'px' });

        if (S.editor.resize.stop.timer != null) {
            clearTimeout(S.editor.resize.stop.timer);
        }
        S.editor.resize.stop.timer = setTimeout(S.editor.resize.stop.stopped, 250);
    }
};