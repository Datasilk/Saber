﻿S.editor.market = {
    canInstall: false,
    open: function () {
        //show market tab
        var host = 'https://www.sabercms.com/'; //S.editor.env == 'prod' ? 'https://www.sabercms.com/' : 'http://localhost:7071/';
        $('.editor .sections > .tab').addClass('hide');

        if ($('.tab.market-section').length == 0) {
            //create new content section
            $('.sections').append('<div class="tab market market-section">' +
                '<iframe src="' + host + '" style="width:100%; height:100%; overflow-y:auto; overflow-x:hidden;" scrolling="yes"></iframe></div>');

            S.editor.resize.window();

            //set up CORS commands
            window.market = {
                userId: null,
                username: null,
                load: function (userId, username) {
                    window.market.userId = userId;
                    window.market.username = username;
                    focusTab();
                },
                template: {
                    install: function (id, token) {
                        if (S.editor.market.canInstall == true && window.parent.confirm('Do you really want to install this website template onto your Saber website? This will completely erase your existing website and replace it with the template, and this cannot be undone!')) {
                            S.popup.show('Installing Website Template', '<div class="pad-lg">Please wait...', { close: false });
                            S.ajax.post('Marketplace/InstallTemplate', { templateId: id, token: token }, () => {
                                S.popup.hide();
                                S.popup.show('Website Template Installed!', '<div class="pad-lg">The selected website template was successfully installed. Please clear your web browser cache and refresh this page to view your new website!</div>', { close: false });
                            });
                        }
                    }
                }
            }

            var toolbarHtml = '';

            //set up postMessage listener
            window.addEventListener("message", (e) => {
                if (host.indexOf(e.origin) == 0 && e.data) {
                    if (e.data.indexOf('marketplace|') == 0) {
                        var data = JSON.parse(e.data.replace('marketplace|', ''));
                        window.market.load(data.userId, data.username);
                        S.ajax.post('Marketplace/Permissions', {}, (response) => {
                            var iframe = $('.tab.market.market-section iframe')[0].contentWindow;
                            S.editor.market.canInstall = response.indexOf('canInstall') >= 0;
                            iframe.postMessage('saber|' + response, '*');
                        });
                    } else if (e.data.indexOf('install-template|') == 0) {
                        var props = e.data.replace('install-template|', '').split(',');
                        window.market.template.install(props[0], props[1]);
                    }
                }
            }, false);

            function focusTab() {
                //select tab & generate toolbar
                $('.tab.market-section').removeClass('hide');
                if (toolbarHtml == '') {
                    S.ajax.post('Marketplace/Toolbar', {}, (response) => {
                        toolbarHtml = response;
                        renderTab();
                    });
                } else {
                    renderTab();
                }
            }

            function renderTab() {
                S.editor.filebar.update('Marketplace', 'icon-market', toolbarHtml);
                var usermenu = $('.toolbar.for-tabs .user-menu');
                var nousermenu = $('.toolbar.for-tabs .nouser-menu');
                if (window.market.userId != null) {
                    usermenu.show();
                    nousermenu.hide();
                } else {
                    usermenu.hide();
                    nousermenu.show();
                }

                //login button
                $('.nouser-menu .tab-login').on('click', () => {
                    $('.tab.market-section iframe').attr('src', host + 'login');
                });

                //upload button
                $('.user-menu .tab-upload').on('click', () => {
                    $('.tab.market-section iframe').attr('src', host + 'dashboard/upload');
                });

                //dashboard button
                $('.user-menu .tab-dashboard').on('click', () => {
                    $('.tab.market-section iframe').attr('src', host + 'dashboard');
                });

                //settings button
                $('.user-menu .tab-settings').on('click', () => {
                    $('.tab.market-section iframe').attr('src', host + 'dashboard/profile');
                });

                //homepage button
                $('.user-menu .tab-home').on('click', () => {
                    $('.tab.market-section iframe').attr('src', host);
                });
            }

            //create tab
            S.editor.tabs.create('Marketplace', 'market-section', { removeOnClose: true },
                () => { //onfocus
                    focusTab();
                },
                () => { //onblur 
                },
                () => { //onsave 
                }
            );
        } else {
            $('.tab.market-section').removeClass('hide');
            S.editor.tabs.select('market-section');
        }
    }
};

S.editor.dropmenu.add('.menu-bar .menu-item-website > .drop-menu .menu.for-plugins', 'market', 'Marketplace', '#icon-market', false, S.editor.market.open);