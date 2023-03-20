S.editor.save = function (path, content) {
    if (path == null || typeof path != 'string') {
        switch (S.editor.type) {
            case 0: case 1: //monaco & ace (apparently share the same code)
                S.editor.save(S.editor.selected, S.editor.instance.getValue());
                return;
        }
    }
    var self = S.editor;
    var id = self.fileId(path);
    var ext = self.fileExt(path);
    var tab = $('.tab-' + id);
    self.dropmenu.hide();
    if (tab.length > 0 && tab.hasClass('selected')) {
        //check if we should save something besides source code

        if ($('.tab-for-content-fields').hasClass('selected')) {
            //save content fields values ///////////////////////////////////////////////////////////////////////////////
            self.fields.save();
            return;

        }
        else if ($('.tab-page-settings-section').hasClass('selected')) {
            //save page settings ///////////////////////////////////////////////////////////////////////////////
            var settings = self.pagesettings;
            var saved = false;


            function saveDescription() {
                //then, save description
                if (settings.description.changed == true) {
                    saved = true;
                    settings.description.save(savePartials);
                } else {
                    savePartials();
                }
            }

            function savePartials() {
                //finally, save header & footer with fields
                if (settings.partials.changed == true) {
                    saved = true;
                    settings.partials.save(showmsg);
                } else if (saved == true) {
                    showmsg();
                }
            }

            //first, save title
            if (settings.title.changed == true) {
                saved = true;
                settings.title.save(saveDescription);
            } else {
                saveDescription();
            }

            function showmsg() {
                //last, show message
                S.message.show('.page-settings .messages', 'confirm', 'Page settings have been updated successfully');
            }

            return;
        }
    }

    //last resort, save source code to file ////////////////////////////////////////////////////////////
    if ($('.editor-drop-menu .item-save.faded').length == 1) { return; }

    //show loading progress animation
    tab.find('.tab-title').prepend(S.loader());
    S.ajax.post('Files/SaveFile', { path: path, content: content },
        function (d) {
            //check whether or not file was a required page resource for this page
            if (self.isResource(path)) {
                S.editor.files[ext].changed = true;
                if (ext == 'html') {
                    //also mark content as changed so content fields can reload
                    S.editor.files.content.changed = true;
                }
            } else if (self.isResource(path, 'website.css')) {
                S.editor.files.website.css.changed = true;
            } else if (self.isResource(path, 'website.js')) {
                S.editor.files.website.js.changed = true;
                S.editor.files.js.changed = true;
            } else if(path.indexOf('partials/' >= 0)) {
                //check if file is a partial and if partial content fields tab is loaded
                var fields_tab = $('.tab-' + self.fileId(path.replace('content/partials/', 'content-fields-')));
                if (fields_tab.length > 0) {
                    S.editor.fields.load(path, false);
                }
                S.editor.files.content.changed = true;
            }

            //check if file is a stylesheet
            if (path.indexOf('.css') > 0 || path.indexOf('.less') > 0) {
                var newpath = '/' + path.replace('.less', '.css');
                if (S.editor.pageStylesheets.indexOf(newpath) >= 0) {
                    S.editor.files.pagecss.push(newpath);
                }
            }

            //check if file is a script
            if (path.indexOf('.js') > 0) {
                var newpath = '/' + path;
                if (S.editor.pageScripts.indexOf(newpath) >= 0) {
                    S.editor.files.pagescripts.push(newpath);
                }
            }
            tab.find('.loader').remove();
            self.unChanged(path);
        },
        function (d) {
            S.editor.error('', d.responseText);
            tab.find('.loader').remove();
        }
    );
};

S.editor.saveAs = function () {
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
};

S.editor.save.enable = function () {
    $('.editor .item-save').removeClass('faded').removeAttr('disabled');
}