S.editor.explorer = {
    path: 'root',
    routes: [],
    queue: [],

    show: function () {
        S.editor.dropmenu.hide();
        if (!$('.editor .file-browser').hasClass('hide')) { S.editor.explorer.hide(); return; }
        if ($('.file-browser ul.menu').children().length == 0) {
            S.editor.explorer.dir('root');
        }
        $('.editor .file-browser').removeClass('hide');
        $('.editor').addClass('show-browser');
        S.editor.resize.window();
    },

    hide: function () {
        $('.editor .file-browser').addClass('hide');
        $('.editor').removeClass('show-browser');
    },

    dir: function (path) {
        if (path == null) { path = S.editor.explorer.path;}
        S.ajax.post('Files/Dir', { path: path, showDelete:true },
            function (d) {
                S.editor.explorer.path = path;
                $('.file-browser ul.menu').html(d);
                //add event listeners to delete buttons
                $('.file-browser .delete-file').on('click', (e) => {
                    e.cancelBubble = true;
                    S.editor.file.delete($(e.target).parents('.item').first().attr('data-path'));
                });
                $('.file-browser .delete-folder').on('click', (e) => {
                    e.cancelBubble = true;
                    S.editor.folder.delete($(e.target).parents('.item').first().attr('data-path'));
                });
                //add event listener for renaming files
                $('.file-browser .type-file .label, .file-browser .type-file .label').on('click', (e) => {
                    var parent = $(e.target).parents('.item').first();
                    var path = parent.attr('data-path');
                    var tab = $('.tab-' + S.editor.fileId(path) + '.selected');
                    if (tab.length == 1 && ['/pages/', 'website.less'].filter(a => path.indexOf(a) >= 0).length == 0) {
                        e.cancelBubble = true;
                        e.stopPropagation();
                    }
                });
                $('.file-browser .type-file .label, .file-browser .type-file .label').on('mousedown', (e) => {
                    let parent = $(e.target).parents('.item').first();
                    let path = parent.attr('data-path');
                    var tab = $('.tab-' + S.editor.fileId(path) + '.selected');
                    if (tab.length == 1 && ['/pages/', 'website.less'].filter(a => path.indexOf(a) >= 0).length == 0) {
                        //show textbox to rename file with
                        e.cancelBubble = true;
                        e.stopPropagation();
                        let label = parent.find('.label');
                        if (label.find('input').length == 1) { return;}
                        let labelName = label.html();
                        label.html('<input type="text" class="rename-file" spellcheck="false" value="' + labelName + '"/>');
                        label.find('input').on('click', (e) => {
                            e.cancelBubble();
                        });
                        label.find('input').on('keydown', (e) => {
                            //detect key press for text box
                            var found = false;
                            switch (e.key.toLowerCase()) {
                                case "enter":
                                    //rename file
                                    var val = label.find('input').val();
                                    S.ajax.post('Files/RenameFile',
                                        { path: path, newname: val },
                                        (e) => {
                                            label.html(val);
                                            //change file browser item
                                            parent.attr('data-path', parent.attr('data-path').replace(labelName, val));
                                            parent.attr('onclick', parent.attr('onclick').replace(labelName, val));
                                            parent.find('.label').attr('title', val);
                                            //change tab associated with file
                                            var tab = $('.tab-' + S.editor.fileId(path));
                                            var isselected = tab.hasClass('selected');
                                            path = path.replace(labelName, val);
                                            tab[0].className = 'tab-' + S.editor.fileId(S.editor.fileId(path)) + (isselected ? ' selected' : '');
                                            tab.find('.tab-title').html(val);
                                            var div = tab.children().first();
                                            div.attr('data-path', div.attr('data-path').replace(labelName, val))
                                                .attr('onclick', div.attr('onclick').replace(labelName, val));
                                            if (isselected) {
                                                var filebar = $('.file-bar .file-path');
                                                filebar.html(filebar.html().replace(labelName, val));
                                                S.editor.selected = S.editor.selected.replace(labelName, val);
                                                S.editor.sessions.selected = S.editor.sessions.selected.replace(labelName, val);
                                            }
                                        }, (err) => {
                                            S.editor.message('', err.responseText, 'error');
                                        });
                                    found = true;
                                    break;
                                case 'escape':
                                    //cancel rename
                                    label.html(labelName);

                                    found = true;
                                    S.editor.filebar.preview.toggle();
                                    break;
                            }
                            if (found) {
                                e.cancelBubble = true;
                                e.stopPropagation();
                            }
                        });
                    }
                });
                var url = path;
                if (path.indexOf('root') == 0) {
                    url = url.replace('root', '').replace('content/', '');
                }
                url += '/';
                $('.browser-path').html(url);
                if (path.indexOf('wwwroot') == 0) {
                    //load resources section
                    S.editor.filebar.resources.show(true);

                    //hide all filebar icons except resources icon
                    $('ul.file-tabs > li:not(.tab-page-resources)').hide();

                    //change filebar path
                    S.editor.filebar.update(path, 'icon-folder');

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
        let paths = path.split('/');
        let id = S.editor.fileId(path);
        S.editor.tabs.changed = false;

        if (isready !== false) {
            //update selected session
            S.editor.selected = path;
            //deselect tabs
            $('.edit-tabs ul.row li, .edit-tabs ul.row > li > div').removeClass('selected');
            //disable save menu
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
        }

        //check for existing tab
        let tab = $('.edit-tabs ul.row .tab-' + id);

        //find route that matches path (if route exists)
        var route = S.editor.explorer.routes.filter(a => a.path == path);
        if (route.length > 0) {
            route = route[0];
            var routes = S.editor.explorer.routes.filter(a => a.path != path);
            routes.forEach(a => {
                if (typeof a.onblur == 'function') { a.onblur(); }
            });
            tab.find('.row.hover').addClass('selected');
            $('.editor > div > .sections > div').addClass('hide');
            route.onfocus();
            return;
        }

        //get file info
        let file = paths[paths.length - 1];
        var fileparts = paths[paths.length - 1].split('.', 2);
        let isPageResource = S.editor.isResource(path);
        if (path.indexOf('content/pages/') >= 0 && path.indexOf('.html') > 0 && isready == true) {
            //redirect to page instead of opening tab
            if (S.editor.files.html.changed == true) {
                //TODO:confirm if user wants to save changes to html page
            }
            window.parent.location.href = path.replace('content/pages', '').replace('.html', '');
            return;
        }

        if (tab.length == 0) {
            //tab doesn't exist yet
            var title = file;
            if (fileparts[0].length > 25) { title = '...' + fileparts[0].substr(fileparts[0].length - 25) + '.' + fileparts[1]; }
            S.editor.tabs.create(title, path, { isPageResource: isPageResource, selected: isready !== false, canClose: !isPageResource }, loadCode);
            if (isready == false && typeof callback == 'function') { callback(); }
        } else {
            //tab exists
            tab.find('.row.hover').addClass('selected');
        }

        function loadCode() {
            S.editor.filebar.code.show();
            if (isPageResource || (paths.indexOf('partials') >= 0 && file.indexOf('.html') > 0)) {
                //show file bar icons for page html resource
                $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').show();
            } else {
                $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').hide();
            }
            if (path.indexOf('.html') > 0) {
                $('.tab-components').show();
            } else {
                $('.tab-components').hide();
            }
            //check for existing source code
            var nocode = (code == null || typeof code == 'undefined');
            var session = S.editor.sessions[id];
            var editor = S.editor.instance;
            var ext = paths[paths.length - 1].split('.')[1];
            var mode = 'html-mustache';
            switch (ext) {
                case 'css': case 'less': mode = ext; break;
                case 'js': mode = 'javascript'; break;
            }

            //change file path
            var cleanPath = path;
            if (cleanPath.indexOf('content/partials/') == 0) {
                cleanPath = cleanPath.replace('content/', '');
            } else if (cleanPath.indexOf('content/') == 0) {
                cleanPath = cleanPath.replace('content/', '');
            }
            if (cleanPath.indexOf('root/') == 0) { cleanPath = cleanPath.replace('root/', ''); }

            //set file bar path text & icon
            S.editor.filebar.update(cleanPath, 'icon-file-' + ext);
            if (session == null && nocode == true) {
                //load new session from ajax POST, loading code from server
                S.ajax.post("Files/Open", { path: path, pageResource: isPageResource === true },
                    function (d) {
                        S.editor.sessions.add(id, mode, S.editor.decodeHtml(d));
                        S.editor.resize.window();
                        if (typeof callback == 'function') { callback(); }
                    },
                    function () {
                        S.editor.error();
                    }
                );
            } else if (nocode == false) {
                //load new session from provided code argument
                S.editor.sessions.add(id, mode, S.editor.decodeHtml(code));
                if (typeof callback == 'function') { callback(); }

            } else {
                //load existing session
                switch (S.editor.type) {
                    case 0: //monaco
                        //save viewstate for currently viewed session
                        S.editor.sessions.saveViewState(S.editor.fileId(S.editor.sessions.selected));

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
                S.editor.resize.window();
                setTimeout(function () {
                    S.editor.resize.window();
                }, 200);
                if (typeof callback == 'function') { callback(); }
            }
            S.editor.sessions.selected = path;
        }
        if (tab.length > 0) { loadCode(); }
    },

    select: (title, path, filetypes, callback) => {
        //show a pop up to select a file from the file explorer
        var html = $('#template_select_file').html();
        let popup = S.popup.show(title, html);
        //load path via api
        S.ajax.post('Files/Dir', { path: path, fileTypes:filetypes, showDelete:false },
            function (d) {
                $('.modal-browser ul.menu').html(d);
                var folders = $('.modal-browser .item[data-type="folder"]');
                var files = $('.modal-browser .item[data-type="file"]');
                folders.attr('onclick', '');
                files.attr('onclick', '');
                folders.on('click', (e) => {
                    var target = $(e.target);
                    if (!target.hasClass('item')) { target = target.parents('.item').first(); }
                    S.editor.explorer.select(title, target.attr('data-path'), filetypes, callback);
                    S.popup.hide(popup);
                });
                files.on('click', (e) => {
                    var target = $(e.target);
                    if (!target.hasClass('item')) { target = target.parents('.item').first(); }
                    callback(target.attr('data-path'));
                    S.popup.hide(popup);
                });
                var url = path;
                if (path.indexOf('root') == 0) {
                    url = url.replace('root', '');
                }
                url += '/';
                $('.popup .modal-path').html(url);
            },
            function () {
                S.editor.error();
            }
        );
    }
};