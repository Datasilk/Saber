S.editor.lang = {
    supported:[],
    load: function (selector, lang, onchange) {
        S.ajax.post('Languages/Get', {},
            function (d) {
                var langs = d.split('|');
                var sel = $(selector);
                S.editor.lang.supported = [];
                for (var x = 0; x < langs.length; x++) {
                    var l = langs[x].split(',');
                    S.editor.lang.supported[l[0]] = l[1];
                    sel.append('<option value="' + l[0] + '"' + (l[0] == lang ? ' selected="selected"' : '') + '>' + l[1] + '</option>');
                }
                sel.on('change', onchange);
            }
        );
    },
    add: {
        show: function () {
            S.popup.show('New Language',
                $('#template_lang_add').html()
            );
            $('.popup form').on('submit', S.editor.lang.add.submit);
        },

        submit: function (e) {
            e.preventDefault();
            e.cancelBubble = true;
            var data = { name: $('#lang_name').val(), abbr: $('#lang_abbr').val() };
            S.ajax.post('Languages/Create', data,
                function (d) {
                    var abbr = data.abbr.toLowerCase();
                    $('.content-fields #lang').append('<option value="' + abbr + '">' + data.name + '</option>').val(abbr);
                    S.editor.fields.load();
                }
            );
            S.popup.hide();
            return false;
        }
    }
};