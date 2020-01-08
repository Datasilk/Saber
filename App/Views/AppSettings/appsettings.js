(function () {
    //set up icon buttons
    var selbtn = {};
    var icons = [
        { name: 'web', type: 0 },
        { name: 'apple', type: 1 },
        { name: 'android', type: 2 },
    ];
    for (var x = 0; x < icons.length; x++) {
        $('.' + icons[x].name + '-app-icon button').on('click', (a) => {
            var btn = $(a.target);
            var img = btn.parent().find('.icon-img img');
            var px = btn.attr('px') || '0';
            var icon = icons.filter(a => a.name == btn.attr('for'))[0];
            selbtn = {
                name: icon.name,
                type: icon.type,
                btn: btn,
                img: img,
                px: px,
                suffix: px != '0' ? '-' + px + 'x' + px : '-icon',
                path: px != '0' ? 'mobile/' : ''
            };
            console.log(selbtn);
            upload_app_icon.click();
        });
    }

    $('#upload_app_icon').on("change", (e) => {
        S.loader();
        S.upload.file(upload_app_icon.files[0], '/api/AppSettings/UploadPngIcon?type=' + selbtn.type + '&px=' + selbtn.px,
            null, //onprogress
            (e) => { //oncomplete
                //update icon
                setTimeout(function () {
                    console.log(selbtn);
                    selbtn.img.attr('src', '/images/' + selbtn.path + selbtn.name + selbtn.suffix + '.png?r=' + Math.round(Math.random() * 9999));
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