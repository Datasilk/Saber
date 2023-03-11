S.editor.file = {
    create: {
        popup: null,
        show: function () {
            S.editor.dropmenu.hide();
            var path = S.editor.explorer.path;
            if (path == 'root') { path = 'wwwroot';}
            S.editor.file.create.popup = S.popup.show('New File',
                $('#template_newfile').html()
                    .replace('##folder-path##', path)
            );
            newfilename.focus();
            //set up button events within popup
            $('.popup form').on('submit', S.editor.file.create.submit)
        },

        submit: function (e) {
            e.preventDefault();
            e.cancelBubble = true;
            var data = {
                path: $('#newfilepath').val(),
                filename: $('#newfilename').val().replace(/\s/g, '')
            };
            if (data.path == 'root') {
                S.editor.error('.popup .messages', 'You cannot create files in the root folder');
                return false;
            }
            S.ajax.post('Files/NewFile', data,
                function (d) {
                    //reload file browser
                    if (data.path == S.editor.explorer.path) {
                        S.editor.explorer.dir();
                    }
                    S.popup.hide(S.editor.file.create.popup);
                },
                function (d) {
                    S.editor.error('.popup .messages', d.response);
                }
            );
            return false;
        }
    },

    delete: (path) => {
        if (window.parent.confirm('Do you really want to delete the file "' + path + '"? This cannot be undone.')) {
            S.ajax.post('Files/DeleteFile', { path: path },
                function (d) {
                    //reload file browser
                    S.editor.explorer.dir();
                    //check if a tab is open from deleted file
                    S.editor.tabs.closeFromPath(path);
                },
                function (d) {
                    S.editor.error('', d.response);
                }
            );
        }
    }
};