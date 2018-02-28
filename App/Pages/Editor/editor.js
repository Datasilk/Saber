S.editor = {
    instance: null,
    EditSession: require("ace/edit_session").EditSession,
    sessions: {},
    selected: '',
    div: $('.code-editor'),

    init: function () {
        //initialize code editor
        var editor = ace.edit("editor");
        editor.setTheme("ace/theme/xcode");

        this.instance = editor;
        this.resize();

        //add button events
        $('.item-browse').on('click', S.editor.explorer.show);
        $('.tab-drop-menu').on('click', S.editor.dropmenu.show);
        $('.bg-overlay').on('click', S.editor.dropmenu.hide);

        //add window resize event
        $(window).on('resize', S.editor.resizeWindow);
    },

    resize: function () {
        var editor = S.editor.instance;
        var newHeight = editor.getSession().getScreenLength() * editor.renderer.lineHeight + editor.renderer.scrollBar.getWidth();
        if (newHeight < 20) { newHeight = 20; }
        newHeight += 30;
        $('#editor').css({ minHeight: newHeight.toString() + "px" });
        $('#editor-section').css({ minHeight: newHeight.toString() + "px" });
        editor.resize();
        S.editor.resizeWindow();
    },

    resizeWindow: function () {
        var win = S.window.pos();
        var div = S.editor.div;
        var pos = div.offset();
        $('.code-editor').css({ height: win.h - pos.top - 20 });
        
    },

    dropmenu: {
        show: function () {
            $('.editor-drop-menu, .bg-overlay').removeClass('hide');
            $(document.body).on('click', S.editor.dropmenu.hide);
        },

        hide: function (e) {
            var hide = false;
            if (e) {
                if ($(e.target).parents('.editor-drop-menu > div').length == 0) {
                    hide = true;
                }
            } else { hide = true; }
            if (hide == true) {
                $('.editor-drop-menu, .bg-overlay').addClass('hide');
                $(document.body).off('click', S.editor.dropmenu.hide);
            }
        }
    },

    save: function () {
        var paths = S.editor.selected.split('_');
        //var path = paths.map()
    },

    fileId: function (path) {
        return path.replace(/\//g, '_').replace(/\./g, '_');
    },

    decodeHtml(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    },

    sessions: {
        add: function (id, mode, code) {
            var editor = S.editor.instance;
            session = new S.editor.EditSession(S.editor.decodeHtml(code));
            session.setMode("ace/mode/" + mode);
            session.on('change', S.editor.resize);
            editor.setSession(session);
            S.editor.sessions[id] = session;
            editor.clearSelection();
            S.editor.resize();
            setTimeout(function () {
                S.editor.resize();
            }, 200);
            editor.focus();
        }
    },

    explorer: {
        show: function () {
            if (!$('.editor .file-browser').hasClass('hide')) { S.editor.explorer.hide(); return;}
            if ($('.file-browser ul.menu').children().length == 0) {
                S.editor.explorer.dir('root');
            }
            $('.editor .file-browser').removeClass('hide');
            $('.editor').addClass('show-browser');
            $('ul.tabs li, ul.tabs > li > div').removeClass('selected');
            $('ul.tabs .tab-browse, ul.tabs > li:nth-child(2)').addClass('selected');
            S.editor.dropmenu.hide();
        },

        hide: function () {
            $('.editor .file-browser').addClass('hide');
            $('.editor').removeClass('show-browser');
        },

        dir: function (path) {
            S.ajax.post('Editor/Dir', { path: path },
                function (d) {
                    $('.file-browser ul.menu').html(d);
                },
                function () {
                    S.message.show('.editor .message', S.message.error.generic);
                }
            );
        },

        open: function (path, code) {
            var id = S.editor.fileId(path);

            //deselect tabs
            $('ul.tabs li, ul.tabs > li > div').removeClass('selected');

            //check for existing tab
            var tab = $('.editor ul.tabs .tab-' + id);
            if (tab.length == 0) {
                var temp = $('#template_tab').html().trim();
                var paths = path.split('/');
                var file = paths[paths.length - 1];
                $('.editor .edit-menu ul.tabs').append(temp
                    .replace(/\#\#id\#\#/g, id)
                    .replace('##path##', path)
                    .replace('##title##', file)
                );
            } else {
                tab.addClass('selected').find('.row.hover').addClass('selected');
            }

            //check for existing source code
            var session = S.editor.sessions[id];
            var editor = S.editor.instance;
            var paths = path.split('/');
            var ext = paths[paths.length - 1].split('.')[1];
            var mode = 'html';
            switch (ext) {
                case 'css': case 'less': mode = ext; break;
                case 'js': mode = 'javascript'; break;
            }
            if (session == null && typeof code == 'undefined') {
                //load new session from ajax POST
                S.ajax.post("Editor/Open", { path: path },
                    function (d) {
                        S.editor.sessions.add(id, mode, d)
                    },
                    function () {
                        S.message.show('.editor .message', S.message.error.generic);
                    }
                );
            } else if (typeof code != 'undefined') {
                //load new session from provided code argument
                S.editor.sessions.add(id, mode, code)
                
            } else {
                //load existing session
                editor.setSession(session);
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, 200);
                editor.focus();
            }

            //update selected session
            S.editor.selected = path;
        }
    }
};

S.editor.init();