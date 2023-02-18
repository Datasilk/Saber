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
                var data = {
                    backup: import_backup.checked == true ? 1 : 0,
                    delete: import_delete.checked == true ? 1 : 0
                };
                xhr.open('POST', '/Import?backup=' + data.backup + '&delete=' + data.delete, true);
                xhr.onreadystatechange = function () {
                    if (xhr.readyState == 4 && xhr.status == 200) {
                        S.popup.hide();
                        alert('Imported website content successfully' +
                            (data.backup == 1 ? ' after generating a zip backup file (/backups/latest.zip)' : '') +
                            '.\n Please "hard" refresh this page (Ctrl + F5) to see the changes made to your website.');
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
            $('.import-export input').on('input', S.editor.export.updateForm);
            S.editor.export.updateForm();
        });
    },

    updateForm: function () {
        var data = {
            webpages: export_webpages.checked == true ? 1 : 0,
            images: export_images.checked == true ? 1 : 0,
            other: export_other.checked == true ? 1 : 0,
            modified: typeof export_lastmodified.value != 'undefined' && export_lastmodified.value != '' ? export_lastmodified.value.replace(/\-/g, '/') : null
        };
        $('a.export-content').attr('href', "/Export" +
            '?webpages=' + data.webpages +
            '&images=' + data.images +
            '&other=' + data.other +
            (data.modified != null ? '&modified=' + data.modified : ''));
    },

    hide: function () {
        S.popup.hide();
    }

};