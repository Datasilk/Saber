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
        S.editor.tabs.create(isRoot ? 'Resources' : 'Page Resources', id + '-section', { showPageButtons:true, selected:true },
            () => { //onfocus
                S.editor.tabs.show(id);
                updateFilebar();
                S.editor.filebar.buttons.show(id);
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );

        function updateFilebar() {
            S.editor.filebar.update((isRoot ? 'Resources for ' + pagename : 'Page Resources for <a href="/' + pagename + '">' + pagename + '</a>'), 'icon-photos', $('#page_resources_toolbar').html());
            if (S.editor.resources.uploader != null) {
                $('.tab-toolbar .uploader').on('click', S.editor.resources.uploader.click);
            }
        }
        if (self._loaded == true && self.path == path) {
            S.editor.tabs.select(id + '-section');
            return;
        }
        S.editor.resources.path = path;
        var container = $('.sections > .' + id);
        S.ajax.post('PageResources/Render', { path: path },
            function (d) {
                container.html(d);
                S.editor.resources._loaded = true;
                pagename = path.replace('content/pages/', '');

                //initialize uploader
                var total = 0;
                S.editor.resources.uploader = launchPad({
                    url: '/Upload/Resources',
                    onQueueStart: function () {
                        total = this.queue.length;
                        container.prepend('<div class="progress-bg"><div class="progress"><div class="info"></div><div class="bar"><div class="progress-bar" style="width:0%"><span></span></div></div></div></div>');
                    },

                    onUploadStart: function (files, xhr, data) {
                        data.append('path', S.editor.resources.path);
                    },

                    onUploadProgress: function (e, perc) {
                        var totalLeft = this.queue.length + this.parallelUploads;
                        var percent = parseInt((100 / total) * ((total - totalLeft) + perc)) + '%';
                        container.find('.progress-bg .info').html('Uploading ' + totalLeft + ' file' + (totalLeft > 1 ? 's' : '') + '...');
                        container.find('.progress-bar').css({ 'width': percent });
                        container.find('.progress-bar span').html(percent);
                    },

                    onQueueComplete: function () {
                        S.editor.resources._loaded = false;
                        //$('.sections .' + id).children().remove();
                        setTimeout(() => {
                            //wait before reloading resources explorer to allow files to process on server
                            S.editor.resources.load(path);
                        }, 1000);
                    }
                });
                updateFilebar();
            }
        );
},

    select: function (path, filetypes, multiselect, title, buttonTitle, uploadTitle, callback) {
        //show a popup to select a resource
        let popup = $(S.popup.show(title, $('#template_resources_popup').html()
            .replace('##button-title##', buttonTitle)
            .replace('##media-type##', uploadTitle)
        ));
        popup.css({ 'width': 'calc(100% - 30px)' });
        $(window).on('resize', resizeResources);

        function resizeResources() {
            var win = S.window.pos();
            popup.find('.resources-list').css({ 'max-height': (win.h + (multiselect ? -200 : -90)) + 'px' });
        }
        resizeResources();

        var selectedResources = [];

        S.ajax.post('PageResources/Render', { path: path, filetypes:filetypes },
            function (d) {
                popup.find('.resources-content').html(d);
                popup.find('.img .close-btn').each((i, a) => {
                    $(a).attr('onclick', $(a).attr('onclick').replace('this)', 'this, \'' + path + '\')'));
                });
                popup.find('.img').prepend($('#template_resource_selected').html());
                popup.find('.img').on('click', (e) => {
                    e.cancelBubble = true;
                    var target = $(e.target);
                    if (!target.hasClass('img')) { target = $(e.target).parents('.img').first(); }
                    $(target).find('.selected').toggleClass('hide');
                    selectedResources = popup.find('.resources-list li')
                        .filter((i, a) => $(a).find('.selected:not(.hide)').length > 0)
                        .map((i, a) => $(a).find('.title').html().trim());
                    if (!multiselect) {
                        //single-select
                        popup.find('.apply')[0].click();
                    }
                })
                if (!multiselect) {
                    //single select
                    popup.find('.apply').hide();
                    popup.find('.img').css({ 'cursor': 'pointer' });
                }
                resizeResources();

                //initialize uploader
                var uploader = S.editor.resources.uploader = launchPad({
                    url: '/Upload/Resources',
                    onUploadStart: function (files, xhr, data) {
                        data.append('path', path);
                    },

                    onQueueComplete: function () {
                        S.popup.hide(popup);
                        S.editor.resources.select(path, filetypes, multiselect, title, buttonTitle, uploadTitle, callback);
                    }
                });
                popup.find('.uploader').on('click', uploader.click);
                popup.find('.apply').on('click', (e) => {
                    S.popup.hide(popup);
                    if (callback) { callback(selectedResources) }
                });
                S.popup.resize();
            }
        );
    },

    delete: function (file, elem, path) {
        if (!window.parent.confirm('Do you really want to delete the file "' + file + '"? This cannot be undone.')) { return; }
        S.ajax.post('PageResources/Delete', { path: path ?? S.editor.resources.path, file: file },
            function (d) {
                $(elem).parents('li').first().remove();
                S.editor.explorer.dir(S.editor.explorer.path);
            },

            function () { S.editor.error('Could not delete resource on the server.'); }
        );
    }
};