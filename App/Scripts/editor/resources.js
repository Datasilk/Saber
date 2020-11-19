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
                    S.editor.tabs.create(isRoot ? 'Resources' : 'Page Resources', id + '-section', { isPageResource: !isRoot },
                        () => { //onfocus
                            $('.tab.' + id).removeClass('hide');
                            console.log('on focus');
                            updateFilebar();
                        },
                        () => { //onblur

                        },
                        () => { //onsave

                        }
                    );

                    function updateFilebar() {
                        S.editor.filebar.update((isRoot ? 'Resources for ' + pagename : 'Page Resources for <a href="/' + pagename + '">/' + pagename + '</a>'), 'icon-photos', $('#page_resources_toolbar').html());
                        console.log($('.tab-toolbar .uploader'));
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
                            console.log(S.editor.resources.uploader);
                            updateFilebar();
                        }
                    );
                },

    delete: function (file, elem) {
        if (!confirm('Do you really want to delete the file "' + file + '"? This cannot be undone.')) { return; }
        S.ajax.post('PageResources/Delete', { path: S.editor.resources.path, file: file },
            function (d) {
                $(elem).parents('li').first().remove();
                S.editor.explorer.dir(S.editor.explorer.path);
            },

            function () { S.editor.error('Could not delete resource on the server.'); }
        );
    }
};