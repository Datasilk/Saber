(function(){
    var byId = (name) => {return document.getElementById(name);};
    var byClass = (name) => {return document.getElementsByClassName(name)[0];};
    var key = location.hash.replace('#', '');
    location.hash = '';
    if(key != ''){
        byClass('form-valid').style.display = 'block';
    }else{
        byClass('form-invalid').style.display = 'block';
        return;
    }
    var form = byId('resetform');
    var submit = byId('resetpass');
    form.addEventListener('submit', function(e) {
        submitForm();
        e.preventDefault();
        return false;
    });

    function submitForm(){
        var data = {
            key: key,
            password: byId('password').value,
            password2: byId('password2').value
        };

        //set up AJAX request
        var req = new XMLHttpRequest();

        //set up callbacks
        req.onload = function () {
            if (req.status >= 200 && req.status < 400) {
                //request success
                document.location.href = '/resetpass-complete'
            } else {
                //connected to server, but returned an error
                error(req.responseText);
            }
        };

        req.onerror = function () {
            //an error occurred before connecting to server
            error('An error occurred when contacting the server');
        };

        //finally, send AJAX request
        req.open('POST', '/api/User/ResetPassword');
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