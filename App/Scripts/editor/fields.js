S.editor.fields = {
    clone: $('.content-fields .textarea-clone > div'),
        selected: '',
            changed: false,
                load: function () {
                    var lang = $('#lang').val();
                    S.editor.fields.changed = false;
                    $('.content-fields form').html('');
                    S.ajax.post('ContentFields/Render', { path: S.editor.path, language: lang },
                        function (d) {
                            S.editor.fields.selected = S.editor.selected;
                            $('.content-fields form').html(d);

                            //add language button
                            $('.content-fields .add-lang a').on('click', S.editor.lang.add.show);

                            //set up events for fields
                            $('.content-fields form textarea').on('keyup, keydown, change', S.editor.fields.change).each(
                                function (field) {
                                    S.editor.fields.change({ target: field });
                                }
                            );
                        },
                        function () { S.editor.error(); }
                    );
                },
    change: function (e) {
        if (S.editor.visible == false) { return; }
        var field = $(e.target);
        //resize field
        var clone = S.editor.fields.clone;
        clone.html(field.val().replace(/\n/g, '<br/>') + '</br>');
        field.css({ height: clone.height() });
        if (S.editor.fields.changed == false) {
            //enable save menu
            $('.item-save').removeClass('faded').removeAttr('disabled');
            S.editor.fields.changed = true;
        }
    },
    save: function () {
        var fields = {};
        var texts = $('.content-fields form').find('textarea, input, select');
        texts.each(function (txt) {
            if (!txt.id || (txt.id && txt.id.indexOf('field_') < 0)) { return;}
            var t = $(txt);
            var id = txt.id.replace('field_', '');
            switch (txt.tagName.toLowerCase()) {
                case 'textarea':
                    fields[id] = t.val();
                    break;
                case 'input':
                    var type = t.attr('type');
                    switch (type) {
                        case 'checkbox':
                            fields[id] = txt.checked == true ? '1' : '0';
                            break;
                    }
                    break;
            }

        });
        S.ajax.post('ContentFields/Save', { path: S.editor.path, fields: fields, language: $('#lang').val() },
            function (d) {
                if (d == 'success') {
                    S.editor.fields.changed = false;
                    //html resource has changed because content fields have changed
                    S.editor.files.html.changed = true;
                    S.message.show('.content-fields .message', 'confirm', 'Content fields were saved.', false, 4000, true);
                } else { S.editor.error(); }
            },
            function () {
                S.editor.error();
            }
        );
    }
};