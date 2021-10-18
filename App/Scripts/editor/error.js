S.editor.error = function (elem, msg) {
    if (!elem || elem == '') { elem = '.editor .toolbar > .messages'; }
    S.editor.message(elem, msg || S.message.error.generic, 'error');
};