S.editor.filebar = {
    update: (text, icon, toolbar) => {
        $('.file-path').html(text);
        $('.file-bar .file-icon use').attr('xlink:href', '#' + icon);
        if (toolbar == null) { toolbar = ''; }
        S.editor.filebar.toolbar.update(toolbar);
    },

    toolbar: {
        update: (html) => {
            $('.tab-toolbar').html(html);
        }
    },

    fields: {
        show: function () {
            if (S.editor.selected.indexOf('/partials/') >= 0) {
                S.editor.fields.load(S.editor.selected);
                return;
            }
            S.editor.dropmenu.hide();
            S.editor.tabs.create("Page Content", "content-fields-section", { showPageButtons:true, selected:true },
                () => { //onfocus
                    S.editor.tabs.show('content-fields-section');
                    $('ul.file-tabs > li.tab-content-fields-section').addClass('selected');
                    var path = S.editor.path.replace('content/pages/', '');
                    S.editor.filebar.update('Page Content for <a href="/' + path + '">' + path + '</a>', 'icon-form-fields');
                    if (S.editor.files.content.changed == true) {
                        //reload content fields
                        S.editor.fields.load();
                    }
                    S.editor.filebar.buttons.show('content-fields');
                },
                () => { //onblur

                },
                () => { //onsave

                }
            );
            $('.tab-content-fields-section').addClass('tab-for-content-fields');

            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

            if ($('.content-fields-section #lang').children().length == 0) {
                //load list of languages
                S.ajax.post('Languages/Get', {},
                    function (d) {
                        var html = '';
                        var langs = d.split('|');
                        var userlang = window.language || 'en';
                        for (var x = 0; x < langs.length; x++) {
                            var lang = langs[x].split(',');
                            html += '<option value="' + lang[0] + '"' + (lang[0] == userlang ? ' selected' : '') + '>' + lang[1] + '</option>';
                        }
                        $('.content-fields-section #lang').html(html);
                        S.editor.fields.load();
                    },
                    function () {
                        S.editor.error();
                    }
                );
            }
        }
    },

    code: {
        show: function () {
            $('.editor .sections > .tab:not(.code-editor)').addClass('hide');
            $('.editor .sections > .code-editor').removeClass('hide');
            S.editor.resize.window();
            $('ul.file-tabs > li').removeClass('selected');
            $('ul.file-tabs > li.tab-file-code').addClass('selected');
            if (S.editor.isChanged(S.editor.selected)) { S.editor.changed(); }
            $('.item-save-as').removeClass('faded').removeAttr('disabled');
            setTimeout(S.editor.resize.window, 10);
        }
    },

    settings: {
        show: function () {
            S.editor.pagesettings.load();
            S.editor.dropmenu.hide();
            S.editor.tabs.show('page-settings');
            S.editor.filebar.buttons.show('page-settings');
            $('ul.file-tabs > li.tab-page-settings').addClass('selected');

            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            $('.item-save-as').addClass('faded').attr('disabled', 'disabled');
        }
    },

    resources: {
        show: function (noload) {
            if (S.editor.selected == '') { return; }
            S.editor.dropmenu.hide();
            S.editor.tabs.show('page-resources');
            $('ul.file-tabs > li.tab-page-resources').addClass('selected');

            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

            //load resources
            if (noload != true) { S.editor.resources.load(S.editor.path); }
            S.editor.filebar.buttons.show('page-resources');
        }
    },

    preview: {
        toggle: function () {
            var iframe = window.parent.document.getElementsByClassName('editor-iframe')[0];
            if (iframe.style.display == 'block') {
                S.editor.preview.show();
            } else {
                S.editor.preview.hide();
            }
        }
    },

    buttons: {
        show: function (selected, ishtml) {
            $('.file-tabs .selected').removeClass('selected');
            if(selected && selected.length > 0)$('.tab-' + selected + ' .row.hover').addClass('selected');
            $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').show();
            if (ishtml) {
                $('.tab-components').show();
                $('.tab-sourcecode').hide();
            } else {
                $('.tab-components').hide();
                $('.tab-sourcecode').show();
            }
        },
        hide: function () {
            $('.tab-components, .tab-sourcecode, .tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').hide();
        }
    }
};