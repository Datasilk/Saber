S.editor.folder = {
    create: {
        show: function () {
            S.editor.dropmenu.hide();
            S.popup.show('New Folder',
                $('#template_newfolder').html()
                    .replace('##folder-path##', S.editor.explorer.path)
            );
            //set up button events within popup
            $('.popup form').on('submit', S.editor.folder.create.submit)
        },

        submit: function(e) {
            e.preventDefault();
            e.cancelBubble = true;
            var data = {
                path: $('#newfolderpath').val().replace(/\s/g, ''),
                folder: $('#newfolder').val()
            };
            if (data.path == 'root') {
                S.message.show('.popup .message', 'error', 'You cannot create folders within the root folder');
                return false;
            }
            S.ajax.post('Files/NewFolder', data,
                function (d) {
                    //reload file browser
                    if (data.path == S.editor.explorer.path) {
                        S.editor.explorer.dir(S.editor.explorer.path);
                    }
                    S.popup.hide();
                },
                function (d) {
                    S.message.show('.popup .message', 'error', d.response);
                }
            );
            return false;
        }
    }
};