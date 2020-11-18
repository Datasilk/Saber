S.editor.users = {
    _loaded:false,
    show: () => {
        var self = S.editor.users;
        S.editor.dropmenu.hide();
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .users-management').removeClass('hide');
        $('ul.file-tabs > li').removeClass('selected');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        //load users list
        S.editor.tabs.create("User Management", "users-management-section", { isPageResource: false },
            () => { //onfocus
                $('.tab.users-management').removeClass('hide');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        if (self._loaded == true) { return; }
        self.search(1, 25, '', 1);
    },

    search: (start, length, search, orderby) => {
        S.ajax.post('Users/List', {start:start, length:length, search:search, orderby:orderby},
            function (d) {
                $('.sections > .users-management').html(d);
                S.editor.users._loaded = true;
            }
        );
    }
};