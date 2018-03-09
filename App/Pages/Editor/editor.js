S.editor = {
    instance: null,
    EditSession: require("ace/edit_session").EditSession,
    sessions: {},
    selected: '',
    div: $('.code-editor'),

    init: function () {
        //initialize code editor
        var editor = ace.edit("editor");
        editor.setTheme("ace/theme/xcode");

        this.instance = editor;
        this.resize();

        //add button events
        $('.item-browse').on('click', S.editor.explorer.show);
        $('.tab-drop-menu').on('click', S.editor.dropmenu.show);
        $('.bg-overlay').on('click', S.editor.dropmenu.hide);
        $('.editor-drop-menu .item-save').on('click', function () { S.editor.save(S.editor.selected, S.editor.instance.getValue()); });
        $('.editor-drop-menu .item-save-as').on('click', S.editor.saveAs);
        $('.tab-preview').on('click', S.editor.preview.show);
        $('.editor-tab').on('click', S.editor.preview.hide);

        //add window resize event
        $(window).on('resize', S.editor.resizeWindow);

        //register hotkeys
        $(window).on('keydown', S.editor.hotkey.pressed);
    },

    resize: function () {
        var editor = S.editor.instance;
        var newHeight = editor.getSession().getScreenLength() * editor.renderer.lineHeight + editor.renderer.scrollBar.getWidth();
        if (newHeight < 20) { newHeight = 20; }
        newHeight += 30;
        $('#editor').css({ minHeight: newHeight.toString() + "px" });
        $('#editor-section').css({ minHeight: newHeight.toString() + "px" });
        editor.resize();
        S.editor.resizeWindow();
    },

    resizeWindow: function () {
        var win = S.window.pos();
        var div = S.editor.div;
        var pos = div.offset();
        $('.code-editor').css({ height: win.h - pos.top });
        
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

    changed: function (checkall) {
        var self = S.editor;
        var id = self.fileId(self.selected);
        var tab = $('.tab-' + id);
        if (tab.length == 1) {
            if (tab.attr('data-edited') != "true" || checkall == true) {
                $('.item-save').removeClass('faded').removeAttr('disabled');
                if (tab.attr('data-edited') != "true") {
                    tab.attr('data-edited', "true");
                    var col = tab.find('.col');
                    col.html(col.html() + ' *');
                }
            }
        }
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

    save: function (path, content) {
        if ($('.editor-drop-menu .item-save.faded').length == 1) { return;}
        var self = S.editor;
        self.dropmenu.hide();
        S.ajax.post('Editor/SaveFile', { path: path, content: content },

            function (d) {
                if (d != "success") {
                    S.message.show('.editor .message', S.message.error.generic);
                } else {
                    self.unChanged(path);
                    S.editor.explorer.open(path);
                }
            },
            function () {
                S.message.show('.editor .message', S.message.error.generic);
            }
        );
    },

    saveAs: function () {
        S.editor.dropmenu.hide();
        var oldpath = S.editor.selected;
        var path = prompt("Save As...", oldpath);
        if (path != '' && path != null) {
            S.editor.save(path, S.editor.instance.getValue());
        }
    },

    fileId: function (path) {
        return path.replace(/\//g, '_').replace(/\./g, '_');
    },

    decodeHtml(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    },

    sessions: {
        add: function (id, mode, code, select) {
            var editor = S.editor.instance;
            session = new S.editor.EditSession(S.editor.decodeHtml(code));
            session.setMode("ace/mode/" + mode);
            session.on('change', S.editor.changed);
            S.editor.sessions[id] = session;
            if (select !== false) {
                editor.setSession(session);
                editor.clearSelection();
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, 200);
                editor.focus();
            }
        },

        remove: function (id) {
            S.editor.sessions[id].destroy();
            delete S.editor.sessions[id];
        }
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
        queue: [],

        show: function () {
            S.editor.dropmenu.hide();
            if (!$('.editor .file-browser').hasClass('hide')) { S.editor.explorer.hide(); return;}
            if ($('.file-browser ul.menu').children().length == 0) {
                S.editor.explorer.dir('root');
            }
            $('.editor .file-browser').removeClass('hide');
            $('.editor').addClass('show-browser');
        },

        hide: function () {
            $('.editor .file-browser').addClass('hide');
            $('.editor').removeClass('show-browser');
        },

        dir: function (path) {
            S.ajax.post('Editor/Dir', { path: path },
                function (d) {
                    $('.file-browser ul.menu').html(d);
                },
                function () {
                    S.message.show('.editor .message', S.message.error.generic);
                }
            );
        },

        openResources: function (path, files) {
            //opens a group of resources (html, less, js) from a specified path
            var self = S.editor.explorer;
            files.forEach((f) => {
                self.queue.push(path + f);
            });
            self.runQueue(true);
        },

        runQueue: function (first) {
            //opens next resource in the queue
            var self = S.editor.explorer;
            var queue = self.queue;
            if (queue.length > 0) {
                var path = queue[0].toString();
                queue.splice(0, 1);
                self.open(path, null, first === true, self.runQueue);
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
            if (tab.length == 0) {
                var temp = $('#template_tab').html().trim();
                var paths = path.split('/');
                var file = paths[paths.length - 1];
                var dir = '/' + paths.join('/').replace(file, '');
                var fileparts = paths[paths.length - 1].split('.', 2);
                var relpath = dir + fileparts[0];
                var title = file;
                var isPageResource = relpath.toLowerCase() == '/content' + window.location.pathname.toLowerCase();
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
                tab.addClass('selected').find('.row.hover').addClass('selected');
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
                        S.editor.sessions.add(id, mode, d, isready !== false);
                        if (typeof callback == 'function') { callback();}
                    },
                    function () {
                        S.message.show('.editor .message', S.message.error.generic);
                    }
                );
            } else if (nocode == false) {
                //load new session from provided code argument
                S.editor.sessions.add(id, mode, code, isready !== false);
                if (typeof callback == 'function') { callback(); }
                
            } else {
                //load existing session
                editor.setSession(session);
                if (S.editor.isChanged(path) == true) {
                    S.editor.changed(true);
                }
                S.editor.resize();
                setTimeout(function () {
                    S.editor.resize();
                }, 200);
                editor.focus();
                if (typeof callback == 'function') { callback(); }
            }
        }
    },

    preview: {
        show: function () {
            var tagcss = $('#page_css');
            var tagjs = $('#page_js');
            var css = tagcss.attr('href');
            var js = tagjs.attr('src');
            var rnd = Math.floor(Math.random() * 9999);
            tagcss.remove();
            tagjs.remove();
            $('head').append(
                '<link rel="stylesheet" type="text/css" id="page_css" href="' + css + '?r=' + rnd + '"></link>'
            );
            S.util.js.load(js + '?r=' + rnd, 'page_js', function () {
                $('.preview, .editor-tab').removeClass('hide');
                $('.editor').addClass('hide');
            });
            setTimeout(function () {
                
            }, 500);
        },

        hide: function () {
            $('.preview, .editor-tab').addClass('hide');
            $('.editor').removeClass('hide');
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
                        S.editor.save(S.editor.selected, S.editor.instance.getValue());
                        has = true;
                        break;
                }
            }
            if (has == true) {
                event.preventDefault();
                return false;
            }
        }
    }
};

S.editor.init();