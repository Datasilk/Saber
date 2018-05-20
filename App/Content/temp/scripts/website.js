(function () {
    //log out function
    window.logout = function () {
        ajax('api/User/LogOut', function () {
            window.location.href = '/login';
        });
    }

    //simple ajax method
    function ajax(url, callback) {
        var req = new XMLHttpRequest();
        req.onload = function () {
            if (req.status >= 200 && req.status < 400 && typeof callback == 'function') {
                callback(req.responseText);
            }
        };
        req.open('GET', url);
        req.send();
    }
})();