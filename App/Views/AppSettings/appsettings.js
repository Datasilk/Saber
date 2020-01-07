(function () {
    //set up icon buttons
    var selbtn = '';
    var icons = [
        { name: 'apple', type: 1 },
        { name: 'android', type: 2 },
    ];
    for (var x = 0; x <= 1; x++) {
        icon = icons[x];
        $('.' + icon.name + '-app-icon button').on('click', (a) => {
            var btn = $(a.target);
            var img = btn.parent().find('.icon-img img');
            var px = btn.attr('px');
            selbtn = { btn: btn, img: img, px: px };
            upload_app_icon.click();
        });
    }

    $('#upload_app_icon').on("change", (e) => {
        S.loader();
        S.upload.file(upload_app_icon.files[0], '/api/AppSettings/UploadPngIcon?type=' + icon.type + '&px=' + selbtn.px,
            null, //onprogress
            (e) => { //oncomplete
                //update icon
                setTimeout(function () {
                    selbtn.img.attr('src', '/images/mobile/' + icon.name + '-' + selbtn.px + 'x' + selbtn.px + '.png?r=' + Math.round(Math.random() * 9999));
                    selbtn.img.parent().removeClass('hide');
                }, 1000);
                $('.loader').remove();
            },
            (e) => { //onerror
                S.editor.error(null, e.responseText);
                $('.loader').remove();
            }
        );
    });

    

    //load accordion functionality 
    S.accordion.load(() => { S.editor.resizeWindow(); }); 
})();