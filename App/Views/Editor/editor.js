S.editor = {
    type: 0, //0 = Monaco, 1 = Ace
    instance: null,
    sessions: {},
    viewstates: {},
    selected: '',
    path: '',
    theme: 'dark',
    sect: $('.sections'),
    div: $('.code-editor'),
    divFields: $('.content-fields'),
    divBrowser: $('.file-browser'),
    initialized: false,
    savedTabs:[],
    Rhino: null,
    visible:false,

    init: function () {
        this.initialized = true;
        this.visible = true;

        //generate path
        var path = window.location.pathname.toLowerCase();
        if (path == '/') { path = '/home'; }
        this.path = 'content' + path;
        var paths = this.path.split('/');
        var file = paths[paths.length - 1];
        var dir = paths.join('/').replace(file, '');
        var fileparts = paths[paths.length - 1].split('.', 2);
        $('.page-name').attr('href', path).html(path);

        //initialize code editor
        var editor = null;
        switch (this.type) {
            case 0: //monaco
                require.config({ paths: { 'vs': '/js/utility/monaco/min/vs' } });
                require(['vs/editor/editor.main'], function () {
                    editor = monaco.editor.create(document.getElementById('editor'), {
                        value: '',
                        theme: "vs" + (this.theme != '' ? '-' + S.editor.theme : ''),
                        automaticLayout: true,
                        colorDecorators: true,
                        dragAndDrop: false,
                        folding: true,
                        formatOnPaste: true,
                        glyphMargin: false,
                        mouseWheelZoom: true,
                        parameterHints: true,
                        showFoldingControls: 'always'
                    });
                    editor.onKeyUp((e) => { S.editor.keyUp(e); });
                    editor.onMouseUp((e) => { S.editor.codebar.update(); });
                    S.editor.instance = editor;
                });
                break;

            case 1: //ace
                editor = ace.edit("editor");
                editor.setTheme("ace/theme/xcode");
                editor.setOptions({
                    //enableEmmet: true
                });
                S.editor.EditSession = require("ace/edit_session").EditSession;

                //add editor key bindings
                editor.commands.addCommand({
                    name: "showKeyboardShortcuts",
                    bindKey: { win: "Ctrl-h", mac: "Command-h" },
                    exec: function (editor) {
                        ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                            module.init(editor);
                            S.editor.instance.showKeyboardShortcuts()
                        })
                    }
                });
                this.instance = editor;
                break;
        }

        //resize code editor
        this.resize();

        //add button events
        $('.item-browse').on('click', S.editor.explorer.show);
        $('.tab-drop-menu').on('click', S.editor.dropmenu.show);
        $('.bg-overlay').on('click', S.editor.dropmenu.hide);
        $('.editor-drop-menu .item-save').on('click', S.editor.save);
        $('.editor-drop-menu .item-save-as').on('click', S.editor.saveAs);
        $('.editor-drop-menu .item-content-fields').on('click', function () { S.editor.filebar.fields.show(true); });
        $('.editor-drop-menu .item-page-settings').on('click', S.editor.filebar.settings.show);
        $('.editor-drop-menu .item-app-settings').on('click', S.editor.appsettings.show);
        $('.editor-drop-menu .item-new-file').on('click', S.editor.file.create.show);
        $('.editor-drop-menu .item-new-folder').on('click', S.editor.folder.create.show);
        $('.editor-drop-menu .item-new-window').on('click', S.editor.newWindow);
        $('.tab-content-fields').on('click', S.editor.filebar.fields.show);
        $('.tab-file-code').on('click', S.editor.filebar.code.show);
        $('.tab-page-settings').on('click', S.editor.filebar.settings.show);
        $('.tab-page-resources').on('click', S.editor.filebar.resources.show);
        $('.tab-preview').on('click', S.editor.filebar.preview.show);
        $('.edit-bar').on('mousedown', function (e) {
            if (e.target != $('.edit-bar')[0]) { return; }
            if (S.editor.Rhino) {
                S.editor.Rhino.drag();
            }
        });

        //add window resize event
        $(window).on('resize', S.editor.resizeWindow);

        //register hotkeys
        $(window).on('keydown', S.editor.hotkey.pressed);

        //register explorer routes
        S.editor.explorer.routes = [
            {}
        ];

        //finally, load content resources that belong to the page
        var tabs = [dir + fileparts[0] + '.html', dir + fileparts[0] + '.less', dir + fileparts[0] + '.js'];
        if (this.savedTabs.length > 0) {
            tabs = tabs.concat(this.savedTabs);
        }
        //get saved tabs from server
        S.ajax.post('Files/GetOpenedTabs', {},
            function (d) {
                tabs = tabs.concat(JSON.parse(d));
                openTabs();
            },
            function (err) {
                openTabs();
            }
        );

        function openTabs() {
            S.editor.explorer.openResources(tabs,
                function () {
                    setTimeout(function () {
                        S.editor.codebar.status('Ready');
                        S.editor.codebar.update();
                    }, 500);
                }
            );
        }

        

        //initialize JavaScript binding into Rhinoceros (if available)
        if (typeof CefSharp != 'undefined') {
            (async function () {
                await CefSharp.BindObjectAsync("Rhino", "bound");
                S.editor.Rhino = Rhino;

                //change color scheme of Rhino window
                S.editor.filebar.preview.hide();
            })();
        }
    },

    resize: function () {
        var editor = S.editor.instance;
        var newHeight = 0;
        switch (S.editor.type) {
            case 0: //monaco

                break;
            case 1: //ace
                newHeight = editor.getSession().getScreenLength() * editor.renderer.lineHeight + editor.renderer.scrollBar.getWidth();
                break;
        }
        
        if (newHeight < 20) { newHeight = 20; }
        newHeight += 30;
        $('#editor').css({ minHeight: newHeight.toString() + "px" });
        $('#editor-section').css({ minHeight: newHeight.toString() + "px" });


        //resize code editor
        switch (S.editor.type) {
            case 1: //ace
                S.editor.instance.resize();
                break;
        }
        S.editor.resizeWindow();
    },

    resizeWindow: function () {
        var win = S.window.pos();
        var sect = S.editor.sect;
        var div = S.editor.div;
        var browser = S.editor.divBrowser;
        var fields = S.editor.divFields;
        var pos = sect.offset();
        var pos2 = browser.offset();
        div.css({ height: '' });
        if (pos.top == 0) {
            pos = fields.offset();
        }
        $('.editor > div > .sections > .tab:nth-child(n+2)').css({ height: win.h - pos.top });
        $('.file-browser').css({ height: win.h - pos2.top });
    },

    dropmenu: {
        show: function () {
            $('.editor-drop-menu, .bg-overlay').removeClass('hide');
            $(document.body).on('click', S.editor.dropmenu.hide);
        },

        hide: function (e) {
            var hide = false;
            if (e) {
                if ($(e.target).parents('.editor-drop-menu > div').length == 0) {
                    hide = true;
                }
            } else { hide = true; }
            if (hide == true) {
                $('.editor-drop-menu, .bg-overlay').addClass('hide');
                $(document.body).off('click', S.editor.dropmenu.hide);
            }
        }
    },

    newWindow: function () {
        S.editor.dropmenu.hide();
        if (S.editor.Rhino) {
            S.editor.Rhino.newwindow();
        } else {
            var id = S.editor.fileId();
            window.open(window.location.href, 'Editor_' + id, 'width=1800,height=900,left=50,top=50,toolbar=No,location=No,scrollbars=auto,status=No,resizable=yes,fullscreen=No');
        }
    },

    keyUp: function (e) {
        var specialkeys = [16, 17, 18, 20, 27, 33, 34, 35, 36, 37, 38, 39, 40, 45, 91, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 123, 144];
        var specialCodes = ['Escape'];
        if (specialkeys.indexOf(e.keyCode) < 0 && e.ctrlKey == false && e.altKey == false && specialCodes.indexOf(e.code)) {
            //content only changes if special keys are not pressed
            S.editor.changed();
        }
    },
    
    changed: function (checkall) {
        var self = S.editor;
        var id = self.fileId(self.selected);
        var tab = $('.tab-' + id);
        if (tab.length == 1) {
            if (tab.attr('data-edited') != "true" || checkall == true) {
                //enable save menu
                $('.item-save').removeClass('faded').removeAttr('disabled');

                if (tab.attr('data-edited') != "true") {
                    //update tab with *
                    tab.attr('data-edited', "true");
                    var col = tab.find('.col');
                    col.html(col.html() + ' *');
                }
            }
        }
        S.editor.codebar.update();
        self.resize();
    },

    isChanged: function (path) {
        var self = S.editor;
        var id = self.fileId(path);
        var tab = $('.tab-' + id);
        if (tab.length == 1) {
            if (tab.attr('data-edited') == "true") {
                return true;
            }
        }
        return false;
    },

    unChanged: function (path) {
        var self = S.editor;
        var id = self.fileId(path);
        var tab = $('.tab-' + id);
        if (tab.length == 1) {
            if (tab.attr('data-edited') == "true") {
                tab.removeAttr('data-edited');
                $('.item-save').addClass('faded').attr('disabled', 'disabled');
                var col = tab.find('.col');
                col.html(col.html().replace(' *', ''));
            }
        }
    },

    error: function (elem, msg) {
        $('.editor > div > .messages').append('<div class="message error"><span>' +
            (msg || S.message.error.generic) + "</span></div>");
    },

    message: function (msg, type) {
        $('.editor > div > .messages').append('<div class="message' (type != null ? " " + type : '') + '"><span>' +
            msg + "</span></div>");
    },

    save: function (path, content) {
        if (path == null || typeof path != 'string') {
            switch (S.editor.type) {
                case 0: case 1: //monaco & ace (apparently share the same code)
                    S.editor.save(S.editor.selected, S.editor.instance.getValue());
                    return;
            }
        }
        var self = S.editor;
        var id = self.fileId(path);
        var ext = S.editor.fileExt(path);
        var tab = $('.tab-' + id);
        self.dropmenu.hide();
        if (tab.length > 0) {
            if (tab.hasClass('selected')) {
                //check if we should save something besides source code

                if ($('.tab-content-fields').hasClass('selected')) {
                    //save content fields values ///////////////////////////////////////////////////////////////////////////////
                    var fields = {};
                    var texts = $('.content-fields form').find('textarea, input, select');
                    texts.each(function (txt) {
                        var t = $(txt);
                        fields[txt.id.replace('field_', '')] = t.val();
                    });

                    S.ajax.post('ContentFields/Save', { path: path, fields: fields, language: $('#lang').val() },
                        function (d) {
                            if (d == 'success') {
                                S.editor.fields.changed = false;
                                //html resource has changed because content fields have changed
                                S.editor.files.html.changed = true;
                                S.message.show('.content-fields .message', 'confirm', 'Content fields were saved.', false, 4000, true);
                            } else { S.editor.error(); }
                        },
                        function () {
                            S.editor.error();
                        }
                    );
                    return;

                } else if ($('.tab-page-settings').hasClass('selected')) {
                    //save page settings ///////////////////////////////////////////////////////////////////////////////
                    var settings = S.editor.settings;

                    //save title
                    if (settings.title.changed == true) {
                        var data = {
                            path: S.editor.path,
                            prefixId: $('#page_title_prefix').val(),
                            suffixId: $('#page_title_suffix').val(),
                            title: $('#page_title').val()
                        };
                        S.ajax.post('PageSettings/UpdatePageTitle', data,
                            function (d) {
                                //show message to user
                                showmsg();
                            },
                            function () { S.editor.error(); }
                        );
                    }
                    

                    //save description
                    if (settings.description.changed == true) {
                        var data = {
                            path: S.editor.path,
                            description: $('#page_description').val()
                        };
                        S.ajax.post('PageSettings/UpdatePageDescription', data,
                            function (d) {
                                //show message to user
                                showmsg();
                            },
                            function () { S.editor.error(); }
                        );
                    }

                    //save header & footer with fields
                    if (settings.partials.changed == true) {
                        //get list of field values
                        var header_fields = {};
                        var footer_fields = {};
                        var elems = $('.header-fields .fields input');
                        elems.each(a => {
                            header_fields[a.name] = $(a).val();
                        });
                        elems = $('.footer-fields .fields input');
                        elems.each(a => {
                            footer_fields[a.name] = $(a).val();
                        });
                        var data = {
                            path: S.editor.path,
                            header: { file: $('#page_header').val(), fields: header_fields },
                            footer: { file: $('#page_footer').val(), fields: footer_fields }
                        };
                        S.ajax.post('PageSettings/UpdatePagePartials', data,
                            function (d) {
                                //show message to user
                                showmsg();
                                //html resource has changed because header & footer partials have changed
                                S.editor.files.html.changed = true;
                            },
                            function () { S.editor.error(); }
                        );
                    }

                    function showmsg() {
                        S.message.show('.page-settings .message', 'confirm', 'Page settings have been updated successfully');
                    }

                    return;
                }
            } 
        }

        //last resort, save source code to file ////////////////////////////////////////////////////////////
        if ($('.editor-drop-menu .item-save.faded').length == 1) { return; }

        //show loading progress animation
        tab.find('.tab-title').prepend(S.loader());
        S.ajax.post('Files/SaveFile', { path: path, content: content },
            function (d) {
                //check whether or not file was a required page resource for this page
                if (S.editor.isResource(path) || S.editor.isResource(path, 'partial')) {
                    S.editor.files[ext].changed = true;
                    if (ext == 'html') {
                        //also mark content as changed so content fields can reload
                        S.editor.files.content.changed = true;
                    }
                } else if(S.editor.isResource(path, 'website.css')) {
                    S.editor.files.website.css.changed = true;
                } else if (S.editor.isResource(path, 'website.js')) {
                    S.editor.files.website.js.changed = true;
                    S.editor.files.js.changed = true;
                }
                tab.find('.loader').remove();
                self.unChanged(path);
                S.editor.explorer.open(path);
            },
            function () {
                S.editor.error();
            }
        );
    },

    saveAs: function () {
        S.editor.dropmenu.hide();
        var oldpath = S.editor.selected;
        var path = prompt("Save As...", oldpath);
        if (path != '' && path != null) {
            var value = '';
            switch (S.editor.type) {
                case 0: case 1: //monaco & ace (apparently shares the same code)
                    value = S.editor.instance.getValue();
                    break;
            }
            S.editor.save(path, value);
        }
    },

    file: {
        create: {
            show: function () {
                S.editor.dropmenu.hide();
                S.popup.show('New File', 
                    $('#template_newfile').html()
                        .replace('##folder-path##', S.editor.explorer.path)
                );
                //set up button events within popup
                $('.popup form').on('submit', S.editor.file.create.submit)
            },

            submit: function (e) {
                e.preventDefault();
                e.cancelBubble;
                var data = {
                    path: $('#newfilepath').val(),
                    filename: $('#newfilename').val().replace(/\s/g, '')
                };
                if (data.path == 'root') {
                    S.message.show('.popup .message', 'error', 'You cannot create files in the root folder');
                    return false;
                }
                S.ajax.post('Files/NewFile', data,
                    function(d) {
                        //reload file browser
                        if (data.path == S.editor.explorer.path) {
                            S.editor.explorer.dir(S.editor.explorer.path);
                        }
                        S.popup.hide();
                    },
                    function(d) {
                        S.message.show('.popup .message', 'error', d.response);
                    }
                );
                return false;
            }
        }
    },

    folder: {
        create: {
            show: function () {
                S.editor.dropmenu.hide();
                S.popup.show('New Folder',
                    $('#template_newfolder').html()
                        .replace('##folder-path##', S.editor.explorer.path)
                );
                //set up button events within popup
                $('.popup form').on('submit', S.editor.folder.create.submit)
            },

            submit: function(e) {
                e.preventDefault();
                e.cancelBubble;
                var data = {
                    path: $('#newfolderpath').val().replace(/\s/g,''),
                    folder: $('#newfolder').val()
                };
                if (data.path == 'root') {
                    S.message.show('.popup .message', 'error', 'You cannot create folders within the root folder');
                    return false;
                }
                S.ajax.post('Files/NewFolder', data,
                    function(d) {
                        //reload file browser
                        if (data.path == S.editor.explorer.path) {
                            S.editor.explorer.dir(S.editor.explorer.path);
                        }
                        S.popup.hide();
                    },
                    function(d) {
                        S.message.show('.popup .message', 'error', d.response);
                    }
                );
                return false;
            }
        }
    },

    fileId: function (path) {
        if (path == null) { path = 'content' + window.location.pathname.toLowerCase(); }
        return path.replace(/\//g, '_').replace(/\./g, '_');
    },

    fileExt: function (path) {
        var paths = path.split('/');
        var file = paths[paths.length - 1];
        var dir = paths.join('/').replace(file, '');
        var fileparts = paths[paths.length - 1].split('.', 2);
        if (fileparts.length > 1) { return fileparts[fileparts.length - 1]; }
        return '';
    },

    decodeHtml(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    },

    sessions: {
        add: function (id, mode, code, select) {
            var editor = S.editor.instance;
            switch (S.editor.type) {
                case 0: //monaco ////////////////////////////////////////////////////
                    require(['vs/editor/editor.main'], function () {
                        var session = monaco.editor.createModel(code, mode);
                        S.editor.sessions[id] = session;
                        S.editor.viewstates[id] = null;
                        if (S.editor.selected != '') {
                            S.editor.sessions.saveViewState(S.editor.fileId(S.editor.selected));
                        }
                        if (select !== false) {
                            S.editor.instance.saveViewState();
                            S.editor.instance.setModel(session);
                            S.editor.filebar.code.show();
                            S.editor.codebar.update();
                        }
                    });
                    break;
                case 1: //ace ///////////////////////////////////////////////////////
                    var session = new S.editor.EditSession(code);
                    session.setMode("ace/mode/" + mode);
                    session.on('change', S.editor.changed);
                    S.editor.sessions[id] = session;
                    
                    if (select !== false) {
                        S.editor.filebar.code.show();
                        editor.setSession(session);
                        editor.clearSelection();
                        S.editor.codebar.update();
                        S.editor.resize();
                        setTimeout(function () {
                            S.editor.resize();
                        }, 200);
                        editor.focus();
                    }
                    break;
            }

        },

        remove: function (id) {
            if(S.editor.sessions[id] != null){
                S.editor.sessions[id].dispose();
                delete S.editor.sessions[id];
            }
        },

        saveViewState(id) {
            //save previous viewstate
            switch (S.editor.type) {
                case 0: //monaco
                    S.editor.viewstates[id] = S.editor.instance.saveViewState();
                    break;
            }
        },

        restoreViewState(id) {
            if (S.editor.viewstates[id]) {
                switch (S.editor.type) {
                    case 0: //monaco
                        S.editor.instance.restoreViewState(S.editor.viewstates[id]);
                        break;
                }
            }
        }
    },

    isResource: function (path, type) {
        var paths = path.toLowerCase().split('/');
        var file = paths[paths.length - 1];
        var dir = paths.join('/').replace(file, '');
        var fileparts = paths[paths.length - 1].split('.', 2);
        var relpath = dir + fileparts[0];
        if (typeof type == 'undefined') {
            return relpath == S.editor.path;
        } else if (type == 'website.css') {
            switch (dir + fileparts.join('.')) {
                case 'content/partials/header.less':
                case 'content/partials/footer.less':
                case 'root/css/website.less':
                    return true;
            }
        } else if (type == 'website.js') {
            switch (dir + fileparts.join('.')) {
                case 'root/scripts/website.js':
                    return true;
            }
        } else if (type == 'partial') {
            switch (dir + fileparts.join('.')) {
                case 'content/partials/header.html':
                case 'content/partials/footer.html':
                    return true;
            }
        }
    },

    files: {
        //required page files, track whether or not they've 
        //been updated on the server via the editor
        html: { changed: false },
        less: { changed: false },
        js: { changed: false },
        website: {
            css: { changed: false },
            js: { changed: false }
        },
        content: {changed: false}
    },

    tabs: {
        create: function (title, path, options, onfocus, onblur, onsave) {
            //build options
            var opts = {
                selected: options ? (options.selected != null ? options.selected : true) : true,
                canSave: options ? (options.canSave != null ? options.canSave : false) : false,
                changed: options ? (options.changed != null ? options.changed : false) : false,
                isPageResource: options ? (options.isPageResource !== null ? options.isPageResource : false) : false,
            };
            var id = path.replace('/', '_');
            var route = {};

            //deselect other tabs
            if (opts.selected == true) {
                $('.edit-tabs ul.row li, .edit-tabs ul.row > li > div').removeClass('selected');
            }
            var elem = $('.edit-tabs ul.row .tab-' + id);
            if (elem.length == 0) {
                //load new tab
                var temp = $('#template_tab').html().trim();
                $('.edit-tabs ul.row').append(temp
                    .replace(/\#\#id\#\#/g, id)
                    .replace('##path##', path)
                    .replace('##title##', title)
                    .replace(/\#\#selected\#\#/g, opts.selected == true ? 'selected' : '')
                    .replace('##resource-icon##', 'hide')
                );
                var elems = $('.edit-tabs ul.row').children();
                elem = $(elems[elems.length - 1]);

                //close button
                $('.tab-' + id + ' .btn-close').on('click', function (e) {
                    S.editor.tabs.close(id, path);
                    e.preventDefault();
                    e.cancelBubble = true;
                });

                route = {
                    id: id,
                    path: path,
                    elem: elem,
                    options: opts,
                    onfocus: () => {
                        if (opts.isPageResource == true) {
                            $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').show();
                        } else {
                            $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').hide();
                        }
                        elem.addClass('selected');
                        if (typeof onfocus == 'function') { onfocus(); }
                    },
                    onblur: onblur,
                    onsave: onsave
                };
                S.editor.explorer.routes.push(route);
                if (opts.selected == true) {
                    route.onfocus();
                }
            } else {
                route = S.editor.explorer.routes.filter(a => a.path == path)[0];
                route.onfocus();
            }
            S.editor.selected = path;
        },
        close: function (id, path) {
            var tab = $('.tab-' + id);
            var sibling = tab.prev().find('.row.hover');
            tab.remove();
            S.editor.sessions.remove(id);

            //check to see if selected tab is being removed
            if (S.editor.selected == path && sibling.length == 1) {
                sibling[0].click();
            }

            //update user session
            S.ajax.post('Files/Close', { path: path });
        }
    },
    
    explorer: {
        path: 'root',
        routes:[],
        queue: [],

        show: function () {
            S.editor.dropmenu.hide();
            if (!$('.editor .file-browser').hasClass('hide')) { S.editor.explorer.hide(); return;}
            if ($('.file-browser ul.menu').children().length == 0) {
                S.editor.explorer.dir('root');
            }
            $('.editor .file-browser').removeClass('hide');
            $('.editor').addClass('show-browser');
            S.editor.resizeWindow();
        },

        hide: function () {
            $('.editor .file-browser').addClass('hide');
            $('.editor').removeClass('show-browser');
        },

        dir: function (path) {
            S.ajax.post('Files/Dir', { path: path },
                function(d) {
                    S.editor.explorer.path = path;
                    $('.file-browser ul.menu').html(d);
                    var url = path;
                    if (path.indexOf('root') == 0) {
                        url = url.replace('root', '');
                    }
                    url += '/';
                    $('.browser-path').html(url);
                    if (path.indexOf('wwwroot') == 0) {
                        //load resources section
                        S.editor.filebar.resources.show(true);

                        //hide all filebar icons except resources icon
                        $('ul.file-tabs > li:not(.tab-page-resources)').hide();

                        //change filebar path
                        $('#filepath').val(path);
                        $('.file-bar .file-icon use').attr('xlink:href', '#icon-folder');

                        //deselect file tab
                        $('.edit-bar ul.row .selected').removeClass('selected');
                        S.editor.selected = '';

                        S.editor.resources.load(path);
                    }
                },
                function () {
                    S.editor.error();
                }
            );
        },

        openResources: function (files, callback) {
            //opens a group of resources (html, less, js) from a specified path
            files.forEach((f) => {
                S.editor.explorer.queue.push(f);
            });
            S.editor.explorer.runQueue(true, callback);
        },

        runQueue: function (first, callback) {
            //opens next resource in the queue
            var self = S.editor.explorer;
            var queue = self.queue;
            if (queue.length > 0) {
                var path = queue[0].toString();
                queue.splice(0, 1);
                self.open(path, null, first === true, function () { self.runQueue(false, callback); });
            } else {
                if (typeof (callback) == 'function') {
                    callback();
                }
            }
        },

        open: function (path, code, isready, callback) {
            //opens a resource that exists on the server
            var paths = path.split('/');
            var id = S.editor.fileId(path);
            var prevId = '';
            if (S.editor.selected != '') {
                prevId = S.editor.fileId(S.editor.selected);
            }

            if (isready !== false) {
                //update selected session
                S.editor.selected = path;
                //deselect tabs
                $('.edit-tabs ul.row li, .edit-tabs ul.row > li > div').removeClass('selected');
                //disable save menu
                $('.item-save').addClass('faded').attr('disabled', 'disabled');
            }

            //check for existing tab
            var tab = $('.edit-tabs ul.row .tab-' + id);

            //find route that matches path (if route exists)
            var route = S.editor.explorer.routes.filter(a => a.path == paths[0]);
            if (route.length > 0) {
                route = route[0];
                var routes = S.editor.explorer.routes.filter(a => a.path != paths[0]);
                routes.forEach(a => {
                    if (typeof a.onblur == 'function') { a.onblur();}
                });
                tab.addClass('selected').find('.row.hover').addClass('selected');
                $('.editor > div > .sections > div').addClass('hide');
                route.onfocus();
                return;
            }

            //get file info
            var file = paths[paths.length - 1];
            var fileparts = paths[paths.length - 1].split('.', 2);
            var isPageResource = S.editor.isResource(path);

            if (tab.length == 0) {
                //tab doesn't exist yet
                var temp = $('#template_tab').html().trim();
                var title = file;
                if (fileparts[0].length > 18) { title = '...' + fileparts[0].substr(fileparts[0].length - 15) + '.' + fileparts[1];}
                $('.edit-tabs ul.row').append(temp
                    .replace(/\#\#id\#\#/g, id)
                    .replace('##path##', path)
                    .replace('##title##', title)
                    .replace(/\#\#selected\#\#/g, isready !== false ? 'selected' : '' )
                    .replace('##tab-type##', isPageResource ? 'page-level' : '')
                    .replace('##resource-icon##', isPageResource ? '' : 'hide')
                );
                //add button events for tab
                if (!isPageResource) {
                    $('.tab-' + id + ' .btn-close').on('click', function (e) {
                        S.editor.tabs.close(path);
                        e.preventDefault();
                        e.cancelBubble = true;
                    });
                } else {
                    //hide close button, tab is special
                    $('.tab-' + id + ' .btn-close').hide();
                }
                
            } else {
                //tab exists
                tab.addClass('selected').find('.row.hover').addClass('selected');
            }
            
            if (isready !== false) {
                $('.tab-content-fields, .tab-page-settings, .tab-page-resources, .tab-preview').hide();
                if (isPageResource) {
                    //show file bar icons for page html resource
                    $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').show();
                }
            }
            

            //check for existing source code
            var nocode = (code == null || typeof code == 'undefined');
            var session = S.editor.sessions[id];
            var editor = S.editor.instance;
            var paths = path.split('/');
            var ext = paths[paths.length - 1].split('.')[1];
            var mode = 'html';
            switch (ext) {
                case 'css': case 'less': mode = ext; break;
                case 'js': mode = 'javascript'; break;
            }

            //change file path
            var cleanPath = path;
            if (path.indexOf('content/partials/') == 0) {
            } else if (path.indexOf('content/') == 0) {
                cleanPath = path.replace('content/', 'content/pages/');
            }
            if (path.indexOf('root/') == 0) { cleanPath = path.replace('root/', ''); }

            if (isready !== false) {
                //set file bar path text & icon
                $('#filepath').val(cleanPath);
                $('.file-bar .file-icon use')[0].setAttribute('xlink:href', '#icon-file-' + ext);
            }

            if (session == null && nocode == true) {
                //load new session from ajax POST, loading code from server
                S.ajax.post("Files/Open", { path: path, pageResource: isPageResource === true },
                    function (d) {
                        S.editor.sessions.add(id, mode, S.editor.decodeHtml(d), isready !== false);
                        if (typeof callback == 'function') { callback();}
                    },
                    function () {
                        S.editor.error();
                    }
                );
            } else if (nocode == false) {
                //load new session from provided code argument
                S.editor.sessions.add(id, mode, S.editor.decodeHtml(code), isready !== false);
                if (typeof callback == 'function') { callback(); }
                
            } else {
                //load existing session
                S.editor.filebar.code.show();
                switch (S.editor.type) {
                    case 0: //monaco
                        //save viewstate for currently viewed session
                        S.editor.sessions.saveViewState(prevId);

                        //load selected session
                        editor.setModel(session);

                        //restore viewstate for selected session
                        S.editor.sessions.restoreViewState(id);
                        editor.focus();
                        break;
                    case 1: //ace
                        editor.setSession(session);
                        editor.focus();
                        break;
                }
                if (S.editor.isChanged(path) == true) {
                    S.editor.changed(true);
                }
                S.editor.codebar.update();
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, 200);
                if (typeof callback == 'function') { callback(); }
            }
        }
    },

    filebar: {
        fields: {
            show: function (loadtab) {
                S.editor.dropmenu.hide();
                S.editor.tabs.create("Content Fields", "content-fields-section", { isPageResource:true },
                    () => { //onfocus
                        $('.tab.content-fields').removeClass('hide');
                    },
                    () => { //onblur

                    },
                    () => { //onsave

                    }
                );


                //show content fields section & hide other sections
                $('.editor .sections > .tab:not(.file-browser)').addClass('hide');
                $('.editor .sections > .content-fields').removeClass('hide');
                $('ul.file-tabs > li').removeClass('selected');
                $('ul.file-tabs > li.tab-content-fields').addClass('selected');

                var lang = 'en';

                if ($('#lang').children().length == 0) {
                    //load list of languages
                    S.ajax.post('Languages/Get', {},
                        function (d) {
                            var html = '';
                            var langs = d.split('|');
                            for (x = 0; x < langs.length; x++) {
                                var lang = langs[x].split(',');
                                html += '<option value="' + lang[0] + '">' + lang[1] + '</option>';
                            }
                            $('#lang').html(html);
                        },
                        function () {
                            S.editor.error();
                        }
                    );
                } else {
                    lang = $('#lang').val();
                }

                //disable save menu
                $('.item-save').addClass('faded').attr('disabled', 'disabled');
                $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

                //load content for page
                if (S.editor.fields.selected != S.editor.selected || S.editor.files.content.changed == true) {
                    S.editor.files.content.changed = false;
                    S.editor.fields.changed = false;
                    $('.content-fields form').html('');
                    S.ajax.post('ContentFields/Render', { path: S.editor.path, language:lang },
                        function (d) {
                            S.editor.fields.selected = S.editor.selected;
                            $('.content-fields form').html(d);

                            //set up events for fields
                            $('.content-fields form textarea').on('keyup, keydown', S.editor.fields.change).each(
                                function (field) {
                                    S.editor.fields.change({ target: field });
                                }
                            );
                        },
                        function () { S.editor.error(); }
                    );
                } else {
                    if (S.editor.fields.changed == true) {
                        //enable save menu since file was previously changed
                        $('.item-save').removeClass('faded').removeAttr('disabled');
                    }
                }
            }
        },

        code: {
            show: function () {
                $('.editor .sections > .tab:not(.file-browser)').addClass('hide');
                $('.editor .sections > .code-editor').removeClass('hide');
                $('ul.file-tabs > li').removeClass('selected');
                $('ul.file-tabs > li.tab-file-code').addClass('selected');
                if (S.editor.isChanged(S.editor.selected)) { S.editor.changed(); }
                $('.item-save-as').removeClass('faded').removeAttr('disabled');
                setTimeout(function () { S.editor.resize(); }, 10);
            }
        },

        settings: {
            show: function () {
                S.editor.tabs.create("Page Settings", "page-settings-section", { isPageResource: true },
                    () => { //onfocus
                        $('.tab.page-settings').removeClass('hide');
                    },
                    () => { //onblur

                    },
                    () => { //onsave

                    }
                );
                S.editor.dropmenu.hide();
                $('.editor .sections > .tab:not(.file-browser)').addClass('hide');
                $('.editor .sections > .page-settings').removeClass('hide');
                $('ul.file-tabs > li').removeClass('selected');
                $('ul.file-tabs > li.tab-page-settings').addClass('selected');

                //disable save menu
                $('.item-save').addClass('faded').attr('disabled', 'disabled');
                $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

                S.editor.settings.load();
            }
        },

        resources: {
            show: function (noload) {
                if (S.editor.selected == '') { return;}
                S.editor.dropmenu.hide();
                $('.editor .sections > .tab:not(.file-browser)').addClass('hide');
                $('.editor .sections > .page-resources').removeClass('hide');
                $('ul.file-tabs > li').removeClass('selected');
                $('ul.file-tabs > li.tab-page-resources').addClass('selected');

                //disable save menu
                $('.item-save').addClass('faded').attr('disabled', 'disabled');
                $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

                if (noload === true) { return;}
                S.editor.resources.load(S.editor.path);
            }
        },

        preview: {
            toggle: function () {
                var self = S.editor.filebar.preview;
                if ($('.editor-preview').hasClass('hide')) {
                    self.show();
                } else {
                    self.hide();
                }
            },
            show: function () {
                var tagcss = $('#page_css');
                var tagjs = $('#page_js');
                var css = '/' + S.editor.path.replace('content/', 'content/pages/') + '.css';
                var src = '/' + S.editor.path.replace('content/', 'content/pages/') + '.js';
                var rnd = Math.floor(Math.random() * 9999);

                //first, reload CSS
                if (S.editor.files.less.changed == true) {
                    S.editor.files.less.changed = false;
                    tagcss.remove();
                    $('head').append(
                        '<link rel="stylesheet" type="text/css" id="page_css" href="' + css + '?r=' + rnd + '"></link>'
                    );
                }

                if (S.editor.files.website.css.changed == true) {
                    //reload website.css
                    S.editor.files.website.css.changed = false;
                    $(website_css).attr('href', '/css/website.css?r=' + rnd);
                }

                //next, reload rendered HTML
                if (S.editor.files.html.changed == true || S.editor.files.content.changed == true) {
                    S.editor.files.html.changed = false;
                    S.ajax.post('Page/Render', { path: S.editor.path + '.html', language: window.language },
                        function (d) {
                            $('.editor-preview').html(d);
                            changeJs(true);
                        }
                    );
                } else if (S.editor.files.js.changed == true) {
                    changeJs();
                }
                showContent();

                //update Rhino browser window (if applicable)
                if (S.editor.Rhino) {
                    S.editor.Rhino.defaulttheme();
                }

                //finally, reload javascript file
                function changeJs(htmlChanged) {
                    $('#website_js').remove();
                    S.util.js.load('/js/website.js' + '?r=' + rnd, 'website_js',
                        function () { 
                            if (S.editor.files.js.changed == true || htmlChanged == true) {
                                S.editor.files.js.changed = false;
                                tagjs.remove();
                                S.util.js.load(src + '?r=' + rnd, 'page_js');
                            }
                        }
                    );
                }

                function showContent() {
                    $('.editor-preview, .editor-tab').removeClass('hide');
                    $('.editor').addClass('hide');
                    S.editor.visible = false;
                }

            },

            hide: function () {
                S.editor.visible = true;
                $('.editor-preview, .editor-tab').addClass('hide');
                $('.editor').removeClass('hide');

                //update Rhino browser window (if applicable)
                if (S.editor.Rhino) {
                    Rhino.bordercolor(34, 34, 34);
                    Rhino.toolbarcolor(34, 34, 34);
                    Rhino.toolbarfontcolor(200, 200, 200);
                    Rhino.toolbarbuttoncolors(
                        S.util.color.argbToInt(255, 34, 34, 34), //bg
                        S.util.color.argbToInt(255, 40, 40, 40), //bg hover
                        S.util.color.argbToInt(255, 0, 153, 255), //bg mouse down
                        S.util.color.argbToInt(255, 200, 200, 200), //font
                        S.util.color.argbToInt(255, 200, 200, 200), //font hover
                        S.util.color.argbToInt(255, 200, 200, 200) //font mouse down
                    );
                }

                if (S.editor.initialized == false) {
                    S.editor.init();
                    return;
                }
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, 10);
            }
        }
    },

    codebar: {
        update: function () {
            var editor = S.editor.instance;
            if (typeof editor == 'undefined' || editor == null) { return;}
            var linenum = 'Ln '; 
            var charnum = 'Col ';
            var linestotal = 'End ';
            switch (S.editor.type) {
                case 0: //monaco
                    var pos = editor.getPosition();
                    var model = editor.getModel();
                    linenum += pos.lineNumber.toString();
                    charnum += pos.column.toString();
                    linestotal += model.getLineCount();
                    break;
                case 1: //ace

                    break;
            }
            $('.code-curr-line').html(linenum);
            $('.code-curr-char').html(charnum);
            $('.code-total-lines').html(linestotal);
        },
        status: function (msg) {
            $('.code-status').html(msg);
        }
    },

    fields: {
        clone: $('.content-fields .textarea-clone > div'),
        selected: '',
        changed: false,
        change: function (e) {
            var field = $(e.target);
            //resize field
            var clone = S.editor.fields.clone;
            clone.html(field.val().replace(/\n/g, '<br/>') + '</br>');
            field.css({ height: clone.height() });
            if (S.editor.fields.changed == false) {
                //enable save menu
                $('.item-save').removeClass('faded').removeAttr('disabled');
                S.editor.fields.changed = true;
            }
        }
    },

    settings: {
        _loaded: false,
        clone: null,
        headers: [],
        footers: [],
        field_template:'',
        
        load: function () {
            var self = S.editor.settings;
            S.editor.tabs.create("Page Settings", "page-settings-section", { isPageResource: true },
                () => { //onfocus
                    $('.tab.page-settings').removeClass('hide');
                },
                () => { //onblur

                },
                () => { //onsave

                }
            );
            if (self._loaded == true) { return; }
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
                    e.cancelBubble;
                    var data = { title: $('#page_title_new_prefix').val(), prefix:true };
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
                    e.cancelBubble;
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
                    if (prefix[prefix.length - 1] != ' ') { prefix += ' ';}
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

        }
    },

    appsettings: {
        show: function () {
            S.editor.tabs.create("App Settings", "app-settings-section", {},
                () => { //onfocus
                    $('.tab.app-settings').removeClass('hide');
                },
                () => { //onblur

                },
                () => { //onsave

                }
            );
            S.editor.dropmenu.hide();
            $('.editor .sections > .tab:not(.file-browser)').addClass('hide');
            $('.editor .sections > .app-settings').removeClass('hide');

            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

            S.ajax.post('AppSettings/Render', {},
                function (d) {
                    var data = JSON.parse(d);
                    S.ajax.inject(data);
                }
            );
        }
    },

    resources: {
        _loaded: false,
        uploader: null,
        path: '',

        load: function (path) {
            var self = S.editor.resources;
            S.editor.tabs.create("Page Resources", "page-resources-section", { isPageResource: true },
                () => { //onfocus
                    $('.tab.page-resources').removeClass('hide');
                },
                () => { //onblur

                },
                () => { //onsave

                }
            );
            if (self._loaded == true && self.path == path) { return; }
            S.editor.resources.path = path;
            $('.sections > .page-resources').html('');
            S.ajax.post('PageResources/Render', { path: path },
                function (d) {
                    $('.sections > .page-resources').html(d);
                    self._loaded = true;
                    var p = path.replace('content/', '');
                    $('.page-name').attr('href', '/' + p).html(p);

                    //initialize uploader
                    if (self.uploader == null) {
                        S.editor.resources.uploader = launchPad({
                            url: 'Upload/Resources',
                            onUploadStart: function (files, xhr, data) {
                                data.append('path', S.editor.resources.path);
                            },

                            onQueueComplete: function () {
                                S.editor.resources._loaded = false;
                                $('.sections .page-resources').children().remove();
                                S.editor.resources.load(S.editor.resources.path);
                            }
                        });
                    }
                    $('.button.uploader').on('click', self.uploader.click);
                }
            );
        },

        delete: function (file, elem) {
            if (!confirm('Do you really want to delete the file "' + file + '"? This cannot be undone.')) { return;}
            S.ajax.post('PageResources/Delete', { path: S.editor.resources.path, file: file },
                function (d) {
                    $(elem).parents('li').first().remove();
                },

                function () { S.editor.error('Could not delete resource on the server.'); }
            );
        }
    },

    hotkey: {
        pressed: function (e) {
            if (S.editor.visible == false) { return;}
            var has = false;
            var key = String.fromCharCode(e.which).toLowerCase();
            if (e.ctrlKey == true) {
                switch (key) {
                    case 's':
                        //save file
                        S.editor.save();
                        has = true;
                        break;
                }
            }
            if (has == true) {
                event.preventDefault();
                return false;
            }
        },

        pressedPreview: function (e) {
            if (e.ctrlKey == false && e.altKey == false && e.shiftKey == false) {
                switch (e.which) {
                    case 27: //escape key
                        S.editor.filebar.preview.toggle();
                        break;
                }
            }
        }
    }
};

//set up editor tab
$('.editor-tab').on('click', S.editor.filebar.preview.hide);

//register hotkeys for preview mode
$(window).on('keydown', S.editor.hotkey.pressedPreview);