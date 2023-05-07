S.editor.security = {
    _loaded: false,
    _loadedGroups: [],

    show: (callback) => {
        var self = S.editor.security;
        S.editor.dropmenu.hide();
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .security-groups').removeClass('hide');
        $('ul.file-tabs > li').removeClass('selected');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        //load users list
        S.editor.tabs.create('Security Groups', 'security-groups-section', null,
            () => { //onfocus
                S.editor.tabs.show('security-groups');
                self.groups.updateFilebar();
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        if (self._loaded == true) {
            S.editor.tabs.select('security-groups-section');
            if (typeof callback == 'function') { callback(); }
            return;
        }
        self.groups.load(callback);
    },

    groups: {
        load: (callback) => {
            //load security groups
            var self = S.editor.security;
            S.ajax.post('Security/Groups', {},
                function (d) {
                    $('.sections > .security-groups .scroller').html(d);
                    S.editor.security._loaded = true;
                    self.groups.updateFilebar();
                    //add event listeners
                    $('table.groups tbody tr').on('click', (e) => {
                        var tr = S.target.find(e, 'tr');
                        var id = tr.getAttribute('data-id');
                        var name = $(tr).children().first().html().trim();

                        //show tab for security group
                        self.group.load(id, name);
                    });
                    if (typeof callback == 'function') { callback(); }
                }
            );
        },
        updateFilebar:() => {
            S.editor.filebar.update('Security Groups', 'icon-security', $('#security_groups_toolbar').html());
            $('.tab-toolbar button.new-group').on('click', S.editor.security.groups.create.show);
        },

        create: {
            show: () => {
                S.popup.show('New Security Group', $('#template_newgroup').html());
                //set up button events within popup
                $('.popup form').on('submit', (e) => {
                    e.preventDefault();
                    var data = {
                        name: $('#newname').val().trim()
                    };

                    S.ajax.post('Security/CreateGroup', data,
                        function (d) {
                            S.popup.hide();
                            S.editor.security.groups.load();
                        },
                        function (err) {
                            S.editor.error('.popup .msg', err.responseText);
                        }
                    );
                });
            }
        }
    },

    group: {
        load: (id, name) => {
            var self = S.editor.security;
            if (self._loadedGroups.filter(a => a == id).length > 0 && $('.tab-security-group-' + id).length > 0) {
                //tab already exists
                S.editor.tabs.select('security-group-' + id);
            } else {
                //create tab & load security group details
                $('.tab.security-group-' + id).remove();
                $('.editor .sections > .tab').addClass('hide');
                $('.sections').append('<div class="tab security-group-' + id + '"><div class="scroller"></div></div>');
                S.editor.resize.window();

                S.editor.tabs.create('Security Group: ' + name, 'security-group-' + id, null,
                    () => { //onfocus
                        S.editor.tabs.show('security-group-' + id);
                        self.group.updateFilebar(id, name);
                    },
                    () => { //onblur

                    },
                    () => { //onsave

                    }
                );

                S.ajax.post('Security/GroupDetails', {groupId:id},
                    function (d) {
                        $('.tab.security-group-' + id + ' .scroller').html(d);
                        S.editor.security._loadedGroups.push(id);
                        self.group.updateFilebar(id, name);
                        //add event listeners
                        $('.security-group-' + id + ' input[type="checkbox"]').on('change', (e) => { self.group.savekey(e, id); });
                        $('.security-group-' + id + ' button.delete').on('click', () => { self.group.delete(id, name); });
                    }
                );
            }
        },

        updateFilebar: (id, name) => {
            S.editor.filebar.update('Security Group: ' + name, 'icon-security');
        },

        savekey: (e, id) => {
            var chk = e.target;
            var data = {
                groupId: id,
                key: chk.name,
                value: chk.checked
            }
            //update security group tab
            var tr = $('table.groups tr[data-id="' + id + '"] td:nth-child(2) span');
            if (tr) {
                tr.html($('.tab.security-group-' + id + ' input[type="checkbox"]:checked').map((i, a) => a.name).join(', '));
            }

            S.ajax.post('Security/SaveKey', data, null, () => {
                S.editor.error('', "An error occurred when trying to save your security group changes");
            }
            );
        },

        delete: (id, name) => {
            if (window.parent.confirm('Do you really want to delete the security group "' + name + '"? This cannot be undone.')) {
                S.ajax.post('Security/DeleteGroup', { groupId: id },
                    () => {
                        //remove section
                        $('.tab.security-group-' + id).remove();
                        //remove tab
                        S.editor.tabs.close('security-group-' + id, 'security-group-' + id);
                        //remove row in table within security group tab
                        $('table.groups tr[data-id="' + id + '"]').remove();
                    },
                    () => {
                        S.editor.error('', "An error occurred when trying to save your security group changes");
                    });
            }
        }
    }
};