(function(){
    var byId = (name) => {return document.getElementById(name);};
    var byClass = (name) => {return document.getElementsByClassName(name)[0];};
    var form = byId('loginform');
    var submit = byId('login');
    form.addEventListener('submit', function(e) {
        submitForm();
        e.preventDefault();
        return false;
    });

    function submitForm(){
        var data = {
            email: byId('email').value,
            password: byId('password').value
        };

        //set up AJAX request
        var req = new XMLHttpRequest();

        //set up callbacks
        req.onload = function () {
            if (req.status >= 200 && req.status < 400) {
                //request success
                response = JSON.parse(req.responseText);
                document.location.href = '/' + response.redirect;
            } else {
                //connected to server, but returned an error
                error('Incorrect email and/or password');
            }
        };

        req.onerror = function () {
            //an error occurred before connecting to server
            error('An error occurred when contacting the server');
        };

        //finally, send AJAX request
        req.open('POST', '/api/User/Authenticate');
        req.setRequestHeader('Content-Type', 'text/html');
        req.send(JSON.stringify(data));
    }

    function error(msg){
        var box = byClass('msg');
        box.className = 'msg error';
        box.innerHTML = msg;
        box.style.display = 'block';
    }


})();