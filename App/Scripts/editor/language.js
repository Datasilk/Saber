S.editor.lang = {
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