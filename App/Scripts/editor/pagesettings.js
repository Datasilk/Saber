S.editor.pagesettings = {
    _loaded: false,
    clone: null,
    headers: [],
    footers: [],

    load: function () {
        var self = S.editor.pagesettings;
        S.editor.tabs.create('Page Settings', 'page-settings-section', { showPageButtons: true, selected: true },
            () => { //onfocus
                S.editor.tabs.show('page-settings');
                S.editor.filebar.buttons.show('page-settings');
                let p = S.editor.path.replace('content/pages/', '');
                S.editor.filebar.update('Page Settings for <a href="/' + p + '">' + p + '</a>', 'icon-settings');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );

        var path = S.editor.path;
        S.ajax.post('PageSettings/Render', { path: path },
            function (d) {
                var data = JSON.parse(d);
                var json = JSON.parse(data.json);
                S.ajax.inject(data);

                //load settings header & footer fields
                S.editor.pagesettings.headers = json.headers;
                S.editor.pagesettings.footers = json.footers;

                //update settings header & footer events
                $('#page_header').on('input', S.editor.pagesettings.partials.header.update);
                $('#page_footer').on('input', S.editor.pagesettings.partials.footer.update);

                //set up settings title
                S.editor.pagesettings._loaded = true;
                S.editor.pagesettings.clone = $('.page-settings .textarea-clone > div');
                var p = path.replace('content/pages/', '');
                $('.editor .page-name').attr('href', '/' + p).html(p);
                S.editor.resize.window();

                //set up events to detect changes
                var description = $('#page_description');
                $('#page_title_prefix, #page_title_suffix, #page_title').on('input', self.title.change);
                description.on('input', self.description.change);
                self.change(description);

                //set up button events
                $('.page-settings .title-prefix .icon a').on('click', S.editor.pagesettings.title.prefix.show);
                $('.page-settings .title-suffix .icon a').on('click', S.editor.pagesettings.title.suffix.show);
                $('.page-styles .btn-add-style').on('click', S.editor.pagesettings.styles.add.show);
                $('.page-scripts .btn-add-script').on('click', S.editor.pagesettings.scripts.add.show);
                $('.page-security .btn-add-group').on('click', S.editor.pagesettings.security.add.show);
                $('.editor .security-list .close-btn').on('click', S.editor.pagesettings.security.remove);

                //execute init scripts
                S.editor.pagesettings.partials.header.update();
                S.editor.pagesettings.partials.footer.update();
                S.editor.pagesettings.styles.init();
                S.editor.pagesettings.scripts.init();
            }
        );
    },

    change: function (field, changed) {
        //update textarea height for given field
        if (S.editor.visible == true) {
            var clone = S.editor.pagesettings.clone;
            clone.html(field.val().replace(/\n/g, '<br/>') + '</br>');
            field.css({ height: clone.height() });
            if (changed == true) {
                S.editor.save.enable();
            }
        }
    },

    title: {
        _timer: null,
        changed: false,

        prefix: {
            show: function () {
                S.popup.show('New Page Title Prefix',
                    $('#template_pagetitle_newprefix').html()
                );
                $('.popup form').on('submit', S.editor.pagesettings.title.prefix.submit);
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { title: $('#page_title_new_prefix').val(), prefix: true };
                S.ajax.post('PageSettings/CreatePageTitlePart', data,
                    function (d) {
                        $('#page_title_prefix').append('<option value="' + d + '">' + d + '</option>').val(d);
                        S.editor.pagesettings.title.change();
                    }
                );
                S.popup.hide();
                return false;
            }
        },

        suffix: {
            show: function () {
                S.popup.show('New Page Title Suffix',
                    $('#template_pagetitle_newsuffix').html()
                );
                $('.popup form').on('submit', S.editor.pagesettings.title.suffix.submit);
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { title: $('#page_title_new_suffix').val(), prefix: false };
                S.ajax.post('PageSettings/CreatePageTitlePart', data,
                    function (d) {
                        $('#page_title_suffix').append('<option value="' + d + '">' + d + '</option>').val(d);
                        S.editor.pagesettings.title.change();
                    }
                );
                S.popup.hide();
                return false;
            }
        },

        change: function () {
            var prefix = $('#page_title_prefix').val();
            var suffix = $('#page_title_suffix').val();
            if (prefix != '' && prefix[prefix.length - 1] != ' ') { prefix += ' '; }
            if (suffix != '' && suffix[0] != ' ') { suffix = ' ' + suffix; }
            window.document.title = prefix + $('#page_title').val() + suffix;
            S.editor.save.enable();
            S.editor.pagesettings.title.changed = true;
        },

        save: function (callback) {
            var data = {
                path: S.editor.path,
                prefix: $('#page_title_prefix').val(),
                suffix: $('#page_title_suffix').val(),
                title: $('#page_title').val()
            };
            S.ajax.post('PageSettings/UpdatePageTitle', data, callback,
                function () { S.editor.error(); }
            );
            S.editor.pagesettings.title.changed = false;
        }
    },

    description: {
        changed: false,
        change: function () {
            S.editor.pagesettings.description.changed = true;
            S.editor.pagesettings.change($('#page_description'), true);
        },

        save: function (callback) {
            var data = {
                path: S.editor.path,
                description: $('#page_description').val()
            };
            S.ajax.post('PageSettings/UpdatePageDescription', data, callback,
                function () { S.editor.error(); }
            );
            S.editor.pagesettings.description.changed = false;
        }
    },

    partials: {
        changed: false,
        header: {
            update: function () {
                S.editor.pagesettings.partials.changed = true;
                S.editor.files.html.changed = true;
                var file = 'content/partials/' + $('#page_header').val();
                $('.edit-header-file a').attr('href', 'javascript:S.editor.explorer.open(\'' + file + '\')');
                $('.edit-header-content a').attr('href', 'javascript:S.editor.fields.load(\'' + file + '\')');
                S.editor.save.enable();
            }
        },

        footer: {
            update: function () {
                S.editor.pagesettings.partials.changed = true;
                S.editor.files.html.changed = true;
                var file = 'content/partials/' + $('#page_footer').val();
                $('.edit-footer-file a').attr('href', 'javascript:S.editor.explorer.open(\'' + file + '\')');
                $('.edit-footer-content a').attr('href', 'javascript:S.editor.fields.load(\'' + file + '\')');
                S.editor.save.enable();
            }
        },

        save: function (callback) {
            //get list of field values
            var data = {
                path: S.editor.path,
                header: $('#page_header').val(),
                footer: $('#page_footer').val()
            };
            S.ajax.post('PageSettings/UpdatePagePartials', data,
                function (d) {
                    //html resource has changed because header & footer partials have changed
                    S.editor.files.html.changed = true;
                    if (callback) { callback(); }
                },
                function () { S.editor.error(); }
            );
            S.editor.pagesettings.partials.changed = false;
        }

    },

    styles: {
        add: {
            show: function () {
                S.popup.show('Add Stylesheet to Page',
                    $('#template_styles_add').html()
                );
                $('.popup form').on('submit', S.editor.pagesettings.styles.add.submit);

                //get list of available scripts
                S.ajax.post('PageSettings/GetAvailableStylesheets', {}, (list) => {
                    var html = [];
                    list = JSON.parse(list);
                    for (var x = 0; x < list.length; x++) {
                        html.push('<option value="' + list[x] + '">' + list[x] + '</option>');
                    }
                    $('#available_styles').html(html.join('\n'));

                });
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { file: $('#available_styles').val(), path: S.editor.path };
                S.ajax.post('PageSettings/AddStylesheetToPage', data, (list) => {
                    //add stylesheet to page
                    S.util.css.load(data.file, 'css_' + data.file.replace(/\//g, '_').replace(/\./g, '_'), window.parent.document);
                    S.editor.files.less.changed = true;
                    $('.page-settings .styles-list > ul').html(list);
                    S.editor.pagesettings.styles.init();
                    S.popup.hide();
                });
            }
        },

        remove: function (e) {
            var target = $(S.target.findByClassName(e, 'close-btn'));
            var data = { file: target.attr('data-path'), path: S.editor.path };
            S.ajax.post('PageSettings/RemoveStylesheetFromPage', data, (list) => {
                //add script to page
                S.editor.files.less.changed = true;
                $('.page-settings .styles-list > ul').html(list);
                S.editor.pagesettings.styles.init();
            });
        },

        init: function () {
            $('.page-settings .styles-list .close-btn').on('click', S.editor.pagesettings.styles.remove);
            S.drag.sort.add('.page-settings .styles-list ul', '.page-settings .styles-list li', () => {
                //update website.config with new stylesheet sort order
                S.ajax.post('PageSettings/SortStylesheets', {
                    stylesheets: $('.page-settings .styles-list li div[data-path]').map((i, a) => $(a).attr('data-path')),
                    path: S.editor.path
                }, () => { },
                    (err) => {
                        S.editor.error('', 'error');
                    });
            });
        }
    },

    scripts: {
        add: {
            show: function () {
                S.popup.show('Add Script to Page',
                    $('#template_scripts_add').html()
                );
                $('.popup form').on('submit', S.editor.pagesettings.scripts.add.submit);

                //get list of available scripts
                S.ajax.post('PageSettings/GetAvailableScripts', {}, (list) => {
                    var html = [];
                    list = JSON.parse(list);
                    for (var x = 0; x < list.length; x++) {
                        html.push('<option value="' + list[x] + '">' + list[x] + '</option>');
                    }
                    $('#available_scripts').html(html.join('\n'));

                });
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { file: $('#available_scripts').val(), path: S.editor.path };
                S.ajax.post('PageSettings/AddScriptToPage', data, (list) => {
                    //add script to page
                    S.util.js.load(data.file, 'js_' + data.file.replace(/\//g, '_').replace(/\./g, '_'), null, null, window.parent.document);
                    S.editor.files.js.changed = true;
                    $('.page-settings .scripts-list > ul').html(list);
                    S.editor.pagesettings.scripts.init();
                    S.popup.hide();
                });
            }
        },

        remove: function (e) {
            var target = $(S.target.findByClassName(e, 'close-btn'));
            var data = { file: target.attr('data-path'), path: S.editor.path };
            S.ajax.post('PageSettings/RemoveScriptFromPage', data, (list) => {
                //add script to page
                S.editor.files.js.changed = true;
                $('.page-settings .scripts-list > ul').html(list);
                S.editor.pagesettings.scripts.init();
            });
        },

        init: function () {
            $('.page-settings .scripts-list .close-btn').on('click', S.editor.pagesettings.scripts.remove);
            S.drag.sort.add('.page-settings .scripts-list ul', '.page-settings .scripts-list li', () => {
                //update page json with new stylesheet sort order
                S.ajax.post('PageSettings/SortScripts', {
                    scripts: $('.page-settings .scripts-list li div[data-path]').map((i, a) => $(a).attr('data-path')),
                    path: S.editor.path
                }, () => { },
                    (err) => {
                        S.editor.error('', err.responseText);
                    });
            });
        }
    },

    security: {
        add: {
            show: function () {
                S.popup.show('Add Security Group to Page',
                    $('#template_security_add').html()
                );
                $('.popup form').on('submit', S.editor.pagesettings.security.add.submit);

                //get list of available scripts
                S.ajax.post('PageSettings/GetAvailableSecurityGroups', {}, (list) => {
                    var html = [];
                    list = JSON.parse(list);
                    for (var x = 0; x < list.length; x++) {
                        html.push('<option value="' + list[x].id + '">' + list[x].name + '</option>');
                    }
                    $('#available_groups').html(html.join('\n'));

                });
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { groupId: $('#available_groups').val(), path: S.editor.path };
                S.ajax.post('PageSettings/AddSecurityGroup', data, (list) => {
                    //update security group list
                    $('.editor .security-list > ul').html(list);
                    $('.editor .security-list .close-btn').on('click', S.editor.pagesettings.security.remove);
                    S.popup.hide();
                });
            }
        },

        remove: function (e) {
            var target = $(S.target.findByClassName(e, 'close-btn'));
            var data = { groupId: target.attr('data-id'), path: S.editor.path };
            S.ajax.post('PageSettings/RemoveSecurityGroup', data, (list) => {
                //update security group list
                $('.editor .security-list > ul').html(list);
                $('.editor .security-list .close-btn').on('click', S.editor.pagesettings.security.remove);
            });
        }
    }
};