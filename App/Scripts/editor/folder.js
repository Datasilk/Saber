S.editor.folder = {
    create: {
        popup: null,
        show: function () {
            S.editor.dropmenu.hide();
            var path = S.editor.explorer.path;
            if (path == 'root') { path = 'wwwroot'; }
            S.editor.folder.create.popup = S.popup.show('New Folder',
                $('#template_newfolder').html()
                    .replace('##folder-path##', path)
            );
            newfolder.focus();
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
                S.editor.error('.popup .messages', 'You cannot create folders within the root folder');
                return false;
            }
            S.ajax.post('Files/NewFolder', data,
                function (d) {
                    //reload file browser
                    if (data.path == S.editor.explorer.path) {
                        S.editor.explorer.dir(S.editor.explorer.path);
                    }
                    S.popup.hide(S.editor.folder.create.popup);
                },
                function (d) {
                    S.editor.error('.popup .messages', d.response);
                }
            );
            return false;
        }
    },

    delete: (path) => {
        if (window.parent.confirm('Do you really want to delete the folder "' + path + '"? This cannot be undone.')) {
            S.ajax.post('Files/DeleteFolder', { path: path },
                function (d) {
                    //reload file browser
                    S.editor.explorer.dir();
                    //check if any tabs are open from deleted folder
                    S.editor.tabs.closeFromPath(path);
                },
                function (d) {
                    S.editor.error('', d.response);
                }
            );
        }
    }
};