S.editor.appsettings = {
    show: function () {
        S.editor.tabs.create("Website Settings", "app-settings-section", {},
            () => { //onfocus
                $('.tab.app-settings').removeClass('hide');
                S.editor.filebar.update('Website Settings', 'icon-settings');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        S.editor.dropmenu.hide();
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .app-settings').removeClass('hide');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        S.ajax.post('AppSettings/Render', {},
            function (d) {
                var data = JSON.parse(d);
                S.ajax.inject(data);
                S.editor.resizeWindow();
            }
        );
    }
};