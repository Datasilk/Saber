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
        S.editor.resizeWindow();
    },

    hide: function () {
        $('.editor .file-browser').addClass('hide');
        $('.editor').removeClass('show-browser');
    },

    dir: function (path) {
        if (path == null) { path = S.editor.explorer.path;}
        S.ajax.post('Files/Dir', { path: path },
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
        var paths = path.split('/');
        var id = S.editor.fileId(path);
        var prevId = '';
        if (S.editor.selected != '') {
            prevId = S.editor.fileId(S.editor.selected);
        }
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
        var tab = $('.edit-tabs ul.row .tab-' + id);

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
        var file = paths[paths.length - 1];
        var fileparts = paths[paths.length - 1].split('.', 2);
        var isPageResource = S.editor.isResource(path);
        if (path.indexOf('content/pages/') >= 0 && path.indexOf('.html') > 0 && isready == true) {
            //redirect to page instead of opening tab
            if (S.editor.files.html.changed == true) {
                //TODO:confirm if user wants to save changes to html page
            }
            location.href = path.replace('content/pages', '').replace('.html', '');
            return;
        }

        if (tab.length == 0) {
            //tab doesn't exist yet
            var temp = $('#template_tab').html().trim();
            var title = file;
            //truncate title with ... prefix
            if (fileparts[0].length > 18) { title = '...' + fileparts[0].substr(fileparts[0].length - 15) + '.' + fileparts[1]; }
            //generate tab html
            $('.edit-tabs ul.row').append(temp
                .replace(/\#\#id\#\#/g, id)
                .replace(/\#\#path\#\#/g, path)
                .replace('##title##', title)
                .replace(/\#\#selected\#\#/g, isready !== false ? 'selected' : '')
                .replace('##tab-type##', isPageResource ? 'page-level' : '')
                .replace('##resource-icon##', isPageResource ? '' : 'hide')
            );
            //add button events for tab
            if (!isPageResource) {
                $('.tab-' + id + ' .btn-close').on('click', function (e) {
                    S.editor.tabs.close(id, path);
                    e.preventDefault();
                    e.cancelBubble = true;
                });
            } else {
                //hide close button, tab is special
                $('.tab-' + id + ' .btn-close').hide();
            }

        } else {
            //tab exists
            tab.find('.row.hover').addClass('selected');
        }

        if (isready !== false) {
            $('.tab-components, .tab-content-fields, .tab-page-settings, .tab-page-resources, .tab-preview').hide();
            if (isPageResource) {
                //show file bar icons for page html resource
                $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').show();
            }
            if (path.indexOf('.html') > 0) {
                $('.tab-components').show();
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
        } else if (path.indexOf('content/pages')) {
        } else if (path.indexOf('content/') == 0) {
            cleanPath = path.replace('content/', 'content/pages/');
        }
        if (path.indexOf('root/') == 0) { cleanPath = path.replace('root/', ''); }

        if (isready !== false) {
            //set file bar path text & icon
            S.editor.filebar.update(cleanPath, 'icon-file-' + ext);
        }

        if (session == null && nocode == true) {
            //load new session from ajax POST, loading code from server
            S.ajax.post("Files/Open", { path: path, pageResource: isPageResource === true },
                function (d) {
                    S.editor.sessions.add(id, mode, S.editor.decodeHtml(d), isready !== false && S.editor.tabs.changed == false);
                    if (typeof callback == 'function') { callback(); }
                },
                function () {
                    S.editor.error();
                }
            );
        } else if (nocode == false) {
            //load new session from provided code argument
            S.editor.sessions.add(id, mode, S.editor.decodeHtml(code), isready !== false && S.editor.tabs.changed == false);
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
};