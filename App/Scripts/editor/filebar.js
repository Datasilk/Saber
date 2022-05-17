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
            var self = S.editor.filebar.preview;
            var iframe = window.parent.document.getElementsByClassName('editor-iframe')[0];
            if (iframe.style.display == 'block') {
                self.show();
            } else {
                self.hide();
            }
        },

        show: function () {
            var iframe = window.parent.document.getElementsByClassName('editor-iframe')[0];
            var doc = window.parent.document;
            var tagcss = doc.getElementById('page_css');
            var tagjs = doc.getElementById('page_js');
            var css = '/' + S.editor.path + '.css';
            var src = '/' + S.editor.path + '.js';
            var rnd = Math.floor(Math.random() * 9999);

            //first, reload CSS
            if (S.editor.files.less.changed == true) {
                S.editor.files.less.changed = false;
                tagcss.remove();
                var link = doc.createElement('link');
                link.rel = 'stylesheet';
                link.type = 'text/css';
                link.id = 'page_css';
                link.href = css + '?r=' + rnd;
                doc.head.appendChild(link);
            }

            if (S.editor.files.website.css.changed == true) {
                //reload website.css
                S.editor.files.website.css.changed = false;
                doc.getElementById('website_css').setAttribute('href', '/css/website.css?r=' + rnd);
            }

            //next, reload rendered HTML
            if (S.editor.files.html.changed == true || S.editor.files.content.changed == true) {
                S.editor.files.html.changed = false;
                S.editor.files.content.changed = false;
                S.ajax.post('Page/Render' + window.parent.location.search, { path: S.editor.path + '.html', language: window.language || 'en' },
                    function (d) {
                        var website = doc.getElementsByClassName('website')[0];
                        website.innerHTML = d.html;
                        if (d.javascript) {
                            window.parent.addScript(d.javascript);
                        }
                        changeJs(true);
                    },
                    null, true
                );
            } else if (S.editor.files.js.changed == true) {
                changeJs();
            }
            showContent();

            //update Rhino browser window (if applicable)
            if (S.editor.Rhino) {
                S.editor.Rhino.defaulttheme();
            }

            //finally, reload javascript file
            function changeJs(htmlChanged) {
                var js = doc.getElementById('website_js');
                js.parentNode.removeChild(js);

                S.util.js.load('/js/website.js' + '?r=' + rnd, 'website_js',
                    function () {
                        if (S.editor.files.js.changed == true || htmlChanged == true) {
                            S.editor.files.js.changed = false;
                            tagjs.remove();
                            S.util.js.load(src + '?r=' + rnd, 'page_js', null, null, doc);
                        }
                    }, (err) => {}, doc);
            }

            function showContent() {
                iframe.style.display = "none";
                window.parent.document.body.style.overflow = '';
                S.editor.visible = false;
            }

        },

        hide: function () {
            var container = window.parent.document.getElementsByClassName('editor-iframe')[0];
            container.style.display = "block";
            window.parent.document.body.style.overflow = 'hidden';
            S.editor.visible = true;
            var iframe = window.parent.document.getElementById('editor-iframe');
            iframe.contentWindow.focus();

            //update Rhino browser window (if applicable)
            if (S.editor.Rhino) {
                window.parent.Rhino.bordercolor(34, 34, 34);
                window.parent.Rhino.toolbarcolor(34, 34, 34);
                window.parent.Rhino.toolbarfontcolor(200, 200, 200);
                window.parent.Rhino.toolbarbuttoncolors(
                    S.util.color.argbToInt(255, 34, 34, 34), //bg
                    S.util.color.argbToInt(255, 40, 40, 40), //bg hover
                    S.util.color.argbToInt(255, 0, 153, 255), //bg mouse down
                    S.util.color.argbToInt(255, 200, 200, 200), //font
                    S.util.color.argbToInt(255, 200, 200, 200), //font hover
                    S.util.color.argbToInt(255, 200, 200, 200) //font mouse down
                );
            }

            if (S.editor.initialized == false) {
                S.editor.init();
                return;
            }
            S.editor.resize.window();
            setTimeout(function () {
                S.editor.resize.window();
            }, 10);
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