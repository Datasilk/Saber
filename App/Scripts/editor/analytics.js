S.editor.analytics = {
    show: function () {
        S.editor.tabs.create("Website Analytics", "analytics-section", {},
            () => { //onfocus
                S.editor.tabs.show('website-analytics');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        S.editor.tabs.show('website-analytics');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        S.ajax.post('Analytics/Render', { timeScale: 3 },
            function (d) {
                var data = JSON.parse(d);
                S.ajax.inject(data);
                S.editor.resize.window();
            }
        );
    }
};