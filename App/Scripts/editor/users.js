S.editor.users = {
    show: () => {
        S.editor.dropmenu.hide();
        $('.editor .sections > .tab').addClass('hide');
        $('.editor .sections > .users-management').removeClass('hide');
        $('ul.file-tabs > li').removeClass('selected');

        //disable save menu
        $('.item-save').addClass('faded').attr('disabled', 'disabled');
        $('.item-save-as').addClass('faded').attr('disabled', 'disabled');

        //load users list
        S.editor.tabs.create("User Management", "users-management-section", { isPageResource: false },
            () => { //onfocus
                $('.tab.users-management').removeClass('hide');
            },
            () => { //onblur

            },
            () => { //onsave

            }
        );
    }
};