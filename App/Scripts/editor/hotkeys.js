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
        } else if (e.altKey == true) {
            if (!isNaN(key) && !isNaN(parseFloat(key))) {
                var index = parseInt(key) - 1;
                if (index == -1) { index = 9; } //0 key is last index
                var tabs = $('.edit-tabs li');
                if (tabs.length > index) {
                    S.editor.tabs.select(tabs[index].className.replace('tab-', '').replace(' selected', ''));
                    has = true;
                }
            }
        } else {
            switch (e.code) {
                case 'F2': //content fields
                    S.editor.filebar.fields.show();
                    has = true;
                    break;
                case 'F3': //page settings
                    S.editor.filebar.settings.show();
                    has = true;
                    break;
                case 'F4'://page resources
                    S.editor.filebar.resources.show();
                    has = true;
                    break;
                case 'F6'://website settings
                    S.editor.websettings.show();
                    has = true;
                    break;
                case 'F7'://user accounts
                    S.editor.users.show();
                    has = true;
                    break;
                case 'F8'://security groups
                    S.editor.security.show();
                    has = true;
                    break;
                case 'F9'://file browser
                    S.editor.explorer.show();
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
                    console.log('toggle preview');
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