S.login = {
    submit: function () {
        //save new password for user
        var email = S('#email').val();
        var pass = S('#password').val();
        var msg = S('.login .message');

        //validate email
        if (!S.login.validateEmail(email)) {
            S.editor.error(msg, 'You must provide a valid email address');
            return;
        }

        //disable button
        S('#btnlogin').prop("disabled", "disabled");

        //send new account info to server
        S.ajax.post('User/OAuth', { clientId: clientId, email: email, password: pass },
            function (data) {
                //redirect user to 3rd-party web application
                window.location.href = data;
            }, function () {
                    S.editor.error(msg, 'An error occurred while trying to create your account');
                    S('#btnlogin').prop("disabled", null);
            });
    },

    validateEmail: function (email) {
        return /^([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x22([^\x0d\x22\x5c\x80-\xff]|\x5c[\x00-\x7f])*\x22)(\x2e([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x22([^\x0d\x22\x5c\x80-\xff]|\x5c[\x00-\x7f])*\x22))*\x40([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x5b([^\x0d\x5b-\x5d\x80-\xff]|\x5c[\x00-\x7f])*\x5d)(\x2e([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x5b([^\x0d\x5b-\x5d\x80-\xff]|\x5c[\x00-\x7f])*\x5d))*$/.test(email);
    }
}

//add event listeners
S('.login form').on('submit', function (e) {
    e.preventDefault();
    e.cancelBubble = true;
    S.login.submit();
    return false;
});