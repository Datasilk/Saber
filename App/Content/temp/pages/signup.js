(function(){
    var byId = (name) => {return document.getElementById(name);};
    var byClass = (name) => {return document.getElementsByClassName(name)[0];};
    var form = byId('signupform');
    var submit = byId('btnsignup');
    form.addEventListener('submit', submitForm);

    function submitForm(e){
        e.preventDefault();
        var data = {
            name: byId('name').value,
            emailaddr: byId('email').value,
            password: byId('password').value,
            password2: byId('password2').value,
        };
        return;

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
                error(req.responseText);
            }
        };

        req.onerror = function () {
            //an error occurred before connecting to server
                error(req.responseText);
        };

        //finally, send AJAX request
        req.open('POST', '/api/User/SignUp');
        req.setRequestHeader('Content-Type', 'text/html');
        req.send(JSON.stringify(data));
    }

    function error(msg){
        var box = byClass('msg');
        var label = byClass('msg-lbl');
        label.innerHTML(msg);
        box.style.display = 'block';
    }


})();