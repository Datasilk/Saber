S.dashboard = {
    website: {
        id: 0,
        title: '',
        host:''
    },

    sections: {
        load: function(url, name){
            if ($('.dash-body .section-' + name).length > 0) {
                S.dashboard.sections.show(name);
                if (url != null) {
                    S.url.push(document.title, url.substr(1));
                }
                S.menu.select('.dash-menu ul.menu');
                return false;
            }
            return true;
        },

        show: function (name) {
            $('.dash-body .dash-section').hide();
            $('.dash-body .section-' + name).show();
        }
    },

    menu: {
        validate: function (url, name) {
            //check to see if dashboard section is already loaded
            return S.dashboard.sections.load(url, name);
        }
    }
}

//add event to listen for menu click & page url change
S.menu.addListener('dash-menu', '.dash-menu ul.menu');
S.menu.select('.dash-menu ul.menu');

//add event for url change
S.events.url.callback.add('dash-menu-url', null, function (e) {
    if (e) {
        var name = e.state.replace('dashboard/', '').split('/', 2).join('-');
        return S.dashboard.sections.load(null, name);
    }
});