S.login = {
    submit: function () {
        var data = {
            email: $('#email').val(),
            password: $('#password').val()
        }
        var msg = $('.login .message');

        S.ajax.post('User/Authenticate', data, function (d) {
            if (d) {
                S.message.show(msg, '', 'Login success! Redirecting...');
                window.location.href = d;
            }
        }, function (err) {
            S.message.show(msg, 'error', 'Your credentials are incorrect');
        });
    }
};
$('.login form').on('submit', function (e) { S.login.submit(); e.preventDefault(); return false; });