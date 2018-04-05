S.editor = {
    type: 0, //0 = Monaco, 1 = Ace
    instance: null,
    sessions: {},
    selected: '',
    path: '',
    theme: 'dark',
    div: $('.code-editor'),
    divFields: $('.content-fields'),
    divBrowser: $('.file-browser'),
    initialized: false,

    init: function () {
        this.initialized = true;

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
                require.config({ paths: { 'vs': '/js/utility/monaco' } });
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
                    editor.onKeyUp((e) => {
                        var specialkeys = [18, 37, 38, 39, 40, 20, 17, 35, 112, 113, 114, 115, 116, 117, 118, 118, 120, 121, 122, 123, 36, 144, 33, 34, 91];
                        if (specialkeys.indexOf(e.keyCode) < 0 && e.ctrlKey == false && e.altKey == false) {
                            //content only changes if special keys are not pressed
                            S.editor.changed();
                        }
                    });
                    editor.onMouseUp((e) => {
                        S.editor.codebar.update();
                    });
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
        $('.tab-content-fields').on('click', S.editor.filebar.fields.show);
        $('.tab-file-code').on('click', S.editor.filebar.code.show);
        $('.tab-page-settings').on('click', S.editor.filebar.settings.show);
        $('.tab-page-resources').on('click', S.editor.filebar.resources.show);
        $('.tab-preview').on('click', S.editor.filebar.preview.show);
        $('.editor-drop-menu .item-content-fields').on('click', function () { S.editor.filebar.fields.show(true); });
        $('.editor-drop-menu .item-new-file').on('click', S.editor.file.create.show);
        $('.editor-drop-menu .item-new-folder').on('click', S.editor.folder.create.show);
        $('.editor-drop-menu .item-new-window').on('click', S.editor.newWindow);
        $('.page-settings .title-prefix .icon a').on('click', S.editor.settings.title.prefix.show);
        $('.page-settings .title-suffix .icon a').on('click', S.editor.settings.title.suffix.show);

        //add window resize event
        $(window).on('resize', S.editor.resizeWindow);

        //register hotkeys
        $(window).on('keydown', S.editor.hotkey.pressed);

        //finally, load content resources that belong to the page
        S.editor.explorer.openResources(dir, [fileparts[0] + '.html', fileparts[0] + '.less', fileparts[0] + '.js'],
            function () {
                setTimeout(function () {
                    S.editor.codebar.status('Ready');
                    S.editor.codebar.update();
                }, 500);
            }
        );
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
        var div = S.editor.div;
        var browser = S.editor.divBrowser;
        var fields = S.editor.divFields;
        var pos = div.offset();
        var pos2 = browser.offset();
        div.css({ height: '' });
        if (pos.top == 0) {
            pos = fields.offset();
        }
        $('.code-editor, .content-fields').css({ height: win.h - pos.top });
        $('.page-settings, .page-resources').css({ minHeight: win.h - pos.top });
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
        var id = S.editor.fileId();
        window.open(window.location.href, 'Editor_' + id, 'width=1800,height=900,left=50,top=50,toolbar=No,location=No,scrollbars=auto,status=No,resizable=yes,fullscreen=No');
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

    error: function (msg) {
        S.message.show('.editor .message', 'error', msg || S.message.error.generic);
    },

    message: function (msg) {
        S.message.show('.editor .message', 'confirm', msg);
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
        var tab = $('.tab-' + id);
        if ($('.editor-drop-menu .item-save.faded').length == 1) { return;}
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

                    S.ajax.post('Editor/SaveContentFields', { path: path, fields: fields, language: $('#lang').val() },
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
                        S.ajax.post('Editor/UpdatePageTitle', data,
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
                        S.ajax.post('Editor/UpdatePageDescription', data,
                            function (d) {
                                //show message to user
                                showmsg();
                            },
                            function () { S.editor.error(); }
                        );
                    }

                    function showmsg() {
                        S.message.show('.editor .message', 'confirm', 'Page settings have been updated successfully');
                    }

                    return;
                }
            } 
        }

        //last resort, save source code to file
        S.ajax.post('Editor/SaveFile', { path: path, content: content },
            function (d) {
                if (d != "success") {
                    S.editor.error();
                } else {
                    //check whether or not file was a required page resource for this page
                    if (S.editor.isResource(path)) {
                        var ext = S.editor.fileExt(path);
                        S.editor.files[ext].changed = true;
                        if (ext == 'html') {
                            //also mark content as changed so content fields can reload
                            S.editor.files.content.changed = true;
                        }
                    }

                    self.unChanged(path);
                    S.editor.explorer.open(path);
                }
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
                S.ajax.post('Editor/NewFile', data,
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
                S.ajax.post('Editor/NewFolder', data,
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
        if (path == null) { path = 'content' + window.location.pathname.toLowerCase();}
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

                        if (select !== false) {
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
            S.editor.sessions[id].destroy();
            delete S.editor.sessions[id];
        }
    },

    isResource: function (path) {
        var paths = path.split('/');
        var file = paths[paths.length - 1];
        var dir = paths.join('/').replace(file, '');
        var fileparts = paths[paths.length - 1].split('.', 2);
        var relpath = dir + fileparts[0];
        return relpath.toLowerCase() == S.editor.path;
    },

    files: {
        //required page files, track whether or not they've 
        //been updated on the server via the editor
        html: { changed: false },
        less: { changed: false },
        js: { changed: false },
        content: {changed: false}
    },

    tabs: {
        close: function (path) {
            var id = S.editor.fileId(path);
            var tab = $('.tab-' + id);
            var sibling = tab.prev().find('.row.hover');
            tab.remove();
            S.editor.sessions.remove(id);

            //check to see if selected tab is being removed
            if (S.editor.selected == path && sibling.length == 1) {
                sibling[0].click();
            }
        }
    },
    
    explorer: {
        path: 'root',

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
            S.ajax.post('Editor/Dir', { path: path },
                function(d) {
                    S.editor.explorer.path = path;
                    $('.file-browser ul.menu').html(d);
                    var url = path;
                    if (path.indexOf('root' == 0)) {
                        url = url.replace('root', '');
                    }
                    url += '/';
                    $('.browser-path').html(url);
                },
                function () {
                    S.editor.error();
                }
            );
        },

        openResources: function (path, files, callback) {
            //opens a group of resources (html, less, js) from a specified path
            var self = S.editor.explorer;
            files.forEach((f) => {
                self.queue.push(path + f);
            });
            self.runQueue(true, callback);
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
            var id = S.editor.fileId(path);

            if (isready !== false) {
                //update selected session
                S.editor.selected = path;
                //deselect tabs
                $('.edit-tabs ul.tabs li, .edit-tabs ul.tabs > li > div').removeClass('selected');
                //disable save menu
                $('.item-save').addClass('faded').attr('disabled', 'disabled');
            }

            //check for existing tab
            var tab = $('.edit-tabs ul.tabs .tab-' + id);
            var paths = path.split('/');
            var file = paths[paths.length - 1];
            var fileparts = paths[paths.length - 1].split('.', 2);
            var isPageResource = S.editor.isResource(path);

            if (tab.length == 0) {
                //tab doesn't exist yet
                var temp = $('#template_tab').html().trim();
                var title = file;
                if (fileparts[0].length > 18) { title = '...' + fileparts[0].substr(fileparts[0].length - 15) + '.' + fileparts[1];}
                $('.edit-tabs ul.tabs').append(temp
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
                    $('.tab-content-fields, .tab-page-settings, .tab-page-resources, .tab-preview').show();
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
            if (path.indexOf('content/') == 0) { cleanPath = path.replace('content/', 'content/pages/'); }
            if (path.indexOf('root/') == 0) { cleanPath = path.replace('root/', ''); }

            if (isready !== false) {
                //set file bar path text & icon
                $('#filepath').val(cleanPath);
                $('.file-bar .file-icon use')[0].setAttribute('xlink:href', '#icon-file-' + ext);
            }

            if (session == null && nocode == true) {
                //load new session from ajax POST
                S.ajax.post("Editor/Open", { path: path },
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
                        editor.setModel(session);
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
                if (loadtab === true) {
                    S.editor.explorer.open(S.editor.path + '.html');
                }

                //show content fields section & hide other sections
                $('.editor .sections > .tab:not(.file-browser)').addClass('hide');
                $('.editor .sections > .content-fields').removeClass('hide');
                $('ul.file-tabs > li').removeClass('selected');
                $('ul.file-tabs > li.tab-content-fields').addClass('selected');

                var lang = 'en';

                if ($('#lang').children().length == 0) {
                    //load list of languages
                    S.ajax.post('Editor/Languages', {},
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
                    S.ajax.post('Editor/RenderContentFields', { path: S.editor.path, language:lang },
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
            show: function () {
                S.editor.dropmenu.hide();
                $('.editor .sections > .tab:not(.file-browser)').addClass('hide');
                $('.editor .sections > .page-resources').removeClass('hide');
                $('ul.file-tabs > li').removeClass('selected');
                $('ul.file-tabs > li.tab-page-resources').addClass('selected');

                //disable save menu
                $('.item-save').addClass('faded').attr('disabled', 'disabled');
                $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

                S.editor.resources.load();
            }
        },

        preview: {
            toggle: function () {
                var self = S.editor.filebar.preview;
                if ($('.preview').hasClass('hide')) {
                    self.show();
                } else {
                    self.hide();
                }
            },
            show: function () {
                var tagcss = $('#page_css');
                var tagjs = $('#page_js');
                var css = tagcss.attr('href');
                var src = tagjs.attr('src').split('?')[0];
                var rnd = Math.floor(Math.random() * 9999);

                //first, reload CSS
                if (S.editor.resources.less.changed == true) {
                    S.editor.resources.less.changed = false;
                    tagcss.remove();
                    $('head').append(
                        '<link rel="stylesheet" type="text/css" id="page_css" href="' + css + '?r=' + rnd + '"></link>'
                    );
                }


                //next, reload rendered HTML
                if (S.editor.resources.html.changed == true) {
                    S.editor.resources.html.changed = false;
                    S.ajax.post('Editor/RenderPage', { path: S.editor.path + '.html', language: window.language },
                        function (d) {
                            $('.preview > .content').html(d);
                            changeJs(true);
                        }
                    );
                } else {
                    changeJs();
                }

                //finally, reload javascript file
                function changeJs(htmlChanged) {
                    if (S.editor.resources.js.changed == true) {
                        S.editor.resources.js.changed = false;
                        tagjs.remove();
                        S.util.js.load(src + '?r=' + rnd, 'page_js',
                            function () { showContent(); }
                        );
                    } else {
                        if (htmlChanged === true) {
                            //reload javascript anyway since HTML changed
                            tagjs.remove();
                            S.util.js.load(src, 'page_js',
                                function () { showContent(); }
                            );
                        } else {
                            //no need to reload Js, HTML didn't change
                            showContent();
                        }
                    }
                }

                function showContent() {
                    $('.preview, .editor-tab').removeClass('hide');
                    $('.editor').addClass('hide');
                }

            },

            hide: function () {
                $('.preview, .editor-tab').addClass('hide');
                $('.editor').removeClass('hide');
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

        load: function () {
            var self = S.editor.settings;
            if (self._loaded == true) { return; }
            var path = S.editor.path;
            S.ajax.post('Editor/RenderPageSettings', { path: path },
                function (d) {
                    $('.sections > .page-settings').append(d);
                    self._loaded = true;
                    self.clone = $('.page-settings .textarea-clone > div');
                    var p = path.replace('content/', '');
                    $('.page-name').attr('href', '/' + p).html(p);

                    //set up events to detect changes
                    var description = $('#page_description');
                    $('#page_title_prefix, #page_title_suffix, #page_title').on('change, keyup', self.title.change);
                    description.on('change, keyup, keydown', self.description.change);
                    self.change(description, true);
                }
            );
        },

        change: function (field, changed) {
            //update textarea height for given field
            var clone = S.editor.settings.clone;
            clone.html(field.val().replace(/\n/g, '<br/>') + '</br>');
            field.css({ height: clone.height() });
            if (changed == false) {
                //enable save menu
                $('.item-save').removeClass('faded').removeAttr('disabled');
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
                    S.ajax.post('Editor/CreatePageTitlePart', data,
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
                    S.ajax.post('Editor/CreatePageTitlePart', data,
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
        }
    },

    resources: {
        _loaded: false,

        load: function () {
            var self = S.editor.resources;
            if (self._loaded == true) { return; }
            var path = S.editor.path;
            S.ajax.post('Editor/RenderPageResources', { path: path },
                function (d) {
                    $('.sections > .page-resources').append(d);
                    self._loaded = true;
                    var p = path.replace('content/', '');
                    $('.page-name').attr('href', '/' + p).html(p);
                }
            );
        }
    },

    hotkey: {
        pressed: function (e) {
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