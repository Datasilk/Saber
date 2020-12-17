S.editor.codebar = {
    update: function () {
        var editor = S.editor.instance;
        if (typeof editor == 'undefined' || editor == null) { return; }
        var linenum = 'Ln ';
        var charnum = 'Col ';
        var linestotal = 'End ';
        switch (S.editor.type) {
            case 0: //monaco
                var pos = editor.getPosition();
                var model = editor.getModel();
                linenum += pos.lineNumber.toString();
                charnum += pos.column.toString();
                linestotal += model.getLineCount();
                break;
            case 1: //ace

                break;
        }
        S('.code-curr-line').html(linenum);
        S('.code-curr-char').html(charnum);
        S('.code-total-lines').html(linestotal);
    },
    status: function (msg) {
        S('.code-status').html(msg);
    }
};