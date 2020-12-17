S.editor.websettings = {
    _loaded:false,
    show: function () {
        S.editor.tabs.create("Website Settings", "web-settings-section", {},
            () => { //onfocus
                S('.tab.web-settings').removeClass('hide');
                S.editor.filebar.update('Website Settings', 'icon-settings');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        S.editor.dropmenu.hide();
        S('.editor .sections > .tab').addClass('hide');
        S('.editor .sections > .web-settings').removeClass('hide');

        //disable save menu
        S('.item-save').addClass('faded').attr('disabled', 'disabled');
        S('.item-save-as').addClass('faded').attr('disabled', 'disabled');
        if (S.editor.websettings._loaded) {
            S.editor.tabs.select('web-settings-section');
        } else {
            S.ajax.post('WebsiteSettings/Render', {},
                function (d) {
                    var data = JSON.parse(d);
                    S.ajax.inject(data);
                    S.editor.resizeWindow();
                    S.editor.websettings._loaded = true;
                }
            );
        }
    }
};