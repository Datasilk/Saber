S.editor.preview = {
    show: function () {
        //display live preview of website
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

        if (S.editor.files.pagecss.length > 0) {
            //reload page-specific stylesheets
            S.editor.files.pagecss.forEach(css => {
                doc.querySelectorAll('link[href^="' + css + '"]')[0].setAttribute('href', css + '?r=' + rnd);
            });
            S.editor.files.pagecss = [];
        }

        if (S.editor.files.pagescripts.length > 0) {
            //reload page-specific scripts
            S.editor.files.pagescripts.forEach(js => {
                doc.querySelectorAll('script[src^="' + js + '"]')[0].setAttribute('src', js + '?r=' + rnd);
            });
            S.editor.files.pagescripts = [];
        }

        //next, reload rendered HTML
        if (S.editor.files.html.changed == true || S.editor.files.content.changed == true) {
            S.editor.files.html.changed = false;
            S.editor.files.content.changed = false;
            var search = window.parent.location.search;
            if (search != '') {
                var parts = search.substr(1).split('&');
                search = '?';
                parts.forEach(part => {
                    if (part.indexOf('path=') < 0 && part.indexOf('language=') < 0) {
                        search += part + '&';
                    }
                });
                if (search.length > 1) { search = search.substr(0, search.length - 2); } else { search = ''; }
            }
            S.ajax.post('Page/Render' + search, { path: S.editor.path + '.html', language: window.language || 'en' },
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
            if (js) { js.parentNode.removeChild(js); }

            S.util.js.load('/js/website.js' + '?r=' + rnd, 'website_js',
                function () {
                    if (S.editor.files.js.changed == true || htmlChanged == true) {
                        S.editor.files.js.changed = false;
                        tagjs.remove();
                        S.util.js.load(src + '?r=' + rnd, 'page_js', null, null, doc);
                    }
                }, (err) => { }, doc);
        }

        function showContent() {
            iframe.style.display = "none";
            window.parent.document.body.style.overflow = '';
            S.editor.visible = false;
        }

    },

    hide: function () {
        //hide preview & show Saber Editor
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
};