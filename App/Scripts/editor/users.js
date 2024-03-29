﻿S.editor.users = {
    _loaded: false,
    _loadedUsers: [],
    parameters: {start:1, length:25, search:'', orderby:1},

    show: (callback) => {
        var finished = false;
        var self = S.editor.users;
        S.editor.dropmenu.hide();
        if (typeof callback != 'function') { callback = null; }
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .users-management').removeClass('hide');
        $('ul.file-tabs > li').removeClass('selected');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        //load users list
        S.editor.tabs.create('User Management', 'users-management-section', { removeOnClose:true},
            () => { //onfocus
                S.editor.tabs.show('users-management');
                updateFilebar();
            },
            () => { //onblur

            },
            () => { //onsave

            },
            () => { //onclose
                $('.tab.users-settings').remove();
            }
        );
        function updateFilebar() {
            S.editor.filebar.update('User Management', 'icon-users', $('#users_manage_toolbar').html());
            $('.tab-toolbar button.new-user').on('click', S.editor.users.create.show);
            $('.tab-toolbar a.button.user-settings').on('click', S.editor.users.settings.show);
            if (finished == false && typeof callback == 'function') {
                finished = true;
                callback();
            }
        }
        if (self._loaded == true) {
            S.editor.tabs.select('users-management-section');
            if (typeof callback == 'function') { callback(); }
            return;
        }
        self.search(1, 25, '', 1, updateFilebar);
    },

    search: (start, length, search, orderby, callback) => {
        if (search == null) { search = $('#users_search').val(); }
        S.editor.users.parameters = { start: start, length: length, search: search, orderby: orderby }
        S.ajax.post('Users/List', S.editor.users.parameters,
            function (d) {
                $('.sections > .users-management .scroller').html(d);
                $('.sections > .users-management button.search').on('click', () => {
                    S.editor.users.search(1, length, null, 1);
                });
                $('.sections > .users-management .search form').on('submit', (e) => {
                    e.preventDefault();
                    S.editor.users.search(1, length, null, 1);
                });
                S.editor.users._loaded = true;
                //add event listeners
                $('.users-management tbody tr').on('click', (e) => {
                    var tr = $(S.target.find(e, 'tr'));
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
            S.popup.show('New User', $('#template_newuser').html());
            //set up button events within popup
            $('.popup form').on('submit', (e) => {
                e.preventDefault();
                var data = {
                    name: $('#newname').val().trim(),
                    emailaddr: $('#newemail').val().trim(),
                    password: $('#newpass').val(),
                    password2: $('#newpass').val()
                };
                //validate data
                if (/^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/.test(data.emailaddr) == false) {
                    S.editor.error('.popup .msg', "Email address is not valid");
                    return;
                }
                if (data.password != $('#newpass2').val()) {
                    S.editor.error('.popup .msg', "Passwords do not match");
                    return;
                }
                if (data.password.length < 8 || data.password.length > 16) {
                    S.editor.error('.popup .msg', "Password must be between 8 to 16 characters long");
                    return;
                }

                S.ajax.post('User/SignUp', data,
                    function (d) {
                        S.popup.hide();
                        var p = S.editor.users.parameters;
                        S.editor.users.search(p.start, p.length, data.emailaddr, p.orderby);
                    },
                    function (err) {
                        S.editor.error('.popup .msg', err.responseText);
                    }
                );
            })
        }
    },

    details: {
        show: (id, email) => {
            var self = S.editor.users;
            if (self._loadedUsers.filter(a => a == id).length > 0 && $('.tab-user-' + id).length > 0) {
                //tab already exists
                S.editor.tabs.select('user-' + id);
            } else {
                //create tab & load user details
                $('.tab.user-' + id).remove();
                $('.editor .sections > .tab').addClass('hide');
                $('.sections').append('<div class="tab user-' + id + '"><div class="scroller"></div></div>');
                S.editor.resize.window();

                S.editor.tabs.create('User: ' + email, 'user-' + id, { },
                    () => { //onfocus
                        S.editor.tabs.show('user-' + id);
                        self.details.updateFilebar(id, email);
                    },
                    () => { //onblur

                    },
                    () => { //onsave

                    }
                );

                S.ajax.post('Users/Details', { userId: id },
                    function (d) {
                        $('.tab.user-' + id + ' .scroller').html(d);
                        S.editor.users._loadedUsers.push(id);
                        self.details.updateFilebar(id, email);
                        //add event listeners
                        $('.tab.user-' + id + ' .btn-assign-group').on('click', () => { S.editor.users.security.assign(id); })
                        $('.tab.user-' + id + ' .user-group .btn-delete-group').on('click', (e) => {
                            var groupId = $(e.target).parents('.user-group').attr('data-id');
                            S.editor.users.security.remove(id, groupId);
                        });
                        $('.tab.user-' + id + ' .btn-save').on('click', () => {
                            var newemail = $('.tab.user-' + id + ' #user_email').val();
                            var newname = $('.tab.user-' + id + ' #user_name').val();
                            var isadmin = $('.tab.user-' + id + ' #admin_privilages')[0].checked;
                            var enabled = $('.tab.user-' + id + ' #user_enabled')[0].checked;
                            S.ajax.post('Users/Update', { userId: id, email: newemail, name: newname, isadmin: isadmin, enabled: enabled }, () => {
                                S.editor.message('.tab.user-' + id + ' .messages', 'User Information successfully updated!');
                            });
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
            S.popup.show('Assign Security Group', $('#template_assigngroup').html());
            //set up button events within popup
            $('.popup button.apply').on('click', (e) => {
                var data = {
                    userId: userId,
                    groupId: $('.popup #groupid').val()
                };

                S.ajax.post('Users/AssignGroup', data,
                    function (d) {
                        S.popup.hide();
                        S.editor.users.security.update(userId);
                    },
                    function (err) {
                        S.editor.error('.popup .msg', err.responseText);
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
                    $('.tab.user-' + userId + ' .group-list').html(d);
                    //add event listeners
                    $('.user-group .btn-delete-group').on('click', (e) => {
                        var groupId = $(e.target).parents('.user-group').attr('data-id');
                        S.editor.users.security.remove(userId, groupId);
                    });
                }
            );
        }
    },

    settings: {
        show: function () {
            $('.editor .tab.users-management').addClass('hide');
            var tab = $('.editor .tab.users-settings');
            var toolbar = $('.editor .tab-toolbar');
            toolbar.append('<button class="cancel outline">Back</button>');
            var btn = toolbar.find('a.button.user-settings');
            var cancel = $('.editor .tab-toolbar button.cancel');
            cancel.on('click', () => {
                tab.hide();
                $('.editor .tab.users-management').removeClass('hide');
                cancel.remove();
                btn[0].removeAttribute('style');
            });
            btn.hide();
            if (tab.length == 0) {
                //create new content area and load user settings
                $('.editor .sections').append('<div class="tab users-settings"></div>');
                tab = $('.editor .tab.users-settings');
                S.ajax.post('Users/RenderSettings', {}, (response) => {
                    tab.append(response);
                    //set up settings form
                    var btnsettings = $('.tab.users-settings .btn-save-settings');
                    btnsettings.hide();
                    btnsettings.on('click', S.editor.users.settings.save);

                    tab.find('select, input').on('change, keyup', () => {
                        btnsettings.show();
                    });
                    },
                    (err) => {
                        S.editor.error()
                    });

                S.editor.resize.window();
            } else {
                tab.show();
            }
        },

        save: function () {
            var data = {
                groupId: $('#users_security_group').val(),
                maxSignups: $('#users_max_signups').val(),
                maxSignupsRange: $('#users_max_signups_range').val()
            }

            S.ajax.post('Users/UpdateSettings', data, () => {
                S.editor.message(null, 'User System Settings were successfully saved');
            }, () => {
                    S.editor.error(null, 'Error saving User System Settings. Please try again or contact support.');
            });
        }
    }
};