S.editor.dropmenu = {
    show: function (e, container) {
        if (!container) { container = 'li'; }
        $(e.target).parents(container).first().find('.drop-menu').removeClass('hide');
        $(document.body).off('click').on('click', S.editor.dropmenu.hide);
    },

    hide: function (e) {
        var hide = false;
        if (e) {
            if ($(e.target).parents('.drop-menu').length == 0) {
                hide = true;
            }
        } else { hide = true; }
        if (hide == true) {
            $('.drop-menu').addClass('hide');
            $(document.body).off('click', S.editor.dropmenu.hide);
        }
    },

    add: function (ul, id, label, icon, separator, onclick, li_class) {
        //ul = css selector for the ul element that contains the menu items (e.g. .menu-item-website)
        //icon can be either a file or the name of an SVG object (e.g. #icon-analytics)
        //separator = if True, then put a separator above the menu item
        var html = $('#template_menuitem').html()
            .replace('##id##', id)
            .replace('##label##', label)
            .replace('##li-class##', li_class ? ' class="' + li_class + '"' : '')
            .replace('##icon##', icon && icon != '' ? (
                    icon.indexOf('.png') > 0 ? '<img src="' + icon + '"/>' :
                    $('#template_svgicon').html().replace('##icon##', icon)
                ) : '')
            .replace('##separator##', separator == true ?
                $('#template_separator').html()
                .replace('##li-class##', li_class ? ' class="' + li_class + '"' : '')
            : '');
        $(ul).append(html);
        if (onclick) {
            var c = $(ul).children();
            $(c[c.length - 1]).on('click', (e) => {
                onclick(e);
                $('.menu-bar .drop-menu').addClass('hide');
                $(document.body).off('click', S.editor.dropmenu.hide);
            });
        }
    },

    hover: function (e, menubar) {
        if ($(menubar + ' .drop-menu:not(.hide)').length == 0) { return; }
        var target = $(e.target);
        if (e.target.tagName.toLowerCase() != 'li') {
            target = target.parents('li').first();
        }
        var menu = target.find('.drop-menu');
        if (menu.hasClass('hide')) {
            $(menubar + ' .drop-menu').addClass('hide');
            menu.removeClass('hide');
        }
    }
};

S.editor.topmenu = {
    add: function (id, label) {
        //ul = css selector for the ul element that contains the menu items (e.g. .menu-item-website)
        //icon can be either a file or the name of an SVG object (e.g. #icon-analytics)
        //separator = if True, then put a separator above the menu item
        var html = $('#template_topmenuitem').html()
            .replace('##id##', id)
            .replace('##label##', label);
        $('.top-menu ul.menu-bar').append(html);
    },

    hover: function (e) {
        if ($('.top-menu ul.menu-bar .drop-menu:not(.hide)').length == 0) { return; }
        var target = $(e.target);
        if (e.target.tagName.toLowerCase() != 'li') {
            target = target.parents('li').first();
        }
        var menu = target.find('.drop-menu');
        if (menu.hasClass('hide')) {
            $('ul.menu-bar .drop-menu').addClass('hide');
            menu.removeClass('hide');
        }
    }
}