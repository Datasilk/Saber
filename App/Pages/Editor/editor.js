﻿S.editor = {
    instance: null,
    EditSession: require("ace/edit_session").EditSession,
    sessions: {},

    init: function () {
        //initialize code editor
        var editor = ace.edit("editor");
        editor.setTheme("ace/theme/xcode");

        this.instance = editor;
        this.resize();
        editor.getSession().on('change', this.resize);

        //add button events
        $('.tab-browse').on('click', S.editor.explorer.show);
    },

    resize: function () {
        var editor = S.editor.instance;
        var newHeight = editor.getSession().getScreenLength() * editor.renderer.lineHeight + editor.renderer.scrollBar.getWidth();
        if (newHeight < 20) { newHeight = 20; }
        newHeight += 30;
        $('#editor').css({ minHeight: newHeight.toString() + "px" });
        $('#editor-section').css({ minHeight: newHeight.toString() + "px" });
        editor.resize();
    },

    fileId: function (path) {
        return path.replace(/\//g, '_').replace(/\./g, '_');
    },

    decodeHtml(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    },

    explorer: {
        show: function () {
            if ($('.file-browser ul.columns-list').children().length == 0) {
                S.editor.explorer.dir('root');
            }
            $('.editor .sections > div').addClass('hide');
            $('.editor .file-browser').removeClass('hide');
            $('ul.tabs li, ul.tabs > li > div').removeClass('selected');
            $('ul.tabs .tab-browse, ul.tabs > li:nth-child(1)').addClass('selected');
        },

        dir: function (path) {
            S.ajax.post('Editor/Dir', { path: path },
                function (d) {
                    $('.file-browser ul.columns-list').html(d);
                },
                function () {
                    S.message.show('.editor .message', S.message.error.generic);
                }
            );
        },

        open: function (path, code) {
            var id = S.editor.fileId(path);

            //hide sections
            $('.editor .sections > div').addClass('hide');
            $('.editor .code-editor').removeClass('hide');

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
            var delay = 200;
            if (session == null && typeof code == 'undefined') {
                //load new session from ajax POST
                S.ajax.post("Editor/Open", { path: path },
                    function (d) {
                        session = new S.editor.EditSession(S.editor.decodeHtml(d));
                        session.setMode("ace/mode/" + mode);
                        editor.setSession(session);
                        S.editor.sessions[id] = session;
                        editor.clearSelection();
                        S.editor.resize();
                        setTimeout(function () {
                            S.editor.resize();
                        }, delay);
                        $('.editor').append('<script language="text/html" id="file_' + id + '">' + d + '</script>');
                    },
                    function () {
                        S.message.show('.editor .message', S.message.error.generic);
                    }
                );
            } else if (typeof code != 'undefined') {
                //load new session from provided code argument
                session = new S.editor.EditSession(S.editor.decodeHtml(code));
                session.setMode("ace/mode/" + mode);
                editor.setSession(session);
                S.editor.sessions[id] = session;
                editor.clearSelection();
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, delay);
            } else {
                //load existing session
                editor.setSession(session);
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, delay);
            }
            editor.focus();
        }
    }
};

S.editor.init();