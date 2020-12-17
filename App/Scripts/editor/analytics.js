S.editor.analytics = {
    show: function () {
        S.editor.tabs.create("Website Analytics", "analytics-section", {},
            () => { //onfocus
                S('.tab.website-analytics').removeClass('hide');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        S.editor.dropmenu.hide();
        S('.editor .sections > .tab').addClass('hide');
        S('.editor .sections > .website-analytics').removeClass('hide');

        //disable save menu
        S('.item-save').addClass('faded').attr('disabled', 'disabled');
        S('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        S.ajax.post('Analytics/Render', { timeScale: 3 },
            function (d) {
                var data = JSON.parse(d);
                S.ajax.inject(data);
                S.editor.resizeWindow();
            }
        );
    }
};