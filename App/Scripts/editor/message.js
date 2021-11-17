S.editor.message = function (elem, msg, type) {
    if (!elem || elem == '') { elem = '.editor .toolbar > .messages'; }
    var container = $(elem);
    var div = document.createElement('div');
    div.className = 'message' + (type != null ? ' ' + type : '');
    div.innerHTML = template_message.innerHTML.replace('##text##', msg);
    container.removeClass('hide').append(div);
    $(div).find('.close-btn').on('click', (e) => {
        $(div).remove();
        if (container.children().length == 0) {
            container.addClass('hide');
        }
    });
};

S.editor.message.confirm = function (title, msg, options, callback) {
    var opts = {
        type:options.type ? options.type : null,
        okay: options.okay ? options.okay : 'Okay',
        yes: options.yes ? options.yes : 'Yes', 
        no: options.no ? options.no : 'No',
        cancel: options.cancel ? options.cancel : 'Cancel'
    };
    var popup = S.popup.show(title,
        $('#template_confirm').html()
        .replace('##msg##', msg)
            .replace('##buttons##', opts.type == null || opts.type == 'okay' ?
            '<div class="col"><button class="button okay" type="submit">' + opts.okay + '</button></div>' :
            opts.type == 'yesno' || opts.type == 'bool' ?
            '<div class="col pad-right-sm"><button class="button okay" type="submit">' + opts.yes + '</button></div>' +
            '<div class="col pad-left-sm"><button class="button cancel">' + opts.no + '</button></div>' :
            opts.type == 'okaycancel' ?
            '<div class="col pad-right-sm"><button class="button okay" type="submit">' + opts.okay + '</button></div>' +
            '<div class="col pad-left-sm"><button class="button cancel">' + opts.cancel + '</button></div>' : ''
        )
    );
    popup.find('button.okay').on('click', (e) => { callback(true); S.popup.hide(popup);});
    popup.find('.button.cancel').on('click', (e) => { callback(false); S.popup.hide(popup);});
}