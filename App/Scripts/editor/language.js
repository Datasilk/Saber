S.editor.lang = {
    add: {
        show: function () {
            S.popup.show('New Language',
                S('#template_lang_add').html()
            );
            S('.popup form').on('submit', S.editor.lang.add.submit);
        },

        submit: function (e) {
            e.preventDefault();
            e.cancelBubble = true;
            var data = { name: S('#lang_name').val(), abbr: S('#lang_abbr').val() };
            S.ajax.post('Languages/Create', data,
                function (d) {
                    var abbr = data.abbr.toLowerCase();
                    S('.content-fields #lang').append('<option value="' + abbr + '">' + data.name + '</option>').val(abbr);
                    S.editor.fields.load();
                }
            );
            S.popup.hide();
            return false;
        }
    }
};