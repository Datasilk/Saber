(function () {
    $('.import-export.import button').on('click', () => {
        //allow user to select a zip file to upload
        $('#import_zip')[0].click();
    });

    $('#import_zip').on('change', () => {
        var file = $('#import_zip')[0].files[0];
        var xhr = new XMLHttpRequest();
        var fd = new FormData();
        xhr.open("POST", "/SaberImport", true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4 && xhr.status == 200) {
                console.log(xhr);
                alert("Imported website content successfully after generating a zip backup file (/backups/" + xhr.responseText + ')')
            }
        };
        xhr.onerror = function (err) {
            alert(err.responseText);
        }
        fd.append("zip", file);
        xhr.send(fd);
    });
})();