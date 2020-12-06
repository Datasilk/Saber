(function(){
    var byId = (name) => {return document.getElementById(name);};
    var byClass = (name) => {return document.getElementsByClassName(name)[0];};
    var form = byId('forgotform');
    var submit = byId('forgot');
    form.addEventListener('submit', function(e) {
        submitForm();
        e.preventDefault();
        return false;
    });

    function submitForm(){
        var data = {
            emailaddr: byId('email').value
        };

        //set up AJAX request
        var req = new XMLHttpRequest();

        //set up callbacks
        req.onload = function () {
            if (req.status >= 200 && req.status < 400) {
                //request success
                document.location.href = '/forgotpass-complete'
            } else {
                //connected to server, but returned an error
                error('Unknown error occurred');
            }
        };

        req.onerror = function () {
            //an error occurred before connecting to server
            error('An error occurred when contacting the server');
        };

        //finally, send AJAX request
        req.open('POST', '/api/User/ForgotPassword');
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