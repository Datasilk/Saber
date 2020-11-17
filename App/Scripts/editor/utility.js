S.editor.fileId = function (path) {
    if (path == null) { path = 'content' + window.location.pathname.toLowerCase(); }
    return path.replace(/\//g, '_').replace(/\./g, '_');
};

S.editor.fileExt = function (path) {
    var paths = path.split('/');
    var file = paths[paths.length - 1];
    var fileparts = paths[paths.length - 1].split('.', 2);
    if (fileparts.length > 1) { return fileparts[fileparts.length - 1]; }
    return '';
};

S.editor.decodeHtml = function(html) {
    var txt = document.createElement("textarea");
    txt.innerHTML = html;
    return txt.value;
};

S.editor.isResource = function (path, type) {
    var paths = path.toLowerCase().split('/');
    var file = paths[paths.length - 1];
    var dir = paths.join('/').replace(file, '');
    var fileparts = paths[paths.length - 1].split('.', 2);
    var relpath = dir + fileparts[0];
    if (typeof type == 'undefined') {
        return relpath == S.editor.path;
    } else if (type == 'website.css') {
        switch (dir + fileparts.join('.')) {
            case 'content/partials/header.less':
            case 'content/partials/footer.less':
            case 'root/css/website.less':
                return true;
        }
    } else if (type == 'website.js') {
        switch (dir + fileparts.join('.')) {
            case 'root/scripts/website.js':
                return true;
        }
    } else if (type == 'partial') {
        switch (dir + fileparts.join('.')) {
            case 'content/partials/header.html':
            case 'content/partials/footer.html':
                return true;
        }
    }
};