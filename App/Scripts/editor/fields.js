S.editor.fields = {
    clone: $('.editor .textarea-clone > div'),
    loaded: false,
    selected: '',
    changed: false,
    checkjs: function (callback) {
        //make sure contentfields.js is loaded
        if (!S.editor.fields.loaded) {
            S.util.js.load('/editor/js/views/contentfields/contentfields.js', 'contentfields', callback);
        } else {
            callback();
        }
    },
    load: function (file, show) {
        S.editor.fields.checkjs(() => {
            setTimeout(() => { S.editor.fields._load(file, show); }, 100);
        });
    },
    popup: function (partial, lang, title, fieldsdata, buttonTitle, submit, excludeFields, renderApi, callback) {
        var popup = S.popup.show(title, '<div class="has-content-fields"><form></form></div>');
        popup.css({ width: '90%', 'max-width': '1200px' });
        S.editor.fields.checkjs(() => {
            S.editor.fields._popup(popup, partial, lang, title, fieldsdata, buttonTitle, submit, excludeFields, renderApi, callback);
        });
        return popup;
    },
    custom: {}
};

//add event listener for window resize stop to change height of all content field textarea inputs
S.editor.resize.stop.add('content-fields', S.editor.fields.resizeAll);