S.editor.settings = {
    _loaded: false,
    clone: null,
    headers: [],
    footers: [],
    field_template: '',

    load: function () {
        var self = S.editor.settings;
        S.editor.tabs.create('Page Settings', 'page-settings-section', { isPageResource: true },
            () => { //onfocus
                S('.tab.page-settings').removeClass('hide');
                var path = S.editor.path.substr(8);
                S.editor.filebar.update('Page Settings for <a href="/' + path + '">' + path + '</a>', 'icon-settings');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
        if (self._loaded == true) {
            S.editor.tabs.select('page-settings-section');
            return;
        }
        var path = S.editor.path;
        S.ajax.post('PageSettings/Render', { path: path },
            function (d) {
                var data = JSON.parse(d);
                var json = JSON.parse(data.json);
                S.ajax.inject(data);

                //load settings header & footer fields
                S.editor.settings.headers = json.headers;
                S.editor.settings.footers = json.footers;
                S.editor.settings.field_template = json.field_template;

                //update settings header & footer events
                S('#page_header').on('change', S.editor.settings.partials.header.update);
                S('#page_footer').on('change', S.editor.settings.partials.footer.update);
                S('.settings-header-footer input[type="text"]').on('keyup', () => {
                    S.editor.settings.partials.changed = true;
                })

                //set up settings title
                S.editor.settings._loaded = true;
                S.editor.settings.clone = S('.page-settings .textarea-clone > div');
                var p = path.replace('content/', '');
                S('.editor .page-name').attr('href', '/' + p).html(p);
                S.editor.resizeWindow();

                //set up events to detect changes
                var description = S('#page_description');
                S('#page_title_prefix, #page_title_suffix, #page_title').on('change, keyup', self.title.change);
                description.on('change, keyup, keydown', self.description.change);
                self.change(description, true);

                //set up button events
                S('.page-settings .title-prefix .icon a').on('click', S.editor.settings.title.prefix.show);
                S('.page-settings .title-suffix .icon a').on('click', S.editor.settings.title.suffix.show);
                S('.page-styles .btn-add-style').on('click', S.editor.settings.styles.add.show);
                S('.page-scripts .btn-add-script').on('click', S.editor.settings.scripts.add.show);
                S('.page-security .btn-add-group').on('click', S.editor.settings.security.add.show);
                S('.editor .styles-list .close-btn').on('click', S.editor.settings.styles.remove);
                S('.editor .scripts-list .close-btn').on('click', S.editor.settings.scripts.remove);
                S('.editor .security-list .close-btn').on('click', S.editor.settings.security.remove);
            }
        );
    },

    change: function (field, changed) {
        //update textarea height for given field
        if (S.editor.visible == true) {
            var clone = S.editor.settings.clone;
            clone.html(field.val().replace(/\n/g, '<br/>') + '</br>');
            field.css({ height: clone.height() });
            if (changed == false) {
                //enable save menu
                S('.editor .item-save').removeClass('faded').removeAttr('disabled');
            }
        }
    },

    title: {
        _timer: null,
        changed: false,

        prefix: {
            show: function () {
                S.popup.show('New Page Title Prefix',
                    S('#template_pagetitle_newprefix').html()
                );
                S('.popup form').on('submit', S.editor.settings.title.prefix.submit);
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { title: S('#page_title_new_prefix').val(), prefix: true };
                S.ajax.post('PageSettings/CreatePageTitlePart', data,
                    function (d) {
                        S('#page_title_prefix').append('<option value="' + d + '">' + d + '</option>').val(d);
                        S.editor.settings.title.change();
                    }
                );
                S.popup.hide();
                return false;
            }
        },

        suffix: {
            show: function () {
                S.popup.show('New Page Title Suffix',
                    S('#template_pagetitle_newsuffix').html()
                );
                S('.popup form').on('submit', S.editor.settings.title.suffix.submit);
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { title: S('#page_title_new_suffix').val(), prefix: false };
                S.ajax.post('PageSettings/CreatePageTitlePart', data,
                    function (d) {
                        S('#page_title_suffix').append('<option value="' + d + '">' + d + '</option>').val(d);
                        S.editor.settings.title.change();
                    }
                );
                S.popup.hide();
                return false;
            }
        },

        change: function () {
            var prefix = S('#page_title_prefix').val();
            var suffix = S('#page_title_suffix').val();
            if (prefix != '' && prefix[prefix.length - 1] != ' ') { prefix += ' '; }
            if (suffix != '' && suffix[0] != ' ') { suffix = ' ' + suffix; }
            window.document.title = prefix + S('#page_title').val() + suffix;
            S('.item-save').removeClass('faded').removeAttr('disabled');
            S.editor.settings.title.changed = true;
        },

        save: function (callback) {
            var data = {
                path: S.editor.path,
                prefix: S('#page_title_prefix').val(),
                suffix: S('#page_title_suffix').val(),
                title: S('#page_title').val()
            };
            S.ajax.post('PageSettings/UpdatePageTitle', data, callback,
                function () { S.editor.error(); }
            );
        }
    },

    description: {
        changed: false,
        change: function () {
            var description = S('#page_description');
            S.editor.settings.change(description, S.editor.settings.description.changed);
            S.editor.settings.description.changed = true;
            S('.item-save').removeClass('faded').removeAttr('disabled');
        },

        save: function (callback) {
            var data = {
                path: S.editor.path,
                description: S('#page_description').val()
            };
            S.ajax.post('PageSettings/UpdatePageDescription', data, callback,
                function () { S.editor.error(); }
            );
        }
    },

    partials: {
        changed: false,

        header: {
            update: function () {
                S.editor.settings.partials.changed = true;
                var template = S.editor.settings.field_template;
                var headers = S.editor.settings.headers;
                var header = headers[headers.map(a => a.file).indexOf(S('#page_header').val())];
                html = [];
                for (field in header.fields) {
                    html.push(template
                        .replace('{{label}}', S.util.str.Capitalize(field.replace('-', ' ')))
                        .replace('{{name}}', field)
                        .replace('{{value}}', header.fields[field])
                    );
                }
                S('.header-fields .fields').html(html.join('\n'));
            }
        },

        footer: {
            update: function () {
                S.editor.settings.partials.changed = true;
                var template = S.editor.settings.field_template;
                var footers = S.editor.settings.footers;
                var footer = footers[footers.map(a => a.file).indexOf(S('#page_footer').val())];
                html = [];
                for (field in footer.fields) {
                    html.push(template
                        .replace('{{label}}', S.util.str.Capitalize(field.replace('-', ' ')))
                        .replace('{{name}}', field)
                        .replace('{{value}}', footer.fields[field])
                    );
                }
                S('.footer-fields .fields').html(html.join('\n'));
            }
        },

        save: function (callback) {
            //get list of field values
            var header_fields = {};
            var footer_fields = {};
            var elems = S('.header-fields .fields input');
            elems.each(a => {
                header_fields[a.name] = S(a).val();
            });
            elems = S('.footer-fields .fields input');
            elems.each(a => {
                footer_fields[a.name] = S(a).val();
            });
            var data = {
                path: S.editor.path,
                header: { file: S('#page_header').val(), fields: header_fields },
                footer: { file: S('#page_footer').val(), fields: footer_fields }
            };
            S.ajax.post('PageSettings/UpdatePagePartials', data,
                function (d) {
                    //html resource has changed because header & footer partials have changed
                    S.editor.files.html.changed = true;
                    if (callback) { callback(); }
                },
                function () { S.editor.error(); }
            );
        }

    },

    styles: {
        add: {
            show: function () {
                S.popup.show('Add Stylesheet to Page',
                    S('#template_styles_add').html()
                );
                S('.popup form').on('submit', S.editor.settings.styles.add.submit);

                //get list of available scripts
                S.ajax.post('PageSettings/GetAvailableStylesheets', {}, (list) => {
                    var html = [];
                    list = JSON.parse(list);
                    for (var x = 0; x < list.length; x++) {
                        html.push('<option value="' + list[x] + '">' + list[x] + '</option>');
                    }
                    S('#available_styles').html(html.join('\n'));

                });
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { file: S('#available_styles').val(), path: S.editor.path };
                S.ajax.post('PageSettings/AddStylesheetToPage', data, (list) => {
                    //add stylesheet to page
                    var style = document.createElement('link');
                    style.rel = 'stylesheet';
                    style.href = data.file;
                    document.head.appendChild(style);
                    S.editor.files.less.changed = true;
                    S('.styles-list > ul').html(list);
                    S('.editor .styles-list .close-btn').on('click', S.editor.settings.styles.remove);
                    S.popup.hide();
                });
            }
        },

        remove: function (e) {
            var target = S(S.target.findByClassName(e, 'close-btn'));
            var data = { file: target.attr('data-path'), path: S.editor.path };
            S.ajax.post('PageSettings/RemoveStylesheetFromPage', data, (list) => {
                //add script to page
                S.editor.files.less.changed = true;
                S('.styles-list > ul').html(list);
                S('.editor .styles-list .close-btn').on('click', S.editor.settings.styles.remove);
            });
        }
    },

    scripts: {
        add: {
            show: function () {
                S.popup.show('Add Script to Page',
                    S('#template_scripts_add').html()
                );
                S('.popup form').on('submit', S.editor.settings.scripts.add.submit);

                //get list of available scripts
                S.ajax.post('PageSettings/GetAvailableScripts', {}, (list) => {
                    var html = [];
                    list = JSON.parse(list);
                    for (var x = 0; x < list.length; x++) {
                        html.push('<option value="' + list[x] + '">' + list[x] + '</option>');
                    }
                    S('#available_scripts').html(html.join('\n'));

                });
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { file: S('#available_scripts').val(), path: S.editor.path };
                S.ajax.post('PageSettings/AddScriptToPage', data, (list) => {
                    //add script to page
                    var script = document.createElement('script');
                    script.src = data.file;
                    document.body.appendChild(script);
                    S.editor.files.js.changed = true;
                    S('.scripts-list > ul').html(list);
                    S('.editor .scripts-list .close-btn').on('click', S.editor.settings.scripts.remove);
                    S.popup.hide();
                });
            }
        },

        remove: function (e) {
            var target = S(S.target.findByClassName(e, 'close-btn'));
            var data = { file: target.attr('data-path'), path: S.editor.path };
            S.ajax.post('PageSettings/RemoveScriptFromPage', data, (list) => {
                //add script to page
                S.editor.files.js.changed = true;
                S('.scripts-list > ul').html(list);
                S('.editor .scripts-list .close-btn').on('click', S.editor.settings.scripts.remove);
            });
        }
    },

    security: {
        add: {
            show: function () {
                S.popup.show('Add Security Group to Page',
                    S('#template_security_add').html()
                );
                S('.popup form').on('submit', S.editor.settings.security.add.submit);

                //get list of available scripts
                S.ajax.post('PageSettings/GetAvailableSecurityGroups', {}, (list) => {
                    var html = [];
                    list = JSON.parse(list);
                    for (var x = 0; x < list.length; x++) {
                        html.push('<option value="' + list[x].id + '">' + list[x].name + '</option>');
                    }
                    S('#available_groups').html(html.join('\n'));

                });
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { groupId: S('#available_groups').val(), path: S.editor.path };
                S.ajax.post('PageSettings/AddSecurityGroup', data, (list) => {
                    //update security group list
                    S('.editor .security-list > ul').html(list);
                    S('.editor .security-list .close-btn').on('click', S.editor.settings.security.remove);
                    S.popup.hide();
                });
            }
        },

        remove: function (e) {
            var target = S(S.target.findByClassName(e, 'close-btn'));
            var data = { groupId: target.attr('data-id'), path: S.editor.path };
            S.ajax.post('PageSettings/RemoveSecurityGroup', data, (list) => {
                //update security group list
                S('.editor .security-list > ul').html(list);
                S('.editor .security-list .close-btn').on('click', S.editor.settings.security.remove);
            });
        }
    }
};