S.editor.message = function (elem, msg, type) {
    S(elem && elem != '' ? elem : '.editor > div > .messages').append(template_message.innerHTML
        .replace('##text##', msg)
        .replace('##type##', type ?? '')
    );
    S('.message .close-btn').off('click').on('click', (e) => {
        S(e.target).parents('.message').first().remove();
    });
    if (elem) { S(elem).removeClass('hide'); }
};