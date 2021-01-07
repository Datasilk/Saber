S.editor.resources = {
    _loaded: false,
    uploader: null,
    path: '',

    load: function (path) {
        var self = S.editor.resources;
        var isRoot = false;
        var pagename = '';
        if (path.indexOf('wwwroot') >= 0) { isRoot = true; }
        var id = isRoot ? 'resources' : 'page-resources';
        S.editor.tabs.create(isRoot ? 'Resources' : 'Page Resources', id + '-section', { selected:true },
            () => { //onfocus
                $('.tab.' + id).removeClass('hide');
                updateFilebar();
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );

        function updateFilebar() {
            S.editor.filebar.update((isRoot ? 'Resources for ' + pagename : 'Page Resources for <a href="/' + pagename + '">/' + pagename + '</a>'), 'icon-photos', $('#page_resources_toolbar').html());
            if (S.editor.resources.uploader != null) {
                $('.tab-toolbar .uploader').on('click', S.editor.resources.uploader.click);
            }
        }
        if (self._loaded == true && self.path == path) {
            S.editor.tabs.select(id + '-section');
            return;
        }
        S.editor.resources.path = path;
        $('.sections > .' + id).html('');
        S.ajax.post('PageResources/Render', { path: path },
            function (d) {
                $('.sections > .' + id).html(d);
                S.editor.resources._loaded = true;
                pagename = path.replace('content/', '');

                //initialize uploader
                if (self.uploader == null) {
                    S.editor.resources.uploader = launchPad({
                        url: '/Upload/Resources',
                        onUploadStart: function (files, xhr, data) {
                            data.append('path', S.editor.resources.path);
                        },

                        onQueueComplete: function () {
                            S.editor.resources._loaded = false;
                            $('.sections .' + id).children().remove();
                            S.editor.resources.load(S.editor.resources.path);
                            S.editor.explorer.dir(S.editor.explorer.path);
                        }
                    });
                }
                updateFilebar();
            }
        );
},

    select: function (path, filetypes, multiselect, title, buttonTitle, callback) {
        //show a popup to select a resource
        let popup = $(S.popup.show(title, $('#template_resources_popup').html().replace('##button-title##', buttonTitle)));
        popup.css({ 'width': 'calc(100% - 30px)' });
        $(window).on('resize', resizeResources);

        function resizeResources() {
            var win = S.window.pos();
            popup.find('.resources-list').css({ 'max-height': (win.h - 200) + 'px' });
        }
        resizeResources();

        var selectedResources = [];

        S.ajax.post('PageResources/Render', { path: path, filetypes:filetypes },
            function (d) {
                popup.find('.resources-list').html(d);
                popup.find('.img .close-btn').each(a => {
                    $(a).attr('onclick', $(a).attr('onclick').replace('this)', 'this, \'' + path + '\')'));
                });
                popup.find('.img').prepend($('#template_resource_selected').html());
                popup.find('.img').on('click', (e) => {
                    var target = e.target;
                    if (target.tagName.toLowerCase() == 'img') { target = $(e.target).parents('.img')[0]; }
                    if (target.className.indexOf('img') >= 0 || target.tagName.toLowerCase() == 'img') {
                        e.cancelBubble = true;
                        $(target).find('.selected').toggleClass('hide');
                        selectedResources = popup.find('.resources-list li')
                            .filter((i, a) => $(a).find('.selected:not(.hide)').length > 0)
                            .map((i, a) => $(a).find('.title').html().trim());
                    }
                })
                resizeResources();

                //initialize uploader
                var uploader = launchPad({
                    url: '/Upload/Resources',
                    onUploadStart: function (files, xhr, data) {
                        data.append('path', path);
                    },

                    onQueueComplete: function () {
                        S.popup.hide(popup);
                        S.editor.resources.select(path, filetypes, multiselect, title, buttonTitle, callback);
                    }
                });
                popup.find('.uploader').on('click', uploader.click);
                popup.find('.apply').on('click', (e) => {
                    S.popup.hide(e);
                    if (callback) { callback(selectedResources) }
                });
                S.popup.resize();
            }
        );
    },

    delete: function (file, elem, path) {
        if (!confirm('Do you really want to delete the file "' + file + '"? This cannot be undone.')) { return; }
        S.ajax.post('PageResources/Delete', { path: path ?? S.editor.resources.path, file: file },
            function (d) {
                $(elem).parents('li').first().remove();
                S.editor.explorer.dir(S.editor.explorer.path);
            },

            function () { S.editor.error('Could not delete resource on the server.'); }
        );
    }
};