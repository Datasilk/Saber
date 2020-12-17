S.editor.fields = {
    clone: S('.content-fields .textarea-clone > div'),
    selected: '',
    changed: false,
    load: function () {
        var lang = S('#lang').val();
        S.editor.fields.changed = false;
        S('.content-fields form').html('');
        S.ajax.post('ContentFields/Render', { path: S.editor.path, language: lang },
            function (d) {
                d.selector = '.content-fields form';
                S.ajax.inject(d);
                S.editor.fields.selected = S.editor.selected;

                //add language button
                S('.content-fields .add-lang a').on('click', S.editor.lang.add.show);

                //set up events for fields
                S('.content-fields form .input-field').on('keyup, keydown, change', S.editor.fields.change).each(
                    function (field) {
                        S.editor.fields.change({ target: field });
                    }
                );

                //set up event for image selection buttons
                S('.content-fields .select-image button').on('click', (e) => {
                    e.preventDefault();
                    S.editor.resources.select(S.editor.path, '.jpg, .png, .gif', true, "Select An Image", "Select Image", (results) => {
                        var container = S(e.target).parents('.content-field');
                        var field = container.find('.input-field');
                        var newpath = S.editor.path.replace('content/', 'content/pages/') + '/';
                        var src = newpath + results[0];
                        container.find('.img').html('<div><img src="' + src + '"/></div>');
                        field.val(src);
                        S.editor.fields.save();
                    });
                });

                S('.content-fields .select-image .input-field').on('change', (e) => {
                    var container = S(e.target).parents('.content-field');
                    var field = container.find('.input-field');
                    container.find('.img').html('<div><img src="' + field.val() + '"/></div>');
                });
            },
            function () { S.editor.error(); },
            true
        );
    },
    resize: function (e) {
        if (S.editor.visible == false) { return; }
        if (e) {
            //resize field
            var field = S(e.target);
            if (field.hasClass('text-field')) {
                var clone = S.editor.fields.clone;
                clone.html(field.val().replace(/\n/g, '<br/>') + '</br>');
                field.css({ height: clone.height() });
            }
        }
    },
    change: function (e) {
        if (S.editor.visible == false) { return; }
        if (S.editor.fields.changed == false) {
            //enable save menu
            S('.item-save').removeClass('faded').removeAttr('disabled');
            S.editor.fields.changed = true;
        }
        S.editor.fields.resize(e);
    },
    save: function () {
        var fields = {};
        var texts = S('.content-fields form .input-field');
        texts.each(function (txt) {
            if (!txt.id || (txt.id && txt.id.indexOf('field_') < 0)) { return;}
            var t = S(txt);
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
                        default:
                            fields[id] = t.val();
                            break;
                    }
                    break;
            }
        });
        S.ajax.post('ContentFields/Save', { path: S.editor.path, fields: fields, language: S('#lang').val() },
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