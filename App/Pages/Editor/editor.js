S.editor = {
    instance: null,

    init: function () {
        //initialize code editor
        var editor = ace.edit("editor");
        editor.setTheme("ace/theme/xcode");
        editor.session.setMode("ace/mode/html");

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

        open: function (path) {
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
            var content = $('#file_' + id);
            var editor = S.editor.instance;

            if (content.length == 0) {
                S.ajax.post("Editor/Open", { path: path },
                    function (d) {
                        var editor = S.editor.instance;
                        editor.setValue(S.editor.decodeHtml(d));
                        editor.clearSelection();
                        S.editor.resize();
                        setTimeout(function () {
                            S.editor.resize();
                        }, 500);
                        $('.editor').append('<script language="text/html" id="file_' + id + '">' + d + '</script>');
                    },
                    function () {
                        S.message.show('.editor .message', S.message.error.generic);
                    }
                );
            } else {
                editor.setValue(S.editor.decodeHtml(content.html().trim()));
                editor.clearSelection();
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, 500);
            }
        }
    }
};

S.editor.init();