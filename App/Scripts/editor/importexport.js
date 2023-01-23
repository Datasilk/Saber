S.editor.import = {
    show: function () {
        S.editor.dropmenu.hide();
        S.ajax.post('ImportExport/RenderImport', {}, function (d) {
            S.popup.show('Import Website', d, { width: 600 });

            $('.import-export.import button').on('click', () => {
                //allow user to select a zip file to upload
                $('#import_zip')[0].click();
            });

            $('#import_zip').on('change', () => {
                //auto-upload zip file after user selects file
                var file = $('#import_zip')[0].files[0];
                var xhr = new XMLHttpRequest();
                var fd = new FormData();
                xhr.open("POST", "/Import", true);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState == 4 && xhr.status == 200) {
                        S.popup.hide();
                        alert("Imported website content successfully after generating a zip backup file (/backups/" + xhr.responseText + ').\n Please "hard" refresh this page (Ctrl + F5) to see the changes made to your website.')
                    }
                };
                xhr.onerror = function (err) {
                    S.popup.hide();
                    alert(err.responseText);
                }
                fd.append("zip", file);
                xhr.send(fd);
            });
        });
    }
};

S.editor.export = {
    show: function () {
        S.editor.dropmenu.hide();
        S.ajax.post('ImportExport/RenderExport', {}, function (d) {
            S.popup.show('Export Website', d, { width: 600 });
        });
    },

    hide: function () {
        S.popup.hide();
    }

};