S.editor.lang = {
    supported:[],
    load: function (selector, lang, onchange) {
        function populate() {
            var sel = $(selector);
            var langs = S.editor.lang.supported;
            sel.html('');
            for (key in langs) {
                sel.append('<option value="' + key + '"' + (key == lang ? ' selected="selected"' : '') + '>' + langs[key] + '</option>');
            }
            sel.off('change', onchange).on('change', onchange);
        }
        if (S.editor.lang.supported.length > 0) {
            populate();
        } else {
            S.ajax.post('Languages/Get', {},
                function (d) {
                    var langs = d.split('|');
                    S.editor.lang.supported = [];
                    for (var x = 0; x < langs.length; x++) {
                        var l = langs[x].split(',');
                        S.editor.lang.supported[l[0]] = l[1];
                    }
                    populate();
                }
            );
        }
    },
    add: {
        show: function (callback) {
            S.popup.show('New Language',
                $('#template_lang_add').html()
            );
            $('.popup form').on('submit', (e) => { S.editor.lang.add.submit(e, callback); });
        },

        submit: function (e, callback) {
            e.preventDefault();
            e.cancelBubble = true;
            var data = { name: $('#lang_name').val(), abbr: $('#lang_abbr').val() };
            S.ajax.post('Languages/Create', data,
                function (d) {
                    var abbr = data.abbr.toLowerCase();
                    $('.content-fields #lang').append('<option value="' + abbr + '">' + data.name + '</option>').val(abbr);
                    S.editor.lang.supported[abbr] = data.name;
                    if (callback) { callback(); }
                }
            );
            S.popup.hide();
            return false;
        }
    }
};