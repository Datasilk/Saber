S.editor = {
    instance: null,

    init: function () {
        //initialize code editor
        var editor = ace.edit("editor");
        editor.setTheme("ace/theme/xcode");
        editor.session.setMode("ace/mode/html");

        this.instance = editor;

        //add button events
        $('.tab-browse').on('click', S.editor.explorer.show);
    },

    explorer: {
        show: function () {
            if ($('.file-browser ul.columns-list').children() == 0) {
                S.editor.dir('root');
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
            var id = path.replace(/\//g, '_').replace(/\./g, '_');

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

            } else {
                editor.setValue(content.html().trim());
                editor.clearSelection();
            }
        }
    }
};

S.editor.init();