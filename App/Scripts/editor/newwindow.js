S.editor.newWindow = function () {
    S.editor.dropmenu.hide();
    if (S.editor.Rhino) {
        S.editor.Rhino.newwindow();
    } else {
        var id = S.editor.fileId();
        window.open(window.location.href.split('Editor?')[0] + S.editor.path.replace('content/', ''), 'Editor_' + id, 'width=1800,height=900,left=50,top=50,toolbar=No,location=No,scrollbars=auto,status=No,resizable=yes,fullscreen=No');
    }
};