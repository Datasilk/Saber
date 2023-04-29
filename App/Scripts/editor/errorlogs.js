S.editor.errorlogs = {
    _loaded: false,
    show: function () {
        S.editor.tabs.create("Error Logs", "errorlogs-section", {},
            () => { //onfocus
                $('.tab.website-errorlogs').removeClass('hide');
                S.editor.filebar.update('Error Logs', 'icon-error');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        S.editor.dropmenu.hide();
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .website-errorlogs').removeClass('hide');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');
        if (S.editor.websettings._loaded) {
            S.editor.tabs.select('errorlogs-section');
        } else {
            S.editor.errorlogs.update(0, 50, '');
        }
    },

    update: function (start, length, search) {
        S.ajax.post('ErrorLogs/Render', {start:start, length:length, search:search},
            function (d) {
                var data = JSON.parse(d);
                S.ajax.inject(data);
                S.editor.resize.window();
                S.editor.errorlogs._loaded = true;
            }
        );
    }
};