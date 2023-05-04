S.editor.websettings.init = function () {
    console.log(this);
    //load accordion functionality 
    S.accordion.load({}, () => { S.editor.resize.window(); });

    //set up icon buttons
    var icons = S.editor.websettings.icons.list;
    for (var x = 0; x < icons.length; x++) {
        S('.' + icons[x].name + '-app-icon button').on('click', this.icons.upload);
    }
    S('#upload_app_icon').on("change", this.icons.upload);

    //set up stylesheets
    S('.website-styles .btn-add-style').on('click', this.styles.add);
    S('.website-styles-list .close-btn').on('click', this.styles.remove);
    S.drag.sort.add('.website-styles-list ul', '.website-styles-list li', this.styles.drag);

    //set up scripts
    S('.website-scripts .btn-add-script').on('click', this.scripts.add);
    S('.website-scripts-list .close-btn').on('click', this.scripts.remove);
    S.drag.sort.add('.website-scripts-list ul', '.website-scripts-list li', this.scripts.drag);

    //set up email settings
    $('.email-clients .btn-add-client').on('click', this.email.clients.add.show);
    S.editor.websettings.email.clients.init.call(this);
    S.editor.websettings.email.actions.init.call(this);

    //set up password settings
    S('.passwords button.save').on('click', S.editor.websettings.pass.save);

    //set up plugins
    S('.plugin-info button.delete').on('click', S.editor.websettings.plugins.delete);

    //set up Public APIs
    $('#publicapis_enabled').on('change', S.editor.websettings.apis.enabled);
    $('.public-api-checkbox').on('change', S.editor.websettings.apis.toggle);
};

S.editor.websettings.styles = {
    add: function () {
        S.popup.show('Add Stylesheet to Page',
            S('#website_styles_add').html()
        );

        //get list of available stylesheets
        S.ajax.post('WebsiteSettings/GetAvailableStylesheets', {}, (list) => {
            var html = [];
            list = JSON.parse(list);
            for (var x = 0; x < list.length; x++) {
                html.push('<option value="' + list[x] + '">' + list[x] + '</option>');
            }
            S('#available_styles').html(html.join('\n'));
        });

        S('.popup form').on('submit', (e) => {
            e.preventDefault();
            e.cancelBubble = true;
            var data = { file: S('#available_styles').val() };
            S.ajax.post('WebsiteSettings/AddStylesheetToSite', data, (list) => {
                //add stylesheets to list
                S.util.css.load(data.file, 'css_' + data.file.replace(/\//g, '_').replace(/\./g, '_'), window.parent.document);
                S.editor.files.less.changed = true;
                S('.website-styles-list > ul').html(list);
                initStyles();
                S.popup.hide();
            });
        });
    },

    remove: function (e) {
        var target = S(S.target.findByClassName(e, 'close-btn'));
        var data = { file: target.attr('data-path') };
        S.ajax.post('WebsiteSettings/RemoveStylesheet', data, (list) => {
            //add stylesheets to list
            S.editor.files.less.changed = true;
            S('.website-styles-list > ul').html(list);
            initStyles();
        });
    },

    drag: function () {
        //update website.config with new stylesheet sort order
        S.ajax.post('WebsiteSettings/SortStylesheets', {
            stylesheets: $('.website-styles-list li div[data-path]').map((i, a) => $(a).attr('data-path'))
        }, () => { },
        (err) => {
            S.editor.error('', err.responseText);
        });
    }
};

S.editor.websettings.scripts = {
    add: function () {
        S.popup.show('Add Script to Page',
            S('#website_scripts_add').html()
        );

        //get list of available stylesheets
        S.ajax.post('WebsiteSettings/GetAvailableScripts', {}, (list) => {
            var html = [];
            list = JSON.parse(list);
            for (var x = 0; x < list.length; x++) {
                html.push('<option value="' + list[x] + '">' + list[x] + '</option>');
            }
            S('#available_scripts').html(html.join('\n'));
        });

        S('.popup form').on('submit', (e) => {
            e.preventDefault();
            e.cancelBubble = true;
            var data = { file: S('#available_scripts').val() };
            S.ajax.post('WebsiteSettings/AddScriptToSite', data, (list) => {
                //add scripts to list
                S.util.js.load(data.file, 'js_' + data.file.replace(/\//g, '_').replace(/\./g, '_'), null, null, window.parent.document);
                S.editor.files.js.changed = true;
                S('.website-scripts-list > ul').html(list);
                initScripts();
                S.popup.hide();
            });
        });
    },

    remove: function (e) {
        var target = S(S.target.findByClassName(e, 'close-btn'));
        var data = { file: target.attr('data-path') };
        S.ajax.post('WebsiteSettings/RemoveScript', data, (list) => {
            //add scripts to list
            S.editor.files.js.changed = true;
            S('.website-scripts-list > ul').html(list);
            initScripts();
        });
    },

    drag: function () {
        //update website.config with new script sort order
        S.ajax.post('WebsiteSettings/SortScripts', {
            scripts: $('.website-scripts-list li div[data-path]').map((i, a) => $(a).attr('data-path'))
        }, () => { },
        (err) => {
            S.editor.error('', err.responseText);
        });
    }
};

S.editor.websettings.icons = {
    list: [
        { name: 'web', type: 0 },
        { name: 'apple', type: 1 },
        { name: 'android', type: 2 },
    ],

    upload: function (e) {
        S.loader();
        var btn = S(e.target);
        var container = btn.parents('.icon-box').first();
        var img = container.find('.icon-img img');
        var px = btn.attr('px') || '0';
        var icons = S.editor.websettings.icons.list;
        var icon = icons.filter(a => a.name == btn.attr('for'))[0];
        var item = {
            name: icon.name,
            type: icon.type,
            btn: btn,
            img: img,
            px: px,
            suffix: px != '0' ? '-' + px + 'x' + px : '-icon',
            path: px != '0' ? 'mobile/' : ''
        };
        S.upload.file(upload_app_icon.files[0], '/api/WebsiteSettings/UploadPngIcon?type=' + item.type + '&px=' + item.px,
            null, //onprogress
            (e) => { //oncomplete
                //update icon
                setTimeout(function () {
                    item.img.attr('src', '/images/' + item.path + item.name + item.suffix + '.png?r=' + Math.round(Math.random() * 9999));
                    item.img.parent().removeClass('hide');
                }, 1000);
                S('.loader').remove();
            },
            (e) => { //onerror
                S.editor.error(null, e.responseText);
                S('.loader').remove();
            }
        );
    }
}

S.editor.websettings.email = {
    clients: {
        init: function () {
            $('.email-clients tbody td:not(:last-child)').on('click', this.email.clients.edit);
        },
        update: function () {
            S.ajax.post("WebsiteSettings/RenderEmailClients", {}, (response) => {
                $('.email-clients .email-contents').html(response);
                S.editor.websettings.email.clients.init.call(S.editor.websettings);
            });
        },
        add: {
            show: function (id) {
                if (typeof id != 'string') { id = ''; }
                S.ajax.post('WebsiteSettings/RenderEmailClient', {clientId:id}, (response) => {
                    S.popup.show(id == null ? 'Add Email Client' : 'Edit Email Client', response, { width: '100%', maxWidth: 420 });
                    $('.popup.show #emailclients').on('input', () => {
                        var key = $('.popup.show #emailclients').val();
                        $('.popup.show .email-client').hide();
                        $('.popup.show .email-client.client-' + key).show();
                    });
                    $('.popup.show button.save').on('click', S.editor.websettings.email.clients.add.submit);
                });
            },

            submit: function (e) {
                //save selected email client settings
                e.preventDefault();
                var section = S('.popup.show .email-client:not(.hide)').first();
                var key = section[0].className.replace('email-client client-', '');
                var inputs = section.find('input, select');
                var params = {};
                inputs.each((i, a) => {
                    var b = S(a);
                    var type = b.attr('type') ?? '';
                    params[b.attr('id').replace(key + '_', '')] = (
                        type == 'checkbox' ? (a.checked == true ? 'True' : 'False') : b.val()
                    );
                });
                var data = {
                    clientId: $('.popup.show #client_id').val(),
                    label: $('.popup.show #client_label').val(),
                    key: key,
                    parameters: params
                };

                S.ajax.post('WebsiteSettings/SaveEmailClient', data, () => {
                    S.editor.message('.popup.show .messages', 'Email Client settings saved successfully');
                    S.editor.websettings.email.clients.update();
                    if (data.clientId == '') { S.popup.hide(); }
                }, (err) => {
                    S.editor.error('.popup.show .messages', err.responseText);
                });
            }
        },
        edit: function (e) {
            var target = $(e.target);
            if (target.parents('.del').length > 0) { return; }
            var id = target.parents('tr').attr('data-id');
            S.editor.websettings.email.clients.add.show(id);
        },
        remove: function (id) {
            if (window.parent.confirm('Do you really want to remove this Email Client? Any Email Actions that rely on this client will no longer work')) {
                S.ajax.post('WebsiteSettings/RemoveEmailClient', { clientId: id }, () => {
                    S.editor.websettings.email.clients.update();
                });
            }
        }
    },

    actions: {
        init: function () {
            $('.email-actions .send-test-email').on('click', this.email.actions.sendTest);
            $('.email-actions .edit-action').on('click', this.email.actions.edit)
        },
        update: function () {
            S.ajax.post('WebsiteSettings/RenderEmailActions', { }, (response) => {
                $('.email-actions .email-contents').html(response);
                S.editor.websettings.email.actions.init.call(S.editor.websettings);
            });
        },
        edit: function (e) {
            var target = $(e.target);
            var key = target.parents('.email-action').first().attr('data-key');
            S.ajax.post('WebsiteSettings/RenderEmailAction', { key: key }, (response) => {
                S.popup.show('Edit Email Action', response, { width: '100%', maxWidth: 420 });
                $('.popup.show button.save').on('click', S.editor.websettings.email.actions.save);
            });
        },
        save: function(e) {
            e.preventDefault();
            var data = {
                type: $('.popup.show #action_key').val(),
                clientId: S('.popup.show #emailclients').val(),
                subject: S('.popup.show #action_subject').val() ?? ''
            }

            S.ajax.post('WebsiteSettings/SaveEmailAction', data, () => {
                S.editor.message('', 'Email Action settings saved successfully');
                S.editor.websettings.email.actions.update();
                S.popup.hide();
            }, (err) => {
                S.editor.error('.popup.show .messages', err.responseText);
            });
        },
        sendTest: function (e) {
            var target = $(e.target);
            var key = target.parents('.email-action').first().attr('data-key');
            S.popup.show('Send test email',
                S('#template_test_email').html()
            );
            var btn = $('.popup.show button.apply')
            btn.on('click', () => {
                //send test email
                btn.hide();
                S.ajax.post('WebsiteSettings/SendTestEmail', { key: key, email: $('#test_email').val() }, (response) => {
                    S.editor.message('.popup.show .messages', 'Email sent successfully');
                    btn.show();
                }, (err) => {
                    S.editor.message('.popup.show .messages', err.responseText, 'error');
                    btn.show();
                });
            });
        }
    }
};

S.editor.websettings.pass = {
    save: function (e) {
        e.preventDefault();
        var data = {
            passwords: {
                MinChars: parseInt(S('#pass_minchars').val()),
                MaxChars: parseInt(S('#pass_maxchars').val()),
                MinNumbers: parseInt(S('#pass_minnumbers').val()),
                MinUppercase: parseInt(S('#pass_minuppercase').val()),
                MinSpecialChars: parseInt(S('#pass_minspecial').val()),
                MaxConsecutiveChars: parseInt(S('#pass_consecutivechars').val()),
                NoSpaces: S('#pass_nospaces')[0].checked
            }
        };
        S.ajax.post('WebsiteSettings/SavePasswords', data, () => {
            S.editor.message('', 'Password settings saved successfully');
        }, (err) => {
            S.editor.error('', err.responseText);
        });
    }
};

S.editor.websettings.plugins = {
    delete: function (e) {
        var target = S(e.target).parents('.plugin-info').first();
        var key = S(target).attr('data-key');
        if (confirm('Do you really want to uninstall the ' + key + ' plugin? This cannot be undone and the Saber application service will be terminated as a result. You may have to restart your web server if you cannot access your website afterwards.')) {
            S.ajax.post('WebsiteSettings/UninstallPlugin', { key: key }, () => {
                S.editor.message('', 'Plugin ' + key + ' was marked for uninstallation. Terminating Saber application service. Please wait...');
                setTimeout(() => { location.reload(); }, 3000);
            }, (err) => {
                S.editor.error('', err.responseText);
            });
        }
    }
};

S.editor.websettings.apis = {
    enabled: function () {
        var api = $('#publicapis_enabled').val();
        var enabled = $('#publicapis_enabled')[0].checked;
        S.ajax.post('WebsiteSettings/SavePublicApi', { api: api, enabled: enabled });
    },
    toggle: function (e) {
        var target = $(e.target);
        var api = target.val();
        var enabled = target[0].checked;
        S.ajax.post('WebsiteSettings/SavePublicApi', { api: api, enabled: enabled });
    }
};

S.editor.websettings.init.call(S.editor.websettings);