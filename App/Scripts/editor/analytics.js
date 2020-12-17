S.editor.analytics = {
    show: function () {
        S.editor.tabs.create("Website Analytics", "analytics-section", {},
            () => { //onfocus
                $('.tab.website-analytics').removeClass('hide');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        S.editor.dropmenu.hide();
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .website-analytics').removeClass('hide');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        S.ajax.post('Analytics/Render', { timeScale: 3 },
            function (d) {
                var data = JSON.parse(d);
                S.ajax.inject(data);
                S.editor.resizeWindow();
            }
        );
    }
};