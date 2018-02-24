var Tapestry = {
    on: function(target, type, listener){
        if(target.length > 1){
            
            for (var i = 0; i < target.length; i++) {
                Tapestry._on(target[i], type, listener);
            }
        }else{
            Tapestry._on(target, type, listener);
        }
    },

    _on: function(target, type, listener){
        if ("addEventListener" in window) {
            target.addEventListener(type, listener, false);
        }
        else {
            target.attachEvent("on" + type, function(){
                listener(window.event);
            });
        }
    },

    hasClass: function(target, className){
        var classes = target.className.split(' ');
        for(var i = 0; i < classes.length; i++){
            if(classes[i] == className){return true;}
        }
        return false;
    },

    addClass: function(target, className){
        if(!target){return;}
        if(!className){return;}
        if(!target.className){return;}
        var classes = target.className;
        classes += (classes != '' ? ' ' : '') + className;
        target.className = classes.replace(new RegExp('  '), ' ');
    },

    removeClass: function(target, className){
        if(!target){return;}
        if(!className){return;}
        if(!target.className){return;}
        var classes = target.className.replace(new RegExp(className), '').replace(new RegExp('  '), ' ');

        target.className = classes;
    },

    getChildrenByTagName: function(target, tagname){
        var elem = target.firstChild;
        var children = [];
        while (elem != null) {
            if(elem.tagName){
                if (elem.tagName.toLowerCase() == tagname.toLowerCase()) {
                    children.push(elem);
                }
            }
            
            elem = elem.nextSibling;
        }
        return children;
    }
};

Tapestry.menu = {
    _clickTimer:null,

    click: function(){
        var p = this;
        if(Tapestry.menu._clickTimer != null){return false;}
        Tapestry.menu._clickTimer = setTimeout(function(){Tapestry.menu._clickTimer = null;}, 100);
        if(p){
            var submenu = p.querySelector('ul.menu');
            if(submenu){
                if(Tapestry.hasClass(submenu, 'expanded') == true){
                    Tapestry.menu.collapse(submenu);
                    return false;
                }
            }
        }
        Tapestry.menu.expand(submenu);
        return false;
    },

    expand: function(submenu){
        Tapestry.addClass(submenu, 'expanded');
    },

    collapse: function(submenu){
        Tapestry.removeClass(submenu, 'expanded');
    }
};