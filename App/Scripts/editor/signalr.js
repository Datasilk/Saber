S.editor.signalR = {
    loaded: false,
    load: function () {
        if (!S.signalR.loaded) {
            S.util.js.load('/editor/js/utility/signalr/signalr.js', 'signalr');
            S.signalR.loaded = true;
        }
    }
}