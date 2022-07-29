(function () {
    var byId = (name) => { return document.getElementById(name); };
    var byClass = (name) => { return document.getElementsByClassName(name)[0]; };
    var form = byId('signupform');

    form.addEventListener('submit', function (e) {
        e.preventDefault();
        submitForm();
        return false;
    });

    function submitForm() { 
        var data = {
            name: byId('name').value,
            email: byId('email').value,
            password: byId('password').value,
            password2: byId('password2').value
        };

        //validate name
        if (data.name == '' || data.name == null) {
            error('You must provide your name');
            return;
        }

        //validate email
        if (!validateEmail(data.email)) {
            error('You must provide a valid email address');
            return;
        }

        //validate password
        if (data.password == '' || data.password2 == '') {
            error('You must type in your password twice');
            return;
        }
        if (data.password != data.password2) {
            error('Your passwords do not match');
            return;
        }
        if (data.password.length < 8) {
            error('Your password must be at least 8 characters long');
            return;
        }

        //set up AJAX request
        var req = new XMLHttpRequest();

        //set up callbacks
        req.onload = function () {
            if (req.status >= 200 && req.status < 400) {
                //request success
                document.location.href = '/login';
            } else {
                //connected to server, but returned an error
                error('Error occurred on server');
            }
        };

        req.onerror = function () {
            //an error occurred before connecting to server
            error('An error occurred when contacting the server');
        };

        //finally, send AJAX request
        req.open('POST', '/api/User/CreateAdminAccount');
        req.setRequestHeader('Content-Type', 'text/html');
        req.send(JSON.stringify(data));
    }

    function error(msg) {
        var box = byClass('message');
        box.innerHTML = msg;
        box.style.display = 'block';
    }

    function validateEmail(email) {
        return /^([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x22([^\x0d\x22\x5c\x80-\xff]|\x5c[\x00-\x7f])*\x22)(\x2e([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x22([^\x0d\x22\x5c\x80-\xff]|\x5c[\x00-\x7f])*\x22))*\x40([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x5b([^\x0d\x5b-\x5d\x80-\xff]|\x5c[\x00-\x7f])*\x5d)(\x2e([^\x00-\x20\x22\x28\x29\x2c\x2e\x3a-\x3c\x3e\x40\x5b-\x5d\x7f-\xff]+|\x5b([^\x0d\x5b-\x5d\x80-\xff]|\x5c[\x00-\x7f])*\x5d))*$/.test(email);
    }
})();