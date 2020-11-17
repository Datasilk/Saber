S.editor.hotkey = {
    pressed: function (e) {
        if (S.editor.visible == false) { return; }
        var has = false;
        var key = String.fromCharCode(e.which).toLowerCase();
        if (e.ctrlKey == true) {
            switch (key) {
                case 's':
                    //save file
                    S.editor.save();
                    has = true;
                    break;
            }
        }
        if (has == true) {
            event.preventDefault();
            return false;
        }
    },

    pressedPreview: function (e) {
        if (e.ctrlKey == false && e.altKey == false && e.shiftKey == false) {
            switch (e.which) {
                case 27: //escape key
                    //ignore escape key if Monaco editor is showing a suggestion popup
                    if ($('.editor-widget.suggest-widget.visible .monaco-list.element-focused').length > 0 ||
                        $('.editor-widget.find-widget.visible').length > 0
                    ) {
                        break;
                    }
                    //show website preview
                    S.editor.filebar.preview.toggle();
                    break;
            }
        }
    }
};