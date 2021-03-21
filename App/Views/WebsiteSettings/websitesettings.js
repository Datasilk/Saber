(function () {
    //load accordion functionality 
    S.accordion.load({}, () => { S.editor.resize.window(); }); 

    //set up icon buttons
    var selbtn = {};
    var icons = [
        { name: 'web', type: 0 },
        { name: 'apple', type: 1 },
        { name: 'android', type: 2 },
    ];
    for (var x = 0; x < icons.length; x++) {
        S('.' + icons[x].name + '-app-icon button').on('click', (a) => {
            var btn = S(a.target);
            var container = btn.parents('.icon-box').first();
            var img = container.find('.icon-img img');
            var px = btn.attr('px') || '0';
            var icon = icons.filter(a => a.name == btn.attr('for'))[0];
            selbtn = {
                name: icon.name,
                type: icon.type,
                btn: btn,
                img: img,
                px: px,
                suffix: px != '0' ? '-' + px + 'x' + px : '-icon',
                path: px != '0' ? 'mobile/' : ''
            };
            upload_app_icon.click();
        });
    }

    S('#upload_app_icon').on("change", (e) => {
        S.loader();
        S.upload.file(upload_app_icon.files[0], '/api/WebsiteSettings/UploadPngIcon?type=' + selbtn.type + '&px=' + selbtn.px,
            null, //onprogress
            (e) => { //oncomplete
                //update icon
                setTimeout(function () {
                    selbtn.img.attr('src', '/images/' + selbtn.path + selbtn.name + selbtn.suffix + '.png?r=' + Math.round(Math.random() * 9999));
                    selbtn.img.parent().removeClass('hide');
                }, 1000);
                S('.loader').remove();
            },
            (e) => { //onerror
                S.editor.error(null, e.responseText);
                S('.loader').remove();
            }
        );
    });

    //set up stylesheets
    S('.website-styles .btn-add-style').on('click', () => {
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
    });

    function removeStyle(e) {
        var target = S(S.target.findByClassName(e, 'close-btn'));
        var data = { file: target.attr('data-path') };
        S.ajax.post('WebsiteSettings/RemoveStylesheet', data, (list) => {
            //add stylesheets to list
            S.editor.files.less.changed = true;
            S('.website-styles-list > ul').html(list);
            initStyles();
        });
    }

    function initStyles() {
        S('.website-styles-list .close-btn').on('click', removeStyle);
        S.drag.sort.add('.website-styles-list ul', '.website-styles-list li', () => {
            //update website.config with new stylesheet sort order
            S.ajax.post('WebsiteSettings/SortStylesheets', {
                stylesheets: $('.website-styles-list li div[data-path]').map((i, a) => $(a).attr('data-path'))
            }, () => { },
            (err) => {
                S.editor.error('', err.responseText);
            });
        });
    }
    initStyles();

    //set up scripts
    S('.website-scripts .btn-add-script').on('click', () => {
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
    });

    function removeScript(e) {
        var target = S(S.target.findByClassName(e, 'close-btn'));
        var data = { file: target.attr('data-path') };
        S.ajax.post('WebsiteSettings/RemoveScript', data, (list) => {
            //add scripts to list
            S.editor.files.js.changed = true;
            S('.website-scripts-list > ul').html(list);
            initScripts();
        });
    }

    function initScripts() {
        S('.website-scripts-list .close-btn').on('click', removeScript);
        S.drag.sort.add('.website-scripts-list ul', '.website-scripts-list li', () => {
            //update website.config with new script sort order
            S.ajax.post('WebsiteSettings/SortScripts', {
                scripts: $('.website-scripts-list li div[data-path]').map((i, a) => $(a).attr('data-path'))
            }, () => { },
            (err) => {
                S.editor.error('', err.responseText);
            });
        });
    }
    initScripts();

    //set up email settings
    S('#emailclients').on('change', () => {
        var id = S('#emailclients').val();
        S('.email-client').addClass('hide');
        S('.email-client.client-' + id).removeClass('hide');
    });

    S('.email-clients button.save').on('click', (e) => {
        //save selected email client settings
        e.preventDefault();
        var section = S('.email-client:not(.hide)').first();
        var id = section[0].className.replace('email-client client-', '');
        var inputs = section.find('input, select');
        var params = {};
        inputs.each(a => {
            var b = S(a);
            var type = b.attr('type') ?? '';
            params[b.attr('id').replace(id + '_', '')] = (
                type == 'checkbox' ? (a.checked == true ? 'True' : 'False') : b.val()
            );
        });
        var data = {
            id: id,
            parameters: params
        };

        S.ajax.post('WebsiteSettings/SaveEmailClient', data, () => {
            S.editor.message('', 'Email Client settings saved successfully');
        }, (err) => {
            S.editor.error('', err.responseText);
        });
    });

    S('.email-actions button.save').on('click', (e) => {
        //save email actions
        e.preventDefault();
        var actions = S('.email-action');
        var data = {
            actions: []
        };
        for (var x = 0; x < actions.length; x++) {
            var action = S(actions[x]);
            var key = action.attr('data-key');
            data.actions.push({
                Type: key,
                Client: S('#emailaction_' + key).val(),
                Subject: S('#emailaction_' + key + '_subject').val() ?? ''
            });
        }

        S.ajax.post('WebsiteSettings/SaveEmailActions', data, () => {
            S.editor.message('', 'Email Action settings saved successfully');
        }, (err) => {
            S.editor.error('', err.responseText);
        });
    });

    S('.passwords button.save').on('click', (e) => {
        //save email actions
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
    });

    S('.plugin-info button.delete').on('click', (e) => {
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
    });
})();