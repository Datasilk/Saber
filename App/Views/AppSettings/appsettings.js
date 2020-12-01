﻿(function () {
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
            var container = btn.parents('.icon-box').first();
            var img = container.find('.icon-img img');
            console.log(container);
            console.log(img);
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

    //set up email settings
    $('#emailclients').on('change', () => {
        var id = $('#emailclients').val();
        $('.email-client').addClass('hide');
        $('.email-client.client-' + id).removeClass('hide');
    });

    $('.email-clients button.save').on('click', (e) => {
        e.preventDefault();
        var section = $('.email-client:not(.hide)').first();
        var id = section[0].className.replace('email-client client-', '');
        var inputs = section.find('input, select');
        var params = {};
        inputs.each(a => {
            var b = $(a);
            var type = b.attr('type') ?? '';
            params[b.attr('id').replace(id + '_', '')] = (
                type == 'checkbox' ? (a.checked == true ? 'True' : 'False') : b.val()
            );
        });
        var data = {
            id: id,
            parameters: params
        };
        console.log(data);

        S.ajax.post('AppSettings/SaveEmailClient', data, () => {
            S.editor.message('', 'Email Client settings saved');
        }, (err) => {
            S.editor.message('', err.responseText, 'error');
        })
    });

    //load accordion functionality 
    S.accordion.load({}, () => { S.editor.resizeWindow(); }); 
})();