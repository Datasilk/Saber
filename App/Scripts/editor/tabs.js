S.editor.tabs = {
    changed: false,
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
            $('.tab-toolbar').html('');
        }
        var elem = $('.edit-tabs ul.row .tab-' + id);
        var routes = S.editor.explorer.routes;
        if (elem.length == 0) {
            //load new tab
            var temp = $('#template_tab').html().trim();
            $('.edit-tabs ul.row').append(temp
                .replace(/\#\#id\#\#/g, id)
                .replace(/\#\#path\#\#/g, path)
                .replace('##title##', title)
                .replace('##tab-type##', '')
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
                    $('.tab-components, .tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').hide();
                    if (opts.isPageResource == true) {
                        $('.tab-content-fields, .tab-file-code, .tab-page-settings, .tab-page-resources, .tab-preview').show();
                    }
                    if (path.indexOf('.html') > 0) {
                        $('.tab-components').show();
                    }
                    elem.addClass('selected');
                    $('.tab-toolbar').html('');
                    S.editor.tabs.changed = true;
                    if (typeof onfocus == 'function') { onfocus(); }
                },
                onblur: () => {
                    $('.tab-components').hide();
                    if (typeof onblur == 'function') { onblur(); }
                },
                onsave: onsave
            };
            if (opts.selected == true) {
                for (var x = 0; x < routes.length; x++) {
                    //blur all other tabs
                    routes[x].onblur();
                }
                route.onfocus();
            }
            
            S.editor.explorer.routes.push(route);
        } else {
            var blur = routes.filter(a => a.path != path)
            for (var x = 0; x < blur.length; x++) {
                //blur all other tabs
                blur[x].onblur();
            }
            route = routes.filter(a => a.path == path)[0];
            route.onfocus();
        }
        S.editor.selected = path;
    },
    select: (id) => {
        $('.edit-tabs li > div').removeClass('selected');
        $('.tab-' + id + ' > div').addClass('selected');
        var blur = S.editor.explorer.routes.filter(a => a.id != id)
        for (var x = 0; x < blur.length; x++) {
            //blur all other tabs
            blur[x].onblur();
        }
        $('.tab-' + id)[0].focus();
    },
    close: function (id, path, callback) {
        var tab = $('.tab-' + id);
        var sibling = tab.prev().find('.row.hover');
        tab.remove();
        S.editor.sessions.remove(id);

        //check to see if selected tab is being removed
        if (S.editor.selected == path && sibling.length == 1) {
            sibling[0].click();
        }

        //update user session
        if (path.indexOf('/') >= 0) {
            S.ajax.post('Files/Close', { path: path }, callback);
        }
    },

    closeFromPath: function (path) {
        //find any tabs that exist in the path
        var tabs = $('.edit-tabs li > div').filter((i, a) => $(a).attr('data-path') && $(a).attr('data-path').indexOf(path) >= 0);
        if (tabs.length > 0) {
            var tab = $(tabs[0]);
            var id = tab.parent()[0].className.replace('tab-', '');
            var tpath = $(tab).attr('data-path');
            S.editor.tabs.close(id, tpath, () => { S.editor.tabs.closeFromPath(path) });
        }
    }
};