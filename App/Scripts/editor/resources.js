S.editor.resources = {
    _loaded: false,
        uploader: null,
            path: '',

                load: function (path) {
                    var self = S.editor.resources;
                    var isRoot = false;
                    if (path.indexOf('wwwroot') >= 0) { isRoot = true; }
                    var id = isRoot ? 'resources' : 'page-resources';
                    S.editor.tabs.create(isRoot ? "Resources" : "Page Resources", id + "-section", { isPageResource: !isRoot },
                        () => { //onfocus
                            $('.tab.' + id).removeClass('hide');
                        },
                        () => { //onblur

                        },
                        () => { //onsave

                        }
                    );
                    if (self._loaded == true && self.path == path) { return; }
                    S.editor.resources.path = path;
                    $('.sections > .' + id).html('');
                    S.ajax.post('PageResources/Render', { path: path },
                        function (d) {
                            $('.sections > .' + id).html(d);
                            S.editor.resources._loaded = true;
                            var p = path.replace('content/', '');
                            $('.page-name').attr('href', '/' + p).html(p);

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
                            $('.button.uploader').on('click', S.editor.resources.uploader.click);
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