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
                $('.tab.page-settings').removeClass('hide');
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
                $('#page_header').on('change', S.editor.settings.partials.header.update);
                $('#page_footer').on('change', S.editor.settings.partials.footer.update);
                $('.settings-header-footer input[type="text"]').on('keyup', () => {
                    S.editor.settings.partials.changed = true;
                })

                //set up settings title
                S.editor.settings._loaded = true;
                S.editor.settings.clone = $('.page-settings .textarea-clone > div');
                var p = path.replace('content/', '');
                $('.page-name').attr('href', '/' + p).html(p);
                S.editor.resizeWindow();

                //set up events to detect changes
                var description = $('#page_description');
                $('#page_title_prefix, #page_title_suffix, #page_title').on('change, keyup', self.title.change);
                description.on('change, keyup, keydown', self.description.change);
                self.change(description, true);

                //set up button events
                $('.page-settings .title-prefix .icon a').on('click', S.editor.settings.title.prefix.show);
                $('.page-settings .title-suffix .icon a').on('click', S.editor.settings.title.suffix.show);
                $('.page-scripts .btn-add-script').on('click', S.editor.settings.scripts.add.show);
                $('.scripts-list .close-btn').on('click', S.editor.settings.scripts.remove);
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
                $('.item-save').removeClass('faded').removeAttr('disabled');
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
                $('.popup form').on('submit', S.editor.settings.title.prefix.submit);
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { title: $('#page_title_new_prefix').val(), prefix: true };
                S.ajax.post('PageSettings/CreatePageTitlePart', data,
                    function (d) {
                        var info = d.split('|');
                        $('#page_title_prefix').append('<option value="' + info[0] + '">' + info[1] + '</option>').val(info[0]);
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
                $('.popup form').on('submit', S.editor.settings.title.suffix.submit);
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble = true;
                var data = { title: $('#page_title_new_suffix').val(), prefix: false };
                S.ajax.post('PageSettings/CreatePageTitlePart', data,
                    function (d) {
                        var info = d.split('|');
                        $('#page_title_suffix').append('<option value="' + info[0] + '">' + info[1] + '</option>').val(info[0]);
                    }
                );
                S.popup.hide();
                return false;
            }
        },

        change: function () {
            var prefix = $('#page_title_prefix')[0].selectedOptions[0].text;
            var suffix = $('#page_title_suffix')[0].selectedOptions[0].text;
            if (prefix == '[None]') {
                prefix = '';
            } else {
                if (prefix[prefix.length - 1] != ' ') { prefix += ' '; }
            }
            if (suffix == '[None]') {
                suffix = '';
            } else {
                if (suffix[0] != ' ') { suffix = ' ' + suffix; }
            }
            window.document.title = prefix + $('#page_title').val() + suffix;
            $('.item-save').removeClass('faded').removeAttr('disabled');
            S.editor.settings.title.changed = true;
        }
    },

    description: {
        changed: false,
        change: function () {
            var description = $('#page_description');
            S.editor.settings.change(description, S.editor.settings.description.changed);
            S.editor.settings.description.changed = true;
            $('.item-save').removeClass('faded').removeAttr('disabled');
        }
    },

    partials: {
        changed: false,

        header: {
            update: function () {
                S.editor.settings.partials.changed = true;
                var template = S.editor.settings.field_template;
                var headers = S.editor.settings.headers;
                var header = headers[headers.map(a => a.file).indexOf($('#page_header').val())];
                html = [];
                for (field in header.fields) {
                    html.push(template
                        .replace('{{label}}', S.util.str.Capitalize(field.replace('-', ' ')))
                        .replace('{{name}}', field)
                        .replace('{{value}}', header.fields[field])
                    );
                }
                $('.header-fields .fields').html(html.join('\n'));
            }
        },

        footer: {
            update: function () {
                S.editor.settings.partials.changed = true;
                var template = S.editor.settings.field_template;
                var footers = S.editor.settings.footers;
                var footer = footers[footers.map(a => a.file).indexOf($('#page_footer').val())];
                html = [];
                for (field in footer.fields) {
                    html.push(template
                        .replace('{{label}}', S.util.str.Capitalize(field.replace('-', ' ')))
                        .replace('{{name}}', field)
                        .replace('{{value}}', footer.fields[field])
                    );
                }
                $('.footer-fields .fields').html(html.join('\n'));
            }
        }

    },

    scripts: {
        add: {
            show: function () {
                S.popup.show('Add Script to Page',
                    $('#template_scripts_add').html()
                );
                $('.popup form').on('submit', S.editor.settings.scripts.add.submit);

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
                    var script = document.createElement('script');
                    script.src = data.file;
                    document.body.appendChild(script);
                    S.editor.files.js.changed = true;
                    $('.scripts-list > ul').html(list);
                });
            }
        },

        remove: function (e) {
            var target = $(e.target);
            if (!target.hasClass('close-btn')) {
                target = target.parents('.close-btn').first();
            }
            var data = { file: target.attr('data-path'), path: S.editor.path };
            S.ajax.post('PageSettings/RemoveScriptFromPage', data, (list) => {
                //add script to page
                S.editor.files.js.changed = true;
                $('.scripts-list > ul').html(list);
            });
        }
    }
};