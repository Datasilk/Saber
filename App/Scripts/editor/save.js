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
    var ext = S.editor.fileExt(path);
    var tab = $('.tab-' + id);
    self.dropmenu.hide();
    if (tab.length > 0 && tab.hasClass('selected')) {
        //check if we should save something besides source code

        if ($('.tab-for-content-fields').hasClass('selected')) {
            //save content fields values ///////////////////////////////////////////////////////////////////////////////
            console.log('save fields');
            S.editor.fields.save();
            return;

        }
        else if ($('.tab-page-settings').hasClass('selected')) {
            //save page settings ///////////////////////////////////////////////////////////////////////////////
            var settings = S.editor.settings;

            //save title
            if (settings.title.changed == true) {
                settings.title.save(showmsg);
                return;
            }

            //save description
            if (settings.description.changed == true) {
                settings.description.save(showmsg);
                return;
            }

            //save header & footer with fields
            if (settings.partials.changed == true) {
                settings.partials.save(showmsg);
                return;
            }

            function showmsg() {
                S.message.show('.page-settings .message', 'confirm', 'Page settings have been updated successfully');
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
            if (S.editor.isResource(path) || S.editor.isResource(path, 'partial')) {
                S.editor.files[ext].changed = true;
                if (ext == 'html') {
                    //also mark content as changed so content fields can reload
                    S.editor.files.content.changed = true;
                }
            } else if (S.editor.isResource(path, 'website.css')) {
                S.editor.files.website.css.changed = true;
            } else if (S.editor.isResource(path, 'website.js')) {
                S.editor.files.website.js.changed = true;
                S.editor.files.js.changed = true;
            }
            tab.find('.loader').remove();
            self.unChanged(path);
            S.editor.explorer.open(path);
        },
        function () {
            S.editor.error();
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