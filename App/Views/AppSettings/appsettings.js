(function () {
    $('.apple-app-icon button').on('click', (e) => {
        $(e.target).parent().find('input[type="file"]')[0].click();
    });

    $('.apple-app-icon input[type="file"]').on('change', (e) => {
        var file = e.target.files[0];
        var xhr = new XMLHttpRequest();
        var fd = new FormData();
        xhr.open("POST", "/AppSettings/UploadAppleIcon", true);
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

    //load accordion functionality
    S.accordion.load(() => { S.editor.resizeWindow(); }); 
})();