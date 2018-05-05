S.login = {
    submit: function () {
        var data = {
            email: $('#email').val(),
            password: $('#password').val()
        }
        var msg = $('.login .message');

        S.ajax.post('User/Authenticate', data, function (d) {
            if (d) {
                var msg = $('.login .message');
                if (d == 'err') {
                    S.message.show(msg, 'error', 'Your credentials are incorrect');
                } else if (d.indexOf('success') == 0) {
                    S.message.show(msg, '', 'Login success! Redirecting...');
                    window.location.href = d.split('|')[1];
                }
            }
        }, function (err) {
            S.message.show(msg, 'error', 'Your credentials are incorrect');
        });
    }
};
$('.login form').on('submit', function (e) { S.login.submit(); e.preventDefault(); return false; });