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
                container.find('.check input[type="checkbox"]').on('input', (e) => {
                    var target = $(e.target);
                    if (e.target.checked == true) {
                        target.parents('.check').addClass('checked');
                    } else {
                        target.parents('.check').removeClass('checked');
                    }
                });

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
                        setTimeout(() => {
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
                var container = $('.popup.show');
                popup.find('.resources-content').html(d);
                popup.find('.img .close-btn').each((i, a) => {
                    $(a).attr('onclick', $(a).attr('onclick').replace('this)', 'this, \'' + path + '\')'));
                });
                popup.find('.img').prepend($('#template_resource_selected').html());

                popup.find('.img').on('click', (e) => {
                    //click on image
                    var target = $(e.target);
                    if (!target.hasClass('img')) { target = $(e.target).parents('.img').first(); }
                    console.log($(e.target));
                    if ($(e.target).hasClass('close-btn') ||
                        $(e.target).parents('.close-btn').length > 0 ||
                        $(e.target).parents('.check').length > 0 ||
                        $(e.target).parents('.menu.hover-only').length > 0
                    ) { return; }
                    e.cancelBubble = true;
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

                //checkboxes
                container.find('.check').remove();

                //initialize uploader
                var total = 0;
                var uploader = S.editor.resources.uploader = launchPad({
                    url: '/Upload/Resources',

                    onQueueStart: function () {
                        total = this.queue.length;
                        container.prepend('<div class="progress-bg"><div class="progress"><div class="info"></div><div class="bar"><div class="progress-bar" style="width:0%"><span></span></div></div></div></div>');
                    },

                    onUploadStart: function (files, xhr, data) {
                        data.append('path', path);
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
                        S.popup.hide(popup);
                        setTimeout(() => {
                            S.editor.resources.select(path, filetypes, multiselect, title, buttonTitle, uploadTitle, callback);
                        }, 1000);
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

    delete: function (e, file, elem, path) {
        console.log('delete!');
        e.cancelBubble = true;
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