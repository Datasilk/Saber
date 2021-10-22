S.editor.events = {
    listeners: [],
    list: [],

    listen: function (event, func) {
        console.log('listen to ' + event);
        var events = S.editor.events.listeners.filter(a => a.event == event);
        if (events.length >= 1) {
            events[0].func = func;
            return;
        }
        S.editor.events.listeners.push({ event: event, func: func });
    },

    add: function (event) {
        console.log('add ' + event + ' listener');
        if (S.editor.events.list.filter(a => a.event == event).length == 0) {
            S.editor.events.list.push(event);
        }
    },

    remove: function (event) {
        var items = S.editor.events.list.filter(a => a.event == event);
        items[0].parentNode.remove(items[0]);
    },

    broadcast: function (event) {
        console.log('broadcast for ' + event);
        var item = S.editor.events.list.filter(a => a.event == event);
        if (item && item.length > 0) {
            var listeners = S.editor.events.listeners.filter(a => a.event == event);
            if (listeners && listeners.length > 0) {
                for (var x = 0; x < listeners.length; x++) {
                    var listener = listeners[x];
                    if (typeof listener.func == 'function') {
                        listener.func();
                    }
                }
            }
        }
    }
};