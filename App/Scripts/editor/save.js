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
        else if ($('.tab-page-settings').hasClass('selected')) {
            //save page settings ///////////////////////////////////////////////////////////////////////////////
            var settings = self.settings;

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
            } else if(path.indexOf('/partials/' >= 0)) {
                //check if file is a partial and if partial content fields tab is loaded
                //var fieldstab = $('.tab-' + self.fileId(path.replace('content/partials/', 'content-fields-')));
                //if (fieldstab.length > 0) {
                //    S.editor.fields.load(path, false);
                //}
                S.editor.files.content.changed = true;
            }
            tab.find('.loader').remove();
            self.unChanged(path);
            //S.editor.explorer.open(path);
        },
        function (d) {
            console.log(d);
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