S.editor.resize = function () {
    var editor = S.editor.instance;
    var newHeight = 0;
    switch (S.editor.type) {
        case 0: //monaco

            break;
        case 1: //ace
            newHeight = editor.getSession().getScreenLength() * editor.renderer.lineHeight + editor.renderer.scrollBar.getWidth();
            break;
    }

    if (newHeight < 20) { newHeight = 20; }
    newHeight += 30;
    $('#editor').css({ minHeight: newHeight.toString() + "px" });
    $('#editor-section').css({ minHeight: newHeight.toString() + "px" });


    //resize code editor
    switch (S.editor.type) {
        case 1: //ace
            S.editor.instance.resize();
            break;
    }
    S.editor.resizeWindow();
};

S.editor.resizeWindow = function () {
    var win = S.window.pos();
    var sect = S.editor.sect;
    var div = S.editor.div;
    var browser = S.editor.divBrowser;
    var fields = S.editor.divFields;
    var pos = sect.offset();
    var pos2 = browser.offset();
    div.css({ height: '' });
    if (pos.top == 0) {
        pos = fields.offset();
    }
    $('.editor > div > .sections > .tab').css({ height: win.h - pos.top });
    $('.file-browser').css({ height: win.h - pos2.top });
};