S.editor.error = function (elem, msg) {
    console.log(elem);
    console.log(msg);
    S.editor.message(elem, msg || S.message.error.generic, 'error');
};