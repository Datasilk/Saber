S.editor.changed = function (checkall) {
    var self = S.editor;
    var id = self.fileId(self.selected);
    var tab = $('.tab-' + id);
    if (tab.length == 1) {
        if (tab.attr('data-edited') != "true" || checkall == true) {
            //enable save menu
            $('.item-save').removeClass('faded').removeAttr('disabled');

            if (tab.attr('data-edited') != "true") {
                //update tab with *
                tab.attr('data-edited', "true");
                var col = tab.find('.col');
                col.html(col.html() + ' *');
            }
        }
    }
    S.editor.codebar.update();
    self.resize();
};

S.editor.isChanged = function (path) {
    var self = S.editor;
    var id = self.fileId(path);
    var tab = $('.tab-' + id);
    if (tab.length == 1) {
        if (tab.attr('data-edited') == "true") {
            return true;
        }
    }
    return false;
};

S.editor.unChanged = function (path) {
    var self = S.editor;
    var id = self.fileId(path);
    var tab = $('.tab-' + id);
    if (tab.length == 1) {
        if (tab.attr('data-edited') == "true") {
            tab.removeAttr('data-edited');
            $('.item-save').addClass('faded').attr('disabled', 'disabled');
            var col = tab.find('.col');
            col.html(col.html().replace(' *', ''));
        }
    }
};

S.editor.files = {
    //required page files, track whether or not they've 
    //been updated on the server via the editor
    html: { changed: false },
    less: { changed: false },
    js: { changed: false },
    website: {
        css: { changed: false },
        js: { changed: false }
    },
    content: { changed: false },
    partials: {} // e.g. { "header.html":false, "footer.html":true }
};