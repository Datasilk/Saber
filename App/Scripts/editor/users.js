S.editor.users = {
    _loaded: false,
    _loadedUsers: [],
    parameters: {start:1, length:25, search:'', orderby:1},

    show: () => {
        var self = S.editor.users;
        S.editor.dropmenu.hide();
        S('.editor .sections > .tab').addClass('hide');
        S('.editor .sections > .users-management').removeClass('hide');
        S('ul.file-tabs > li').removeClass('selected');

        //disable save menu
        S('.item-save').addClass('faded').attr('disabled', 'disabled');
        S('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        //load users list
        S.editor.tabs.create('User Management', 'users-management-section', {},
            () => { //onfocus
                S('.tab.users-management').removeClass('hide');
                updateFilebar();
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        function updateFilebar() {
            S.editor.filebar.update('User Management', 'icon-users', S('#users_manage_toolbar').html());
            S('.tab-toolbar button.new-user').on('click', S.editor.users.create.show);
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
                S('.sections > .users-management .scroller').html(d);
                S.editor.users._loaded = true;
                //add event listeners
                S('.users-management tbody tr').on('click', (e) => {
                    var tr = S(S.target.find(e, 'tr'));
                    var id = tr.children().first().html().trim();
                    var email = tr.children()[1].innerHTML.trim();
                    S.editor.users.details.show(id, email);
                });
                if (typeof callback == 'function') { callback();}
            }
        );
    },

    create: {
        show: () => {
            S.popup.show('New User', S('#template_newuser').html());
            //set up button events within popup
            S('.popup form').on('submit', (e) => {
                e.preventDefault();
                var data = {
                    emailaddr: S('#newemail').val().trim(),
                    password: S('#newpass').val(),
                    name: S('#newname').val().trim()
                };
                //validate data
                if (/^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/.test(data.emailaddr) == false) {
                    S.editor.message('.popup .msg', "Email address is not valid", "error");
                    return;
                }
                if (data.password != S('#newpass2').val()) {
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
    },

    details: {
        show: (id, email) => {
            var self = S.editor.users;
            if (self._loadedUsers.filter(a => a == id).length > 0 && S('.tab-user-' + id).length > 0) {
                //tab already exists
                S.editor.tabs.select('user-' + id);
            } else {
                //create tab & load user details
                S('.tab.user-' + id).remove();
                S('.editor .sections > .tab').addClass('hide');
                S('.sections').append('<div class="tab user-' + id + '"><div class="scroller"></div></div>');
                S.editor.resizeWindow();

                S.editor.tabs.create('User: ' + email, 'user-' + id, { isPageResource: false },
                    () => { //onfocus
                        S('.tab.user-' + id).removeClass('hide');
                        self.details.updateFilebar(id, email);
                    },
                    () => { //onblur

                    },
                    () => { //onsave

                    }
                );

                S.ajax.post('Users/Details', { userId: id },
                    function (d) {
                        S('.tab.user-' + id + ' .scroller').html(d);
                        S.editor.users._loadedUsers.push(id);
                        self.details.updateFilebar(id, email);
                        //add event listeners
                        S('.btn-assign-group').on('click', () => { S.editor.users.security.assign(id); })
                        S('.user-group .btn-delete-group').on('click', (e) => {
                            var groupId = S(e.target).parents('.user-group').attr('data-id');
                            S.editor.users.security.remove(id, groupId);
                        });
                    }
                );
            }
        },

        updateFilebar: (id, email) => {
            S.editor.filebar.update('User: ' + email, 'icon-users');
        }
    },

    security: {
        assign: (userId) => {
            S.popup.show('Assign Security Group', S('#template_assigngroup').html());
            //set up button events within popup
            S('.popup button.apply').on('click', (e) => {
                var data = {
                    userId: userId,
                    groupId: S('.popup #groupid').val()
                };

                S.ajax.post('Users/AssignGroup', data,
                    function (d) {
                        S.popup.hide();
                        S.editor.users.security.update(userId);
                    },
                    function (err) {
                        S.editor.message('.popup .msg', err.responseText, "error");
                    }
                );
            });
        },
        remove: (userId, groupId) => {
            S.ajax.post('Users/RemoveGroup', { userId: userId, groupId: groupId },
                function (d) {
                    S.editor.users.security.update(userId);
                }
            );
        },
        update: (userId) => {
            S.ajax.post('Users/AssignedGroups', { userId: userId },
                function (d) {
                    S('.tab.user-' + userId + ' .group-list').html(d);
                    //add event listeners
                    S('.user-group .btn-delete-group').on('click', (e) => {
                        var groupId = S(e.target).parents('.user-group').attr('data-id');
                        S.editor.users.security.remove(userId, groupId);
                    });
                }
            );
        }
    }
};