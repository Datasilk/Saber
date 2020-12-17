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
            S.editor.dropmenu.hide();
            S.editor.tabs.create("Page Content", "content-fields-section", { isPageResource: true },
                () => { //onfocus
                    $('.tab.content-fields').removeClass('hide');
                    var path = S.editor.path.substr(8);
                    S.editor.filebar.update('Page Content for <a href="/' + path + '">' + path + '</a>', 'icon-form-fields');
                    if (S.editor.files.content.changed == true) {
                        //reload content fields
                        S.editor.fields.load();
                    }
                },
                () => { //onblur

                },
                () => { //onsave

                }
            );


            //show content fields section & hide other sections
            $('.editor .sections > .tab').addClass('hide');
            $('.editor .sections > .content-fields').removeClass('hide');
            $('ul.file-tabs > li').removeClass('selected');
            $('ul.file-tabs > li.tab-content-fields').addClass('selected');

            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

            if ($('#lang').children().length == 0) {
                //load list of languages
                S.ajax.post('Languages/Get', {},
                    function (d) {
                        var html = '';
                        var langs = d.split('|');
                        var userlang = window.language || 'en';
                        for (x = 0; x < langs.length; x++) {
                            var lang = langs[x].split(',');
                            html += '<option value="' + lang[0] + '"' + (lang[0] == userlang ? ' selected' : '') + '>' + lang[1] + '</option>';
                        }
                        $('#lang').html(html);
                        S.editor.fields.load();
                    },
                    function () {
                        S.editor.error();
                    }
                );
            } else {
                //tab already loaded
                S.editor.tabs.select('content-fields-section');
                if (S.editor.fields.changed == true) {
                    //enable save menu since file was previously changed
                    $('.item-save').removeClass('faded').removeAttr('disabled');
                }
            }
        }
    },

    code: {
        show: function () {
            $('.editor .sections > .tab').addClass('hide');
            $('.editor .sections > .code-editor').removeClass('hide');
            $('ul.file-tabs > li').removeClass('selected');
            $('ul.file-tabs > li.tab-file-code').addClass('selected');
            if (S.editor.isChanged(S.editor.selected)) { S.editor.changed(); }
            $('.item-save-as').removeClass('faded').removeAttr('disabled');
            setTimeout(function () { S.editor.resize(); }, 10);
        }
    },

    settings: {
        show: function () {
            S.editor.dropmenu.hide();
            $('.editor .sections > .tab').addClass('hide');
            $('.editor .sections > .page-settings').removeClass('hide');
            $('ul.file-tabs > li').removeClass('selected');
            $('ul.file-tabs > li.tab-page-settings').addClass('selected');

            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

            S.editor.settings.load();
        }
    },

    resources: {
        show: function (noload) {
            if (S.editor.selected == '') { return; }
            S.editor.dropmenu.hide();
            $('.editor .sections > .tab').addClass('hide');
            $('.editor .sections > .page-resources').removeClass('hide');
            $('ul.file-tabs > li').removeClass('selected');
            $('ul.file-tabs > li.tab-page-resources').addClass('selected');

            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

            if (noload === true) { return; }
            S.editor.resources.load(S.editor.path);
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
            var css = '/' + S.editor.path.replace('content/', 'content/pages/') + '.css';
            var src = '/' + S.editor.path.replace('content/', 'content/pages/') + '.js';
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
                S.ajax.post('Page/Render', { path: S.editor.path + '.html', language: window.language || 'en' },
                    function (d) {
                        var website = doc.getElementsByClassName('website')[0];
                        website.innerHTML = d.html;
                        if (d.javascript) {
                            doc.addScript(d.javascript);
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
            var iframe = window.parent.document.getElementsByClassName('editor-iframe')[0];
            iframe.style.display = "block";
            window.parent.document.body.style.overflow = 'hidden';
            S.editor.visible = true;

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
            S.editor.resize();
            setTimeout(function () {
                S.editor.resize();
            }, 10);
        }
    }
};