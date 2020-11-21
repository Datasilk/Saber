S.editor.users = {
    _loaded: false,
    parameters: {start:1, length:25, search:'', orderby:1},

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
        S.editor.tabs.create('User Management', 'users-management-section', { isPageResource: false },
            () => { //onfocus
                $('.tab.users-management').removeClass('hide');
                updateFilebar();
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        function updateFilebar() {
            S.editor.filebar.update('User Management', 'icon-users', $('#users_manage_toolbar').html());
            $('.tab-toolbar button.new-user').on('click', S.editor.users.create.show);
        }
        if (self._loaded == true) {
            S.editor.tabs.select('users-management-section');
            return;
        }
        self.search(1, 25, '', 1, updateFilebar);
    },

    search: (start, length, search, orderby, callback) => {
        S.editor.users.parameters = { start: start, length: length, search: search, orderby: orderby }
        S.ajax.post('Users/List', S.editor.users.parameters,
            function (d) {
                $('.sections > .users-management .scroller').html(d);
                S.editor.users._loaded = true;
                if (typeof callback == 'function') { callback();}
            }
        );
    },

    create: {
        show: () => {
            S.popup.show('New User', $('#template_newuser').html());
            //set up button events within popup
            $('.popup form').on('submit', (e) => {
                e.preventDefault();
                var data = {
                    emailaddr: $('#newemail').val().trim(),
                    password: $('#newpass').val(),
                    name: $('#newname').val().trim()
                };
                //validate data
                if (/^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/.test(data.emailaddr) == false) {
                    S.editor.message('.popup .msg', "Email address is not valid", "error");
                    return;
                }
                if (data.password != $('#newpass2').val()) {
                    S.editor.message('.popup .msg', "Passwords do not match", "error");
                    return;
                }
                if (data.password.length < 8 || data.password.length > 16) {
                    S.editor.message('.popup .msg', "Password must be between 8 to 16 characters long", "error");
                    return;
                }

                S.ajax.post('User/Create', data,
                    function (d) {
                        S.popup.hide();
                        var p = S.editor.users.parameters;
                        S.editor.users.search(p.start, p.length, p.search, p.orderby);
                    },
                    function (err) {
                        S.editor.message('.popup .msg', err.responseText, "error");
                    }
                );
            })
        }
    }
};