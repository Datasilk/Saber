S.editor.message = function (elem, msg, type) {
    $(elem ? elem : '.editor > div > .messages').append(template_message.innerHTML
        .replace('##text##', msg)
        .replace('##type##', type)
    );
    $('.message .close-btn').off('click').on('click', (e) => {
        $(e.target).parents('.message').first().remove();
    });
};