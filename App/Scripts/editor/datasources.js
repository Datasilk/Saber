S.editor.datasources = {
    _dirty: false,

    show: function () {
        //show Data Sources tab
        S.editor.dropmenu.hide();
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .data-sources').removeClass('hide');
        $('ul.file-tabs > li').removeClass('selected');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        if ($('.data-sources-section').length == 0) {
            //load data sources list
            S.editor.tabs.create('Data Sources', 'data-sources-section', { removeOnClose: true },
                () => { //onfocus
                    $('.tab.data-sources').removeClass('hide');
                    updateFilebar();
                },
                () => { //onblur
                },
                () => { //onsave
                },
                () => { //onclose
                }
            );
            function updateFilebar() {
                S.editor.filebar.update('Data Sources', 'icon-datasources', $('#data_sources_toolbar').html());
            }
            S.ajax.post("DataSources/RenderList", {}, (response) => {
                $('.sections .datasources-contents').html(response);
                S.editor.resize.window();
            });
        }
    },
    added: function () {
        S.editor.datasources._dirty = true;
        if ($('.data-sources-section').length > 0) {
            S.ajax.post("DataSources/RenderList", {}, (response) => {
                $('.sections .datasources-contents').html(response);
                S.editor.resize.window();
                S.editor.events.broadcast('data-source-added');
            });
        }
    }
};

setTimeout(() => {
    S.editor.events.add('data-source-added');
}, 50);