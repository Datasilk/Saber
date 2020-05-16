// selector.js - micro library as a jQuery replacement
// https://github.com/Websilk/Selector
// copyright 2017 by Mark Entingh

(function () {

    //global variables
    const tru = true;
    const fals = false;
    const doc = document;
    const pxStyles = ['top', 'right', 'bottom', 'left', 'width', 'height', 'minWidth', 'minHeight', 'maxWidth', 'maxHeight', 'fontSize'],
          pxStylesPrefix = ['border', 'padding', 'margin'],
          listeners = []; //used for capturing event listeners from $('').on 
    //listeners = [{ elem: null, events: [{ name: '', list: [[selector, event]] }] }];

    const classMatch = /^\.[\w-]*$/,
          singlet = /^\w+$/;

    let ajaxQueue = [];
    let ajaxWait = false;

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //Internal functions //////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    /**
     * selector
     * @constructor
     */
    function select(sel) {
        //main function, instantiated via $(sel)
        if (sel) { this.push(query(document, sel)); }
        return this;
    }

    function query(context, selector) {
        //gets a list of elements from a CSS selector
        if (context == null) { return [];}
        let elems = [];
        if (isType(selector,4)) {
            //elements are already defined instead of using a selector /////////////////////////////////////
            elems = isType(selector, 5) ? selector : [selector];
        } else if (selector != null && selector != '') {
            //only use vanilla Javascript to select DOM elements based on a CSS selector (Chrome 1, IE 9, Safari 3.2, Firefox 3.5, Opera 10)
            context = context || doc;
            const el = (
                classMatch.test(selector) ?
                    context.getElementsByClassName(selector.slice(1)) :
                    singlet.test(selector) ?
                        context.getElementsByTagName(selector) :
                        context.querySelectorAll(selector)
            );
            //convert node list into array
            for (let i = el.length; i--; elems.unshift(el[i])) { };
        }
        return elems;
    }

    function isDescendant(parent, child) {
        //checks if element is child of another element
        let node = child;
        while (node != null) {
            node = node.parentNode;
            if (node == parent) {
                return tru;
            }
        }
        return fals;
    }

    function styleName(str) {
        //gets the proper style name from shorthand string
        //for example: border-width translates to borderWidth;
        if (str.indexOf('-') < 0) { return str; }
        const name = str.split('-');
        if(name.length > 1){name[1] = name[1][0].toUpperCase() + name[1].substr(1);}
        return name.join('');
    }

    function setStyle(e, n, val) {
        //properly set a style for an element
        if (e.nodeName == '#text') { return; }
        let v = val;
        
        //check for empty value
        
        if (v === '' || v === null) {
            e.style[n] = v == '' ? null : v; return; 
        }

        //check for numbers that should be using 'px';

        if ((Number(v) == v || v === 0) && v.toString().indexOf('%') < 0) {
            if (pxStyles.indexOf(n) >= 0) {
                v = val + 'px';
            } else if (pxStylesPrefix.some(function (a) { return n.indexOf(a) == 0 }) === true) {
                v = val + 'px';
            }
        }

        //last resort, set style to string value\
        e.style[n] = v;
    }

    function getObj(obj) {
        //get a string from object (either string, number, or function)
        if (obj == null) { return null; }
        if (isType(obj, 1)) {
            //handle object as string
            return obj;
        }
        if (isType(obj, 5)) {
            //handle object as array
            return obj[0];
        }
        if (isType(obj, 6)) {
            //handle object as function (get value from object function execution)
            return getObj(obj());
        }
        return obj;
    }
    
    function diffArray(arr, remove) {
        return arr.filter(function (el) {
            return !remove.includes(el);
        });
    }

    function isArrayThen(obj, arrayFunc) {
        if (isType(obj, 5)) {
            //handle content as array
            for(let x in obj){
                arrayFunc.call(this,obj[x]);
            }
            return tru;
        }
        return fals;
    }

    function isType(obj, type) {
        if(obj != null){
            switch (type) {
                case 0: return tru; //anything
                case 1: return typeof (obj) == 'string'; //string
                case 2: return typeof (obj) == 'boolean'; //boolean
                case 3: return !isNaN(parseFloat(obj)) && isFinite(obj); //number
                case 4: return typeof (obj) == 'object'; //object
                case 5: return typeof obj.splice === 'function'; //array
                case 6: return typeof obj == 'function'; //function
            }
        }
        return fals;
    }

    function normalizeArgs(types, args) {
        let results = [],
            a = [].slice.call(args), //convert arguments object into array
            step = types.length - 1,
            req, skip;
        for (let x = a.length-1; x >= 0; x--) {
            for (let i = step; i >= 0; i--) {
                skip = fals;
                if (types[i].o == tru) {
                    //make sure there are enough arguments
                    //left over for required types
                    req = 0;
                    for (let t = 0; t <= i; t++) {
                        if (types[t].o == fals) { req++;}
                    }
                    skip = req > x;
                }
                if (skip == fals) { skip = !isType(a[x], types[i].t) && a[x] != null; }
                
                results[i] = !skip ? a[x] : null;
                step = i - 1;
                if (!skip || a[x] == null) { break; }
            }
        }
        return results;
    }

    function insertContent(obj, elements, stringFunc, objFunc) {
        //checks type of object and execute callback functions depending on object type
        const type = isType(obj, 1);
        if (type == tru) {
            for (let x = 0; x < elements.length; x++) {
                stringFunc(elements[x]);
            }
        } else {
            for (let x = 0; x < elements.length; x++) {
                objFunc(elements[x]);
            }
        }
        
        return this;
    }

    function clone(elems) {
        let n = new select(null);
        return n.push(elems);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //prototype functions that are accessable by $(selector) //////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    select.prototype = {

        add: function (elems) {
            //Add new (unique) elements to the existing elements array
            let obj = getObj(elems);
            if (!obj) { return this; }
            if (obj.elements) { obj = obj.elements; }
            for (let x in obj) {
                //check for duplicates
                if (this.indexOf(obj[x]) < 0) {
                    //element is unique
                    this.push(obj[x]);
                }
            }
            return this;
        },

        addClass: function (classes) {
            //Add class name to each of the elements in the collection. 
            //Multiple class names can be given in a space-separated string.
            if (this.length > 0) {
                const classList = classes.split(' ');
                for (let x = 0; x < this.length; x++) {
                    let e = this[x];
                    //alter classname for each element in selector
                    if (e.className) {
                        let className = e.className;
                        const list = className.split(' ');
                        for (let c in classList) {
                            if (list.indexOf(classList[c]) < 0) {
                                //class doesn't exist in element classname list
                                className += ' ' + classList[c];
                            }
                        }
                        //finally, change element classname if it was altered
                        e.className = className;
                    } else {
                        e.className = classes;
                    }
                }
            }
            return this;
        },

        after: function (content) {
            //Add content to the DOM after each elements in the collection. 
            //The content can be an HTML string, a DOM node or an array of nodes.
            let obj = getObj(content);
            if (isArrayThen(obj, this.after) || obj == null) { return this; }

            insertContent(obj, this,
                function (e) { e.insertAdjacentHTML('afterend', obj); },
                function (e) { e.parentNode.insertBefore(obj, e.nextSibling); }
            );
            return this;
        },

        animate: function (props, options) {
            if (typeof (Velocity) != 'undefined') {
                Velocity(this, props, options);
            }
            return this;
        },

        append: function (content) {
            //Append content to the DOM inside each individual element in the collection. 
            //The content can be an HTML string, a DOM node or an array of nodes.

            let obj = getObj(content);
            if (isArrayThen.call(this, obj, this.append) || obj == null) { return this; }
            insertContent(obj, this,
                function (e) { e.insertAdjacentHTML('beforeend', obj); },
                function (e) { e.appendChild(obj); }
            );
            return this;
        },

        attr: function (name, val) {
            //Read or set DOM attributes. When no value is given, reads 
            //specified attribute from the first element in the collection. 
            //When value is given, sets the attribute to that value on each element 
            //in the collection. When value is null, the attribute is removed  = function(like with removeAttr). 
            //Multiple attributes can be set by passing an object with name-value pairs.
            let n = getObj(name), v = getObj(val);
            if (isType(n, 5)) {
                //get array of attribute values from first element
                let attrs = {};
                for (let p in n) {
                    attrs[n[p]] = this.length > 0 ? this[0].getAttribute(n[p]) : attrs[n[p]] = '';
                }
                return attrs;
            } else {
                if (v != null) {
                    //set single attribute value to all elements in list
                    for (let x = 0; x < this.length; x++) {
                        this[x].setAttribute(n, v);
                    }
                } else {
                    //get single attribute value from first element in list
                    return this.length > 0 ? this[0].getAttribute(n) : '';
                }
            }
            return this;
        },

        before: function (content) {
            //Add content to the DOM before each element in the collection. 
            //The content can be an HTML string, a DOM node or an array of nodes.
            let obj = getObj(content);
            if (isArrayThen(obj, this.before) || obj == null) { return this; }
            insertContent(obj, this,
                function (e) { e.insertAdjacentHTML('beforebegin', obj); },
                function (e) { e.parentNode.insertBefore(obj, e); }
            );
            return this;
        },

        children: function (sel) {
            //Get immediate children of each element in the current collection. 
            //If selector is given, filter the results to only include ones matching the CSS select.
            let elems = [];
            let seltype = 0;
            if (sel != null) {
                if (isType(sel, 3)) {
                    seltype = 1;
                } else {
                    seltype = 2;
                }
            }
            this.each(function (e) {
                if (seltype == 1) {
                    //get child from index
                    elems.push(e.children[sel]);
                } else {
                    for (let x = 0; x < e.children.length; x++) {
                        if (!seltype) { //no selector
                            elems.push(e.children[x]);
                        } else if (seltype == 2) { //match selector
                            if (e.matches(sel)) {
                                elems.push(e.children[x]);
                            }
                        }
                    }
                }
            });
            return clone(elems);
        },

        closest: function (selector) {
            //Traverse upwards from the current element to find the first element that matches the select. 
            return this;
        },

        css: function (params) {
            //Read or set CSS properties on DOM elements. When no value is given, 
            //returns the CSS property from the first element in the collection. 
            //When a value is given, sets the property to that value on each element of the collection.

            //Multiple properties can be retrieved at once by passing an array of property names. 
            //Multiple properties can be set by passing an object to the method.

            //When a value for a property is blank  = function(empty string, null, or undefined), that property is removed. 
            //When a unitless number value is given, "px" is appended to it for properties that require units.
            if (isType(params, 4)) {
                let haskeys = fals;
                for (let x in params) {
                    //if params is an object with key/value pairs, apply styling to elements\
                    haskeys = tru;
                    this.each(function (e) {
                        setStyle(e, x, params[x]);
                    });
                }
                if (haskeys) { return this; }
                if (isType(params, 5)) {
                    //if params is an array of style names, return an array of style values
                    let vals = [];
                    this.each(function (e) {
                        let props = new Object();
                        params.forEach(function (param) {
                            const prop = e.style[styleName(param)];
                            if (prop) { props[param] = prop; }
                        });
                        vals.push(props);
                    });
                    return vals;
                }
            } else if (isType(params, 1)) {
                const name = styleName(params);
                const arg = arguments[1];
                if (isType(arg, 1)) {
                    //set a single style property if two string arguments are supplied (key, value);
                    this.each(function (e) {
                        setStyle(e, name, arg);
                    });
                } else {
                    //if params is a string, return a single style property
                    if (this.length > 0) {

                        if (this.length == 1) {
                            //return a string value for one element
                            return this[0].style[name];
                        } else {
                            //return an array of strings for multiple elements
                            let vals = [];
                            this.each(function (e) {
                                let val = e.style[name];
                                if (val == null) { val = ''; }
                                vals.push(val);
                            });
                            return vals;
                        }
                    }
                }

            }
            return this;
        },

        each: function (func) {
            //Iterate through every element of the collection. Inside the iterator function, 
            //this keyword refers to the current item  = function(also passed as the second argument to the function). 
            //If the iterator select.prototype.returns 0, iteration stops.
            for (let x = 0; x < this.length; x++) {
                func.call(this, this[x]);
            }
            return this;
        },

        empty: function (func) {
            //Clear DOM contents of each element in the collection.
            this.each(function (e) {
                e.innerHTML = '';
            });
            return this;
        },

        eq: function (index) {
            //Reduce the set of matched elements to the one at the specified index
            let elems = [];
            if (index > this.length - 1) {
                //out of bounds
                elems = [];
            } else if (index < 0) {
                //negetive index
                if (index * -1 >= this.length) {
                    elems = [];
                } else {
                    elems = [this[(this.length - 1) + index]];
                }
            } else {
                elems = [this[index]];
            }
            return clone(elems);
        },

        filter: function (sel) {
            //Filter the collection to contain only items that match the CSS select. 
            //If a select.prototype.is given, return only elements for which the select.prototype.returns a truthy value. 
            let elems = [];
            if (isType(sel, 6)) {
                //filter a boolean function
                for (let i = 0; i < this.length; i++) {
                    if (sel.call(this[i], i, this[i]) == tru) { elems.push(this[i]); }
                }
            } else {
                //filter selector string
                const found = query(document, sel);
                if (found.length > 0) {
                    this.each(function (e) {
                        if (found.indexOf(e) >= 0) {
                            //make sure no duplicates are being added to the array
                            if (elems.indexOf(e) < 0) { elems.push(e); }
                        }
                    });
                }
            }
            return clone(elems);
        },

        find: function (sel) {
            //Find elements that match CSS selector executed in scope of nodes in the current collection.
            let elems = [];
            if (this.length > 0) {
                this.each(function (e) {
                    const found = query(e, sel);
                    if (found.length > 0) {
                        found.forEach(function (a) {
                            //make sure no duplicates are being added to the array
                            if (elems.indexOf(a) < 0) { elems.push(a); }
                        });
                    }
                });
            }
            return clone(elems);
        },

        first: function () {
            //the first element found in the selector
            let elems = [];
            if (this.length > 0) {
                elems = [this[0]];
            }
            return clone(elems);
        },

        get: function (index) {
            //Get all elements or a single element from the current collection. 
            //When no index is given, returns all elements in an ordinary array. 
            //When index is specified, return only the element at that position. 
            return this[index || 0];
        },

        has: function (selector) {
            //Filter the current collection to include only elements that have 
            //any number of descendants that match a selector, or that contain a specific DOM node.
            let elems = [];
            if (this.length > 0) {
                this.each(function (e) {
                    if (query(e, selector).length > 0) {
                        if (elems.indexOf(e) < 0) { elems.push(e); }
                    }
                });
            }
            return clone(elems);
        },

        hasClass: function (classes) {
            //Check if any elements in the collection have the specified class.
            let classList;
            if (isType(classes, 5)) {
                classList = classes;
            } else if (isType(classes, 1)) {
                classList = classes.split(' ');
            }
            for (let x = 0; x < this.length; x++) {
                const n = this[x].className || '';
                if (isType(n, 1)) {
                    const classNames = n.split(' ');
                    if (classNames.length > 0) {
                        if (
                            classList.every(function (a) {
                                return classNames.some(function (b) { return a == b; });
                            })
                        ) {
                            return tru;
                        }
                    }
                }
            }
            return fals;
        },

        height: function (val) {
            //Get the height of the first element in the collection; 
            //or set the height of all elements in the collection.
            //this function differs from jQuery as it doesn't care
            //about box-sizing & border when returning the height
            //of an element (when val is not specified). 
            //It simply returns vanilla js offsetHeight (as it should);
            let obj = getObj(val);
            if (isType(obj, 1)) {
                const n = parseFloat(obj);
                if (!isNaN(n)) { obj = n; } else {
                    //height is string
                    this.each(function (e) {
                        if (e != window && e != document) {
                            e.style.height = obj;
                        }
                    });
                    return this;
                }
            } else if (obj == null) {
                if (this.length > 0) {
                    //get height from first element
                    const elem = this[0];
                    if (elem == window) {
                        return window.innerHeight;
                    } else if (elem == document) {
                        const body = document.body;
                        const html = document.documentElement;
                        return Math.max(
                            body.offsetHeight,
                            body.scrollHeight,
                            html.clientHeight,
                            html.offsetHeight,
                            html.scrollHeight
                        );
                    } else {
                        return elem.clientHeight;
                    }
                }
            } else {
                //height is a number
                if (obj == 0) {
                    this.each(function (e) {
                        e.style.height = 0;
                    });
                } else {
                    this.each(function (e) {
                        e.style.height = obj + 'px';
                    });
                }
            }
            return this;
        },

        hide: function () {
            //Hide elements in this collection by setting their display CSS property to none.
            this.each(function (e) {
                e.style.display = 'none';
            });
            return this;
        },

        hover: function () {
            const args = normalizeArgs([
                { t: 1, o: tru }, //0: selector = string
                { t: 0, o: tru }, //1: data = anything
                { t: 6, o: fals }, //2: onEnter = function
                { t: 6, o: fals }  //3: onLeave = function
            ], arguments);

            let entered = fals;
            this.on('mouseenter', args[0], args[1], function (e) {
                if (!entered) {
                    if (args[2]) { args[2](e); }
                    entered = tru;
                }
            });
            this.on('mouseleave', args[0], args[1], function (e) {
                let p = e, f = fals;
                while (p != null) { if (p == elem) { f = tru; break; } p = p.parentNode; }
                if (!f) {
                    entered = fals;
                    if (args[3]) { args[3](e); }
                }
            });
        },

        html: function (content) {
            //Get or set HTML contents of elements in the collection. 
            //When no content given, returns innerHTML of the first element. 
            //When content is given, use it to replace contents of each element. 
            let obj = getObj(content);
            if (obj == null) {
                if (this.length > 0) {
                    return this[0].innerHTML;
                } else {
                    return '';
                }
            } else {
                this.each(function (e) {
                    e.innerHTML = obj;
                });
            }
            return this;
        },

        /**
         * @suppress {checkTypes}
         */
        index: function (e) {
            //Get the position of an element. When no element is given, 
            //returns position of the current element among its siblings. 
            //When an element is given, returns its position in the current collection. 
            //Returns -1 if not found.
            let i = -1;
            if (this.length > 0) {
                const elem = e ? e : this[0];
                const p = elem.parentNode;
                let c;
                if (p) {
                    c = p.children;
                    if ([].indexOf) {
                        return [].indexOf.call(c, elem);
                    } else {
                        //fallback for older browsers
                        for (let x = 0; x < c.length; x++) {
                            if (c[x] == elem) {
                                return x;
                            }
                        }
                    }
                }
            }
            return i;
        },

        innerHeight: function (height) {
            //Get the current computed inner height (including padding but not border) for the 
            //first element in the set of matched elements or set the inner height of every matched element
            let obj = getObj(height);
            if (obj == null) {
                //get inner height of first element (minus padding)
                if (this.length > 0) {
                    const e = this[0];
                    const style = getComputedStyle(e);
                    let padtop = parseFloat(style.paddingTop);
                    let padbot = parseFloat(style.paddingBottom);
                    if (isNaN(padtop)) { padtop = 0; }
                    if (isNaN(padbot)) { padbot = 0; }
                    return e.clientHeight - (padtop + padbot);
                }
            } else {
                //set height of elements
                return this.height(height);
            }
        },

        innerWidth: function (width) {
            //Get the current computed inner width (including padding but not border) for the 
            //first element in the set of matched elements or set the inner width of every matched element
            let obj = getObj(width);
            if (obj == null) {
                //get inner width of first element (minus padding)
                if (this.length > 0) {
                    const e = this[0];
                    const style = getComputedStyle(e);
                    let padright = parseFloat(style.paddingRight);
                    let padleft = parseFloat(style.paddingLeft);
                    if (isNaN(padright)) { padright = 0; }
                    if (isNaN(padleft)) { padleft = 0; }
                    return e.clientWidth - (padright + padleft);
                }
            } else {
                //set width of elements
                return this.width(width);
            }
        },

        is: function (selector) {
            //Check if all the elements of the current collection matches the CSS select.
            if (this.length > 0) {
                const self = this;
                let obj = getObj(selector);
                for (let x = 0; x < this.length; x++) {
                    switch (obj) {
                        case ':focus':
                            if (this[x] == document.activeElement) {
                                return tru;
                            }
                            break;
                        default:
                            const q = query(this[x].parentNode, obj);
                            if (q.some(function (a) { return a == self[0] })) {
                                return tru;
                            }
                            break;
                    }
                }
                
            }
            return fals;
        },

        last: function () {
            //Get the last element of the current collection.
            let elems = [];
            if (this.length > 0) {
                elems = [this[this.length - 1]];
            }
            return clone(elems);
        },

        map: function (func) { //func(index, element)        
            //Iterate through every element of the collection. Inside the iterator function, 
            //this keyword refers to the current item  = function(also passed as the second argument to the function). 
            //If the iterator select.prototype.returns 0, iteration stops.
            let mapped = [];
            for (let x = 0; x < this.length; x++) {
                mapped.push(func(x, this[x])); 
            }
            return mapped;
        },

        next: function (selector) {
            //Get the next sibling optionally filtered by selector of each element in the collection.
            let elems = [];
            this.each(function (e) {
                let el = e.nextSibling;
                if (selector && el) {
                    //use selector
                    const q = query(e.parentNode, selector);
                    while (el != null) {
                        if (el.nodeName != '#text') {
                            if (q.some(function (s) { return s == el })) {
                                elems.push(el);
                                break;
                            }
                        }
                        el = el.nextSibling;
                    }
                } else if (el) {
                    //no selector
                    while (el != null) {
                        if (el.nodeName != '#text') {
                            elems.push(el);
                            break;
                        }
                        el = el.nextSibling;
                    }
                }
            });
            return clone(elems);
        },

        nextAll: function (selector) {
            //Get all siblings below current sibling optionally filtered by selector of each element in the collection.
            let elems = [];
            if (selector) {
                //use selector
                this.each(function (e) {
                    const q = query(e, selector);
                    let n = e.nextSibling;
                    while (n) {
                        while (n.nodeName == '#text') {
                            n = n.nextSibling;
                            if (!n) { break; }
                        }
                        if (!n) { break; }
                        if (q.some(function (s) { return s == n; })) { elems.push(n); }
                        n = n.nextSibling;
                    }
                });
            } else {
                //no selector
                this.each(function (e) {
                    let n = e.nextSibling;
                    while (n) {
                        while (n.nodeName == '#text') {
                            n = n.nextSibling;
                            if (!n) { break; }
                        }
                        if (!n) { break; }
                        elems.push(n);
                        n = n.nextSibling;
                    }
                });
            }
            return clone(elems);
        },

        not: function (selector) {
            //Filter the current collection to get a new collection of elements that don't match the CSS select. 
            //If another collection is given instead of selector, return only elements not present in it. 
            //If a select.prototype.is given, return only elements for which the select.prototype.returns a falsy value. 
            //Inside the function, the this keyword refers to the current element.
            const sel = getObj(selector);
            let elems = this;
            //check if selector is an array (of selectors)
            if (isType(sel, 5)) {
                sel.forEach(function (s) {
                    const q = query(document, s);
                    elems = diffArray(elems, q);
                });
                this.push(elems);
                return this;
            }
            return clone(diffArray(elems, query(document, sel)));
        },

        off: function () {
            //remove an event handler
            let args = normalizeArgs([
                { t: 1, o: fals }, //0: event = string
                { t: 1, o: tru }, //1: selector = string (optional)
                { t: 6, o: fals }  //2: handler = function
            ], arguments);
            this.each(function (e) {
                for (let x = 0; x < listeners.length; x++) {
                    if (listeners[x].elem == e) {
                        //found element in listeners array, now find specific function (func)
                        const item = listeners[x];
                        if (args[2] == null) {
                            //if no function specified, remove all listeners for a specific event
                            if (args[0] == null) {
                                //remove all events and functions for element from listener
                                for (let y = 0; y < item.events.length; y++) {
                                    const ev = item.events[y];
                                    for (let z = 0; z < ev.list.length; z++) {
                                        e.removeEventListener(ev.name, ev.list[z][1], tru);
                                    }
                                }
                                listeners.splice(x, 1);
                            } else {
                                //remove all functions (for event) from element in listener
                                for (let y = 0; y < item.events.length; y++) {
                                    if (item.events[y].name == args[0]) {
                                        const ev = item.events[y];
                                        for (let z = 0; z < ev.list.length; z++) {
                                            e.removeEventListener(args[0], ev.list[z][1], tru);
                                        }
                                        listeners[x].events.splice(y, 1);
                                        if (listeners[x].events.length == 0) {
                                            //remove element from listeners array since no more events exist for the element
                                            listeners.splice(x, 1);
                                        }
                                        break;
                                    }
                                }
                            }
                        } else {
                            //remove specific listener based on event & function
                            for (let y = 0; y < item.events.length; y++) {
                                if (item.events[y].name == args[0]) {
                                    //remove specific event from element in listeners array
                                    const ev = item.events[y];
                                    for (let z = 0; z < ev.list.length; z++) {
                                        if (ev.list[z][1].toString() === args[2].toString() //check function match
                                            && ev.list[z][0] == args[1]) { //check selector match
                                            e.removeEventListener(args[0], args[2], tru);
                                            listeners[x].events[y].list.splice(z, 1);
                                            break;
                                        }
                                    }

                                    if (listeners[x].events[y].list.length == 0) {
                                        //remove event from element list array since no more functions exist for the event
                                        listeners[x].events.splice(y, 1);
                                        if (listeners[x].events.length == 0) {
                                            //remove element from listeners array since no more events exist for the element
                                            listeners.splice(x, 1);
                                        }
                                    }
                                    break;
                                }
                            }
                        }
                        break;
                    }
                }
            });
            return this;
        },

        offset: function () {
            //Get position of the element in the document. 
            //Returns an object with properties: top, left, width and height.

            //When given an object with properties left and top, use those values to 
            //position each element in the collection relative to the document.
            if (this.length > 0) {
                const box = this[0].getBoundingClientRect();
                return {
                    left: box.left + document.body.scrollLeft,
                    top: box.top + document.body.scrollTop
                };
            }
            return { left: 0, top: 0 };
        },

        offsetParent: function () {
            //Find the first ancestor element that is positioned, 
            //meaning its CSS position value is "relative", "absolute"" or "fixed".
            if (this.length > 0) {
                return this[0].offsetParent;
            }
            return null;
        },

        on: function () {
            //Attach an event handler function for one or more events to the selected elements.
            let args = normalizeArgs([
                { t: 1, o: fals }, //0: event = string
                { t: 1, o: tru }, //1: selector = string (optional)
                { t: 0, o: tru }, //2: data = anything (optional)
                { t: 6, o: fals }  //3: handler = function
            ], arguments);
            const events = args[0].replace(/\s/g, '').split(',');
            for (let i = 0; i < events.length; i++) {
                const ev = events[i];
                if (ev == "hover") {
                    this.hover(args[1], args[2], args[3], args[3]);
                } else {
                    this.each(function (e) {
                        let params = [args[1], args[3]];
                        if (args[1] != null && args[1] != '') {
                            function delegate(el) {
                                const sels = query(e, args[1]);
                                for (let x = 0; x < sels.length; x++) {
                                    if (el.target == sels[x]) {
                                        args[3].apply(sels[x], [].slice.call(arguments));
                                    }
                                }
                            }
                            params = [args[1], delegate];
                            e.addEventListener(ev, delegate, tru);
                        } else {
                            e.addEventListener(ev, args[3], tru);
                        }

                        let listen = fals;
                        for (let x = 0; x < listeners.length; x++) {
                            if (listeners[x].elem == e) {
                                //found element, now find specific event
                                const events = listeners[x].events;
                                let f = fals;
                                for (let y = 0; y < events.length; y++) {
                                    if (events[y].name == ev) {
                                        //found existing event in list
                                        listeners[x].events[y].list.push(params);
                                        f = tru;
                                        break;
                                    }
                                }
                                if (f == fals) {
                                    //event doesn't exist yet
                                    listeners[x].events.push({ name: ev, list: [params] });
                                }
                                listen = tru;
                                break;
                            }
                        }
                        if (listen == fals) { listeners.push({ elem: e, events: [{ name: ev, list: [params] }] }); }
                    });
                }
            }
            return this;
        },

        //TODO: one
        one: function (event, func) {
            //Attach a handler to an event for the elements. The handler is executed at most once per element per event type
        },

        //TODO: outerHeight
        outerHeight: function () {

        },

        //TODO: outerWidth
        outerWidth: function () {

        },

        parent: function (selector) {
            //Get immediate parents of each element in the collection. 
            //If CSS selector is given, filter results to include only ones matching the select.
            let elems = [];
            this.each(function (e) {
                const el = e.parentNode;
                if (selector == null || selector == '') {
                    if (elems.indexOf(el) < 0) {
                        elems.push(el);
                    }
                } else if (el.matches(selector)) {
                    if (elems.indexOf(el) < 0) {
                        elems.push(el);
                    }
                }

            });
            return clone(elems);
        },

        parents: function (selector) {
            //Get all ancestors of each element in the selector. 
            //If CSS selector is given, filter results to include only ones matching the select.
            let elems = [];
            this.each(function (e) {
                let el = e.parentNode;
                while (el) {
                    if (selector == null || selector == '') {
                        if (elems.indexOf(el) < 0) {
                            elems.push(el);
                        }
                    } else {
                        if (el.matches) {
                            if (el.matches(selector)) {
                                if (elems.indexOf(el) < 0) {
                                    elems.push(el);
                                }
                            }
                        } else if (el.matchesSelector) {
                            if (el.matchesSelector(selector)) {
                                if (elems.indexOf(el) < 0) {
                                    elems.push(el);
                                }
                            }
                        }
                    }
                    el = el.parentNode;
                }
            });
            return clone(elems);
        },

        position: function () {
            //Get the position of the first element in the collection, relative to the offsetParent. 
            //This information is useful when absolutely positioning an element to appear aligned with another.
            if (this.length > 0) {
                return { left: this[0].offsetLeft, top: this[0].offsetTop };
            }
            return { left: 0, top: 0 };
        },

        prepend: function (content) {
            //Prepend content to the DOM inside each element in the collection. 
            //The content can be an HTML string, a DOM node or an array of nodes.
            let obj = getObj(content);
            if (isArrayThen(obj, this.append) || obj == null) { return this; }


            insertContent(obj, this,
                function (e) { e.insertAdjacentHTML('afterbegin', obj); },
                function (e) { e.insertBefore(obj, e.firstChild); }
            );
            return this;
        },

        prev: function (selector) {
            //Get the previous sibling optionally filtered by selector of each element in the collection.
            let elems = [];
            this.each(function (e) {
                let el = e.previousSibling;
                if (selector && el) {
                    //use selector
                    const q = query(e.parentNode, selector);
                    while (el != null) {
                        if (el.nodeName != '#text') {
                            if (q.some(function (s) { return s == el })) {
                                elems.push(el);
                                break;
                            }
                        }
                        el = el.previousSibling;
                    }
                } else if (el) {
                    //no selector
                    while (el != null) {
                        if (el.nodeName != '#text') {
                            elems.push(el);
                            break;
                        }
                        el = el.previousSibling;
                    }
                }
            });
            return clone(elems);
        },

        prop: function (name, val) {
            //Read or set properties of DOM elements. This should be preferred over attr in case of 
            //reading values of properties that change with user interaction over time, such as checked and selected.
            const n = getObj(name);
            let v = getObj(val);
            if (isType(n, 5)) {
                //get multiple properties from the first element
                let props = {};
                n.forEach(function (p) {
                    props[p] = this.prop(p);
                });
                return props;
            }

            const execAttr = function (a, b) {
                //get / set / remove DOM attribute
                if (v != null) {
                    if (v == '--') {
                        //remove
                        this.each(function (e) {
                            e.removeAttribute(a);
                        });
                    } else {
                        //set
                        if (v == fals) {
                            this.each(function (e) {
                                e.removeAttribute(a);
                            });
                        } else {
                            this.attr(a, b);
                        }
                    }
                } else {
                    //get
                    if (this.length > 0) {
                        return this[0].getAttribute(a) || '';
                    }
                }
            };

            const execProp = function (a) {
                if (v != null) {
                    if (v == '--') {
                        //remove
                        this.each(function (e) {
                            if (e[a]) { delete e[a]; }
                        });
                    } else {
                        //set
                        v = v == 0 ? fals : tru;
                        this.each(function (e) {
                            e[a] = v;
                        });
                    }

                } else {
                    //get
                    if (this.length > 0) {
                        let e = this[0];
                        let b = e[a];
                        if (b == null) {
                            b = e.getAttribute(a) != null ? tru : fals;
                            e[a] = b;
                        }
                        return b;
                    }
                }
            };

            //get, set, or remove (if val == '--') a specific property from element(s)
            let nn = '';
            switch (n) {
                case "defaultChecked":
                    nn = 'checked';
                    break;
                case "checked":
                    if (!v) { if (this.length > 0) { return this[0].checked; } }
                    break;
                case "defaultSelected":
                    nn = 'selected';
                    break;
                case "selected":
                    break;
                case "defaultDisabled":
                    nn = 'disabled';
                    break;
                case "disabled":
                    //get/set/remove boolean property that belongs to the DOM element object or is an attribute (default)
                    break;

                case "selectedIndex":
                    if (v != null) {
                        if (v === parseInt(v, 10)) {
                            this.each(function (e) {
                                if (e.nodeType == 'SELECT') {
                                    e.selectedIndex = v;
                                }
                            });
                        }
                    }
                    break;
                    return;

                case "nodeName":
                    if (val != null) {
                        //set node name
                        //TODO: replace DOM element with new element of new node name, cloning all attributes & appending all children elements
                        return;
                    } else {
                        //get node name
                        if (this.length > 0) {
                            return this[0].nodeName;
                        } else {
                            return '';
                        }
                    }
                    break;
                case "tagName":
                    if (val != null) {
                        //set node name
                        //TODO: replace DOM element with new element of new tag name, cloning all attributes & appending all children elements
                        return;
                    } else {
                        //get tag name
                        if (this.length > 0) {
                            return this[0].tagName;
                        } else {
                            return '';
                        }
                    }
                    break;

                default:
                    // last resort to get/set/remove property value from style or attribute
                    //first, try getting a style
                    let a = execProp.call(this, n);
                    if (a != null && typeof a != 'undefined') {
                        return a;
                    } else {
                        //next, try getting a attribute
                        a = execAttr.call(this, n, v);
                        if (a != null) {
                            return a;
                        }
                    }
                    break;
            }
            if (nn != '') {
                //get/set/remove default property
                const a = execAttr.call(this, nn, nn);
                if (a != null) { return a; }
            } else {
                //get/set/remove property
                const a = execProp.call(this, n);
                if (a != null) { return a; }
            }

            return this;
        },

        push: function (elems) {
            [].push.apply(this, elems);
            return this;
        },

        ready: function (callback) {
            if (this.length == 1) {
                if (this[0] == document) {
                    if (document.readyState != 'loading') {
                        callback();
                    } else {
                        document.addEventListener('DOMContentLoaded', callback);
                    }
                }
            }
        },

        remove: function (selector) {
            //Remove the set of matched elements from the DOM
            this.each(function (e) {
                e.parentNode.removeChild(e);
            });
            this.push([]);
            return this;
        },

        removeAttr: function (attr) {
            //Remove an attribute from each element in the set of matched elements
            let obj = getObj(attr);
            if (isType(obj, 5)) {
                obj.forEach(function (a) {
                    this.each(function (e) {
                        e.removeAttribute(a);
                    });
                });
            } else if (typeof obj == 'string') {
                this.each(function (e) {
                    e.removeAttribute(obj);
                });
            }

            return this;
        },

        removeClass: function (className) {
            //Remove a single class, multiple classes, or all classes from each element in the set of matched elements
            let obj = getObj(className);
            if (typeof obj == 'string') {
                //check for class name array
                obj = obj.replace(/\,/g, ' ').replace(/\s\s/g, ' ');
                if (obj.indexOf(' ') > 0) {
                    obj = obj.split(' ');
                }
            }
            if (isType(obj, 5)) {
                this.each(function (e) {
                    obj.forEach(function (a) {
                        if (e.className) {
                            e.className = e.className.split(' ').filter(function (b) { return b != '' && b != a; }).join(' ');
                        }
                    });
                });
            } else if (typeof obj == 'string') {
                this.each(function (e) {
                    if (e.className) {
                        e.className = e.className.split(' ').filter(function (b) { return b != '' && b != obj; }).join(' ');
                    }
                });
            }
            return this;
        },

        removeProp: function (name) {
            //Remove a property for the set of matched elements
            this.prop(name, '--');
            return this;
        },

        serialize: function () {
            if (this.length == 0) { return ''; }
            let form = this[0];
            if (!form || form.nodeName !== "FORM") {
                return '';
            }
            let i, j, q = [];
            for (i = form.elements.length - 1; i >= 0; i = i - 1) {
                if (form.elements[i].name === "") {
                    continue;
                }
                switch (form.elements[i].nodeName) {
                    case 'INPUT':
                        switch (form.elements[i].type) {
                            case 'text':
                            case 'hidden':
                            case 'password':
                            case 'button':
                            case 'reset':
                            case 'submit':
                                q.push(form.elements[i].name + "=" + encodeURIComponent(form.elements[i].value));
                                break;
                            case 'checkbox':
                            case 'radio':
                                if (form.elements[i].checked) {
                                    q.push(form.elements[i].name + "=" + encodeURIComponent(form.elements[i].value));
                                }
                                break;
                        }
                        break;
                    case 'file':
                        break;
                    case 'TEXTAREA':
                        q.push(form.elements[i].name + "=" + encodeURIComponent(form.elements[i].value));
                        break;
                    case 'SELECT':
                        switch (form.elements[i].type) {
                            case 'select-one':
                                q.push(form.elements[i].name + "=" + encodeURIComponent(form.elements[i].value));
                                break;
                            case 'select-multiple':
                                for (j = form.elements[i].options.length - 1; j >= 0; j = j - 1) {
                                    if (form.elements[i].options[j].selected) {
                                        q.push(form.elements[i].name + "=" + encodeURIComponent(form.elements[i].options[j].value));
                                    }
                                }
                                break;
                        }
                        break;
                    case 'BUTTON':
                        switch (form.elements[i].type) {
                            case 'reset':
                            case 'submit':
                            case 'button':
                                q.push(form.elements[i].name + "=" + encodeURIComponent(form.elements[i].value));
                                break;
                        }
                        break;
                }
            }
            return q.join("&");
        },

        show: function () {
            //Display the matched elements
            this.removeClass('hide');
            this.each(function (e) {
                e.style.display = 'block';
            });
            return this;
        },

        siblings: function (selector) {
            //Get the siblings of each element in the set of matched elements, optionally filtered by a selector
            let elems = [];
            let sibs = [];
            let q = [];
            const sel = getObj(selector);
            const add = function (e) {
                if (!elems.some(function (a) { return a == e })) { elems.push(e); }
            }
            const find = function (e, s) {
                //find siblings
                if (s != null) {
                    q = query(e.parentNode, s);
                }
                sibs = e.parentNode.children;
                for (let x = 0; x < sibs.length; x++) {
                    const sib = sibs[x];
                    if (sib != e) {
                        if (s != null) {
                            if (q.some(function (a) { return a == sib; })) {
                                add(sib);
                            }
                        } else {
                            add(sib);
                        }
                    }
                }
            };

            if (sel != null) {
                if (isType(sel, 5)) {
                    this.each(function (e) {
                        sel.forEach(function (s) {
                            find(e, s);
                        });
                    });
                } else {
                    this.each(function (e) {
                        find(e, sel);
                    });
                }
            } else {
                this.each(function (e) {
                    find(e, null);
                });
            }
            return clone(elems);
        },

        //TODO: slice
        slice: function () {
            //Reduce the set of matched elements to a subset specified by a range of indices
            [].slice.apply(this, arguments);
            return this;
        },

        splice: function () {
            [].splice.apply(this, arguments);
        },

        stop: function () {
            if (typeof (Velocity) != 'undefined') {
                Velocity(this, "stop");
            }
            return this;
        },

        //TODO: text
        text: function () {
            //Get the combined text contents of each element in the set of matched elements, including their descendants, or set the text contents of the matched elements
            return '';
        },

        toggle: function () {
            //Display or hide the matched elements
            this.each(function (e) {
                if (e.style.display == 'none') {
                    e.style.display = '';
                } else { e.style.display = 'none'; }
            });
            return this;
        },

        toggleClass: function (className) {
            //Add or remove one or more classes from each element in the set of matched elements, depending on either the class' presence or the value of the state argument
            let obj = getObj(className);
            if (typeof obj == 'string') {
                obj = obj.split(' ');
            }
            if (isType(obj, 5)) {
                this.each(function (e) {
                    let c = e.className;
                    let b = -1;
                    if (c != null && c != '') {
                        c = c.split(' ');
                        //array of class names
                        for (var x = 0; x < obj.length; x++) {
                            var a = obj[x];
                            b = c.indexOf(a);
                            if (b >= 0) {
                                //remove class
                                c.splice(b, 1);
                            } else {
                                //add class
                                c.push(a);
                            }
                        }
                        //update element className attr
                        e.className = c.join(' ');
                    } else {
                        e.className = className;
                    }
                });
            }
        },

        val: function (value) {
            //Get the current value of the first element in the set of matched elements or set the value of every matched element
            if (value != null) {
                this.each(function (a) {
                    a.value = value;
                });
            } else {
                if (this.length > 0) {
                    return this[0].value;
                }
                return '';
            }
            return this;
        },

        width: function (val) {
            //Get the current computed width for the first element in the set of matched elements or set the width of every matched element
            let obj = getObj(val);
            if (isType(obj, 1)) {
                const n = parseFloat(obj);
                if (!isNaN(n)) { obj = n; } else {
                    //width is string
                    this.each(function (e) {
                        if (e != window && e != document) {
                            e.style.width = obj;
                        }
                    });
                    return this;
                }
            } else if (obj == null) {
                if (this.length > 0) {
                    //get width from first element
                    const elem = this[0];
                    if (elem == window) {
                        return window.innerWidth;
                    } else if (elem == document) {
                        const body = document.body;
                        const html = document.documentElement;
                        return Math.max(
                            body.offsetWidth,
                            body.scrollWidth,
                            html.clientWidth,
                            html.offsetWidth,
                            html.scrollWidth
                        );
                    } else {
                        return elem.clientWidth;
                    }
                }
            } else {
                //width is a number
                if (obj == 0) {
                    this.each(function (e) {
                        e.style.width = 0;
                    });
                } else {
                    this.each(function (e) {
                        e.style.width = obj + 'px';
                    });
                }
            }
            return this;
        },

        //TODO: wrap
        wrap: function (elem) {
            //Wrap an HTML structure around each element in the set of matched elements
            return this;
        },

        //TODO: wrapAll
        wrapAll: function (elem) {
            //Wrap an HTML structure around all elements in the set of matched elements
            return this;
        },

        //TODO: wrapInner
        wrapInner: function (elem) {
            //Wrap an HTML structure around the content of each element in the set of matched elements
            return this;
        }
    };

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // create public selector object //////////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** @noalias */
    window['$'] = function(selector) {
        return new select(selector);
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // add functionality to the $ object //////////////////////////////////////////////////////////////////////////////
    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /** @noalias */
    window['$'].ajax = function () {
        let args = normalizeArgs([
            { t: 1, o: tru }, //0: url = string (optional)
            { t: 4, o: tru }, //1: settings = object (optional)
        ], arguments);
        args[1] = getObj(args[1]);
        var opt = args[1] || { url: args[0] };
        opt.url = args[0] || opt.url;
        opt.async = opt.async || tru;
        opt.cache = opt.cache || fals;
        opt.contentType = opt.contentType || 'application/x-www-form-urlencoded; charset=UTF-8';
        opt.data = opt.data || '';
        opt.dataType = opt.dataType || '';
        opt.method = opt.method || "GET";
        opt.type = opt.method || opt.type;

        //set up AJAX request
        var req = new XMLHttpRequest();

        //set up callbacks
        req.onload = function () {
            if (req.status >= 200 && req.status < 400) {
                //request success
                let resp = req.responseText;
                if (opt.dataType.toLowerCase() == "json") {
                    resp = JSON.parse(resp);
                }
                if (opt.success) {
                    opt.success(resp, req.statusText, req);
                }
            } else {
                //connected to server, but returned an error
                if (opt.error) {
                    opt.error(req, req.statusText);
                }
            }
            ajaxWait = false;
            runAjaxQueue();
        };

        req.onerror = function () {
            //an error occurred before connecting to server
            if (opt.error) {
                opt.error(req, req.statusText);
            }
            ajaxWait = false;
            runAjaxQueue();
        };

        if (opt.beforeSend) {
            if (opt.beforeSend(req, opt) == fals) {
                //canceled ajax call before sending
                return fals;
            }
        }

        //finally, add AJAX request to queue
        ajaxQueue.unshift({ req: req, opt: opt });
        runAjaxQueue();
    }

    function runAjaxQueue() {
        //run next request in the queue
        if (ajaxWait == true) { return; }
        if (ajaxQueue.length == 0) { return; }
        ajaxWait = true;
        let queue = ajaxQueue[ajaxQueue.length - 1];
        let req = queue.req;
        let opt = queue.opt;

        //remove from queue
        ajaxQueue.pop();

        //run AJAX request from queue
        req.open(opt.method, opt.url, opt.async, opt.username, opt.password);
        req.setRequestHeader('Content-Type', opt.contentType);
        req.send(opt.data);
    }

    /** @noalias */
    /**
     * @param {...string|boolean} var_args
     */
    window['$'].extend = function (var_args) {
        let extended = {};
        let deep = fals;
        let i = 0;
        const length = arguments.length;

        // Check if a deep merge
        if (Object.prototype.toString.call(arguments[0]) === '[object Boolean]') {
            deep = arguments[0];
            i++;
        }

        // Merge the object into the extended object
        const merge = function (obj) {
            for (let prop in obj) {
                if (Object.prototype.hasOwnProperty.call(obj, prop)) {
                    // If deep merge and property is an object, merge properties
                    if (deep && Object.prototype.toString.call(obj[prop]) === '[object Object]') {
                        extended[prop] = window['$'].extend(1, extended[prop], obj[prop]);
                    } else {
                        extended[prop] = obj[prop];
                    }
                }
            }
        };

        // Loop through each object and conduct a merge
        for (; i < length; i++) {
            let obj = arguments[i];
            merge(obj);
        }

        return extended;

    };

})();
/*! VelocityJS.org (1.3.1). (C) 2014 Julian Shapiro. MIT @license: en.wikipedia.org/wiki/MIT_License */
/*! VelocityJS.org jQuery Shim (1.0.1). (C) 2014 The jQuery Foundation. MIT @license: en.wikipedia.org/wiki/MIT_License. */
!function (a) { "use strict"; function b(a) { var b = a.length, d = c.type(a); return "function" !== d && !c.isWindow(a) && (!(1 !== a.nodeType || !b) || ("array" === d || 0 === b || "number" == typeof b && b > 0 && b - 1 in a)) } if (!a.jQuery) { var c = function (a, b) { return new c.fn.init(a, b) }; c.isWindow = function (a) { return a && a === a.window }, c.type = function (a) { return a ? "object" == typeof a || "function" == typeof a ? e[g.call(a)] || "object" : typeof a : a + "" }, c.isArray = Array.isArray || function (a) { return "array" === c.type(a) }, c.isPlainObject = function (a) { var b; if (!a || "object" !== c.type(a) || a.nodeType || c.isWindow(a)) return !1; try { if (a.constructor && !f.call(a, "constructor") && !f.call(a.constructor.prototype, "isPrototypeOf")) return !1 } catch (d) { return !1 } for (b in a); return void 0 === b || f.call(a, b) }, c.each = function (a, c, d) { var e, f = 0, g = a.length, h = b(a); if (d) { if (h) for (; f < g && (e = c.apply(a[f], d), e !== !1) ; f++); else for (f in a) if (a.hasOwnProperty(f) && (e = c.apply(a[f], d), e === !1)) break } else if (h) for (; f < g && (e = c.call(a[f], f, a[f]), e !== !1) ; f++); else for (f in a) if (a.hasOwnProperty(f) && (e = c.call(a[f], f, a[f]), e === !1)) break; return a }, c.data = function (a, b, e) { if (void 0 === e) { var f = a[c.expando], g = f && d[f]; if (void 0 === b) return g; if (g && b in g) return g[b] } else if (void 0 !== b) { var h = a[c.expando] || (a[c.expando] = ++c.uuid); return d[h] = d[h] || {}, d[h][b] = e, e } }, c.removeData = function (a, b) { var e = a[c.expando], f = e && d[e]; f && (b ? c.each(b, function (a, b) { delete f[b] }) : delete d[e]) }, c.extend = function () { var a, b, d, e, f, g, h = arguments[0] || {}, i = 1, j = arguments.length, k = !1; for ("boolean" == typeof h && (k = h, h = arguments[i] || {}, i++), "object" != typeof h && "function" !== c.type(h) && (h = {}), i === j && (h = this, i--) ; i < j; i++) if (f = arguments[i]) for (e in f) f.hasOwnProperty(e) && (a = h[e], d = f[e], h !== d && (k && d && (c.isPlainObject(d) || (b = c.isArray(d))) ? (b ? (b = !1, g = a && c.isArray(a) ? a : []) : g = a && c.isPlainObject(a) ? a : {}, h[e] = c.extend(k, g, d)) : void 0 !== d && (h[e] = d))); return h }, c.queue = function (a, d, e) { function f(a, c) { var d = c || []; return a && (b(Object(a)) ? !function (a, b) { for (var c = +b.length, d = 0, e = a.length; d < c;) a[e++] = b[d++]; if (c !== c) for (; void 0 !== b[d];) a[e++] = b[d++]; return a.length = e, a }(d, "string" == typeof a ? [a] : a) : [].push.call(d, a)), d } if (a) { d = (d || "fx") + "queue"; var g = c.data(a, d); return e ? (!g || c.isArray(e) ? g = c.data(a, d, f(e)) : g.push(e), g) : g || [] } }, c.dequeue = function (a, b) { c.each(a.nodeType ? [a] : a, function (a, d) { b = b || "fx"; var e = c.queue(d, b), f = e.shift(); "inprogress" === f && (f = e.shift()), f && ("fx" === b && e.unshift("inprogress"), f.call(d, function () { c.dequeue(d, b) })) }) }, c.fn = c.prototype = { init: function (a) { if (a.nodeType) return this[0] = a, this; throw new Error("Not a DOM node.") }, offset: function () { var b = this[0].getBoundingClientRect ? this[0].getBoundingClientRect() : { top: 0, left: 0 }; return { top: b.top + (a.pageYOffset || document.scrollTop || 0) - (document.clientTop || 0), left: b.left + (a.pageXOffset || document.scrollLeft || 0) - (document.clientLeft || 0) } }, position: function () { function a(a) { for (var b = a.offsetParent || document; b && "html" !== b.nodeType.toLowerCase && "static" === b.style.position;) b = b.offsetParent; return b || document } var b = this[0], d = a(b), e = this.offset(), f = /^(?:body|html)$/i.test(d.nodeName) ? { top: 0, left: 0 } : c(d).offset(); return e.top -= parseFloat(b.style.marginTop) || 0, e.left -= parseFloat(b.style.marginLeft) || 0, d.style && (f.top += parseFloat(d.style.borderTopWidth) || 0, f.left += parseFloat(d.style.borderLeftWidth) || 0), { top: e.top - f.top, left: e.left - f.left } } }; var d = {}; c.expando = "velocity" + (new Date).getTime(), c.uuid = 0; for (var e = {}, f = e.hasOwnProperty, g = e.toString, h = "Boolean Number String Function Array Date RegExp Object Error".split(" "), i = 0; i < h.length; i++) e["[object " + h[i] + "]"] = h[i].toLowerCase(); c.fn.init.prototype = c.fn, a.Velocity = { Utilities: c } } }(window), function (a) { "use strict"; "object" == typeof module && "object" == typeof module.exports ? module.exports = a() : "function" == typeof define && define.amd ? define(a) : a() }(function () {
    "use strict"; return function (a, b, c, d) {
        function e(a) { for (var b = -1, c = a ? a.length : 0, d = []; ++b < c;) { var e = a[b]; e && d.push(e) } return d } function f(a) { return p.isWrapped(a) ? a = [].slice.call(a) : p.isNode(a) && (a = [a]), a } function g(a) { var b = m.data(a, "velocity"); return null === b ? d : b } function h(a) { return function (b) { return Math.round(b * a) * (1 / a) } } function i(a, c, d, e) { function f(a, b) { return 1 - 3 * b + 3 * a } function g(a, b) { return 3 * b - 6 * a } function h(a) { return 3 * a } function i(a, b, c) { return ((f(b, c) * a + g(b, c)) * a + h(b)) * a } function j(a, b, c) { return 3 * f(b, c) * a * a + 2 * g(b, c) * a + h(b) } function k(b, c) { for (var e = 0; e < p; ++e) { var f = j(c, a, d); if (0 === f) return c; var g = i(c, a, d) - b; c -= g / f } return c } function l() { for (var b = 0; b < t; ++b) x[b] = i(b * u, a, d) } function m(b, c, e) { var f, g, h = 0; do g = c + (e - c) / 2, f = i(g, a, d) - b, f > 0 ? e = g : c = g; while (Math.abs(f) > r && ++h < s); return g } function n(b) { for (var c = 0, e = 1, f = t - 1; e !== f && x[e] <= b; ++e) c += u; --e; var g = (b - x[e]) / (x[e + 1] - x[e]), h = c + g * u, i = j(h, a, d); return i >= q ? k(b, h) : 0 === i ? h : m(b, c, c + u) } function o() { y = !0, a === c && d === e || l() } var p = 4, q = .001, r = 1e-7, s = 10, t = 11, u = 1 / (t - 1), v = "Float32Array" in b; if (4 !== arguments.length) return !1; for (var w = 0; w < 4; ++w) if ("number" != typeof arguments[w] || isNaN(arguments[w]) || !isFinite(arguments[w])) return !1; a = Math.min(a, 1), d = Math.min(d, 1), a = Math.max(a, 0), d = Math.max(d, 0); var x = v ? new Float32Array(t) : new Array(t), y = !1, z = function (b) { return y || o(), a === c && d === e ? b : 0 === b ? 0 : 1 === b ? 1 : i(n(b), c, e) }; z.getControlPoints = function () { return [{ x: a, y: c }, { x: d, y: e }] }; var A = "generateBezier(" + [a, c, d, e] + ")"; return z.toString = function () { return A }, z } function j(a, b) { var c = a; return p.isString(a) ? t.Easings[a] || (c = !1) : c = p.isArray(a) && 1 === a.length ? h.apply(null, a) : p.isArray(a) && 2 === a.length ? u.apply(null, a.concat([b])) : !(!p.isArray(a) || 4 !== a.length) && i.apply(null, a), c === !1 && (c = t.Easings[t.defaults.easing] ? t.defaults.easing : s), c } function k(a) { if (a) { var b = t.timestamp && a !== !0 ? a : (new Date).getTime(), c = t.State.calls.length; c > 1e4 && (t.State.calls = e(t.State.calls), c = t.State.calls.length); for (var f = 0; f < c; f++) if (t.State.calls[f]) { var h = t.State.calls[f], i = h[0], j = h[2], o = h[3], q = !!o, r = null; o || (o = t.State.calls[f][3] = b - 16); for (var s = Math.min((b - o) / j.duration, 1), u = 0, w = i.length; u < w; u++) { var y = i[u], z = y.element; if (g(z)) { var A = !1; if (j.display !== d && null !== j.display && "none" !== j.display) { if ("flex" === j.display) { var B = ["-webkit-box", "-moz-box", "-ms-flexbox", "-webkit-flex"]; m.each(B, function (a, b) { v.setPropertyValue(z, "display", b) }) } v.setPropertyValue(z, "display", j.display) } j.visibility !== d && "hidden" !== j.visibility && v.setPropertyValue(z, "visibility", j.visibility); for (var C in y) if (y.hasOwnProperty(C) && "element" !== C) { var D, E = y[C], F = p.isString(E.easing) ? t.Easings[E.easing] : E.easing; if (p.isString(E.pattern)) { var G = 1 === s ? function (a, b) { return E.endValue[b] } : function (a, b) { var c = E.startValue[b], d = E.endValue[b] - c; return c + d * F(s, j, d) }; D = E.pattern.replace(/{(\d+)}/g, G) } else if (1 === s) D = E.endValue; else { var H = E.endValue - E.startValue; D = E.startValue + H * F(s, j, H) } if (!q && D === E.currentValue) continue; if (E.currentValue = D, "tween" === C) r = D; else { var I; if (v.Hooks.registered[C]) { I = v.Hooks.getRoot(C); var J = g(z).rootPropertyValueCache[I]; J && (E.rootPropertyValue = J) } var K = v.setPropertyValue(z, C, E.currentValue + (n < 9 && 0 === parseFloat(D) ? "" : E.unitType), E.rootPropertyValue, E.scrollData); v.Hooks.registered[C] && (v.Normalizations.registered[I] ? g(z).rootPropertyValueCache[I] = v.Normalizations.registered[I]("extract", null, K[1]) : g(z).rootPropertyValueCache[I] = K[1]), "transform" === K[0] && (A = !0) } } j.mobileHA && g(z).transformCache.translate3d === d && (g(z).transformCache.translate3d = "(0px, 0px, 0px)", A = !0), A && v.flushTransformCache(z) } } j.display !== d && "none" !== j.display && (t.State.calls[f][2].display = !1), j.visibility !== d && "hidden" !== j.visibility && (t.State.calls[f][2].visibility = !1), j.progress && j.progress.call(h[1], h[1], s, Math.max(0, o + j.duration - b), o, r), 1 === s && l(f) } } t.State.isTicking && x(k) } function l(a, b) { if (!t.State.calls[a]) return !1; for (var c = t.State.calls[a][0], e = t.State.calls[a][1], f = t.State.calls[a][2], h = t.State.calls[a][4], i = !1, j = 0, k = c.length; j < k; j++) { var l = c[j].element; b || f.loop || ("none" === f.display && v.setPropertyValue(l, "display", f.display), "hidden" === f.visibility && v.setPropertyValue(l, "visibility", f.visibility)); var n = g(l); if (f.loop !== !0 && (m.queue(l)[1] === d || !/\.velocityQueueEntryFlag/i.test(m.queue(l)[1])) && n) { n.isAnimating = !1, n.rootPropertyValueCache = {}; var o = !1; m.each(v.Lists.transforms3D, function (a, b) { var c = /^scale/.test(b) ? 1 : 0, e = n.transformCache[b]; n.transformCache[b] !== d && new RegExp("^\\(" + c + "[^.]").test(e) && (o = !0, delete n.transformCache[b]) }), f.mobileHA && (o = !0, delete n.transformCache.translate3d), o && v.flushTransformCache(l), v.Values.removeClass(l, "velocity-animating") } if (!b && f.complete && !f.loop && j === k - 1) try { f.complete.call(e, e) } catch (p) { setTimeout(function () { throw p }, 1) } h && f.loop !== !0 && h(e), n && f.loop === !0 && !b && (m.each(n.tweensContainer, function (a, b) { if (/^rotate/.test(a) && (parseFloat(b.startValue) - parseFloat(b.endValue)) % 360 === 0) { var c = b.startValue; b.startValue = b.endValue, b.endValue = c } /^backgroundPosition/.test(a) && 100 === parseFloat(b.endValue) && "%" === b.unitType && (b.endValue = 0, b.startValue = 100) }), t(l, "reverse", { loop: !0, delay: f.delay })), f.queue !== !1 && m.dequeue(l, f.queue) } t.State.calls[a] = !1; for (var q = 0, r = t.State.calls.length; q < r; q++) if (t.State.calls[q] !== !1) { i = !0; break } i === !1 && (t.State.isTicking = !1, delete t.State.calls, t.State.calls = []) } var m, n = function () { if (c.documentMode) return c.documentMode; for (var a = 7; a > 4; a--) { var b = c.createElement("div"); if (b.innerHTML = "<!--[if IE " + a + "]><span></span><![endif]-->", b.getElementsByTagName("span").length) return b = null, a } return d }(), o = function () { var a = 0; return b.webkitRequestAnimationFrame || b.mozRequestAnimationFrame || function (b) { var c, d = (new Date).getTime(); return c = Math.max(0, 16 - (d - a)), a = d + c, setTimeout(function () { b(d + c) }, c) } }(), p = { isNumber: function (a) { return "number" == typeof a }, isString: function (a) { return "string" == typeof a }, isArray: Array.isArray || function (a) { return "[object Array]" === Object.prototype.toString.call(a) }, isFunction: function (a) { return "[object Function]" === Object.prototype.toString.call(a) }, isNode: function (a) { return a && a.nodeType }, isNodeList: function (a) { return "object" == typeof a && /^\[object (HTMLCollection|NodeList|Object)\]$/.test(Object.prototype.toString.call(a)) && a.length !== d && (0 === a.length || "object" == typeof a[0] && a[0].nodeType > 0) }, isWrapped: function (a) { return a && (p.isArray(a) || p.isNumber(a.length) && !p.isString(a) && !p.isFunction(a)) }, isSVG: function (a) { return b.SVGElement && a instanceof b.SVGElement }, isEmptyObject: function (a) { for (var b in a) if (a.hasOwnProperty(b)) return !1; return !0 } }, q = !1; if (a.fn && a.fn.jquery ? (m = a, q = !0) : m = b.Velocity.Utilities, n <= 8 && !q) throw new Error("Velocity: IE8 and below require jQuery to be loaded before Velocity."); if (n <= 7) return void (jQuery.fn.velocity = jQuery.fn.animate); var r = 400, s = "swing", t = { State: { isMobile: /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent), isAndroid: /Android/i.test(navigator.userAgent), isGingerbread: /Android 2\.3\.[3-7]/i.test(navigator.userAgent), isChrome: b.chrome, isFirefox: /Firefox/i.test(navigator.userAgent), prefixElement: c.createElement("div"), prefixMatches: {}, scrollAnchor: null, scrollPropertyLeft: null, scrollPropertyTop: null, isTicking: !1, calls: [] }, CSS: {}, Utilities: m, Redirects: {}, Easings: {}, Promise: b.Promise, defaults: { queue: "", duration: r, easing: s, begin: d, complete: d, progress: d, display: d, visibility: d, loop: !1, delay: !1, mobileHA: !0, _cacheValues: !0, promiseRejectEmpty: !0 }, init: function (a) { m.data(a, "velocity", { isSVG: p.isSVG(a), isAnimating: !1, computedStyle: null, tweensContainer: null, rootPropertyValueCache: {}, transformCache: {} }) }, hook: null, mock: !1, version: { major: 1, minor: 3, patch: 1 }, debug: !1, timestamp: !0 }; b.pageYOffset !== d ? (t.State.scrollAnchor = b, t.State.scrollPropertyLeft = "pageXOffset", t.State.scrollPropertyTop = "pageYOffset") : (t.State.scrollAnchor = c.documentElement || c.body.parentNode || c.body, t.State.scrollPropertyLeft = "scrollLeft", t.State.scrollPropertyTop = "scrollTop"); var u = function () { function a(a) { return -a.tension * a.x - a.friction * a.v } function b(b, c, d) { var e = { x: b.x + d.dx * c, v: b.v + d.dv * c, tension: b.tension, friction: b.friction }; return { dx: e.v, dv: a(e) } } function c(c, d) { var e = { dx: c.v, dv: a(c) }, f = b(c, .5 * d, e), g = b(c, .5 * d, f), h = b(c, d, g), i = 1 / 6 * (e.dx + 2 * (f.dx + g.dx) + h.dx), j = 1 / 6 * (e.dv + 2 * (f.dv + g.dv) + h.dv); return c.x = c.x + i * d, c.v = c.v + j * d, c } return function d(a, b, e) { var f, g, h, i = { x: -1, v: 0, tension: null, friction: null }, j = [0], k = 0, l = 1e-4, m = .016; for (a = parseFloat(a) || 500, b = parseFloat(b) || 20, e = e || null, i.tension = a, i.friction = b, f = null !== e, f ? (k = d(a, b), g = k / e * m) : g = m; ;) if (h = c(h || i, g), j.push(1 + h.x), k += 16, !(Math.abs(h.x) > l && Math.abs(h.v) > l)) break; return f ? function (a) { return j[a * (j.length - 1) | 0] } : k } }(); t.Easings = { linear: function (a) { return a }, swing: function (a) { return .5 - Math.cos(a * Math.PI) / 2 }, spring: function (a) { return 1 - Math.cos(4.5 * a * Math.PI) * Math.exp(6 * -a) } }, m.each([["ease", [.25, .1, .25, 1]], ["ease-in", [.42, 0, 1, 1]], ["ease-out", [0, 0, .58, 1]], ["ease-in-out", [.42, 0, .58, 1]], ["easeInSine", [.47, 0, .745, .715]], ["easeOutSine", [.39, .575, .565, 1]], ["easeInOutSine", [.445, .05, .55, .95]], ["easeInQuad", [.55, .085, .68, .53]], ["easeOutQuad", [.25, .46, .45, .94]], ["easeInOutQuad", [.455, .03, .515, .955]], ["easeInCubic", [.55, .055, .675, .19]], ["easeOutCubic", [.215, .61, .355, 1]], ["easeInOutCubic", [.645, .045, .355, 1]], ["easeInQuart", [.895, .03, .685, .22]], ["easeOutQuart", [.165, .84, .44, 1]], ["easeInOutQuart", [.77, 0, .175, 1]], ["easeInQuint", [.755, .05, .855, .06]], ["easeOutQuint", [.23, 1, .32, 1]], ["easeInOutQuint", [.86, 0, .07, 1]], ["easeInExpo", [.95, .05, .795, .035]], ["easeOutExpo", [.19, 1, .22, 1]], ["easeInOutExpo", [1, 0, 0, 1]], ["easeInCirc", [.6, .04, .98, .335]], ["easeOutCirc", [.075, .82, .165, 1]], ["easeInOutCirc", [.785, .135, .15, .86]]], function (a, b) { t.Easings[b[0]] = i.apply(null, b[1]) }); var v = t.CSS = { RegEx: { isHex: /^#([A-f\d]{3}){1,2}$/i, valueUnwrap: /^[A-z]+\((.*)\)$/i, wrappedValueAlreadyExtracted: /[0-9.]+ [0-9.]+ [0-9.]+( [0-9.]+)?/, valueSplit: /([A-z]+\(.+\))|(([A-z0-9#-.]+?)(?=\s|$))/gi }, Lists: { colors: ["fill", "stroke", "stopColor", "color", "backgroundColor", "borderColor", "borderTopColor", "borderRightColor", "borderBottomColor", "borderLeftColor", "outlineColor"], transformsBase: ["translateX", "translateY", "scale", "scaleX", "scaleY", "skewX", "skewY", "rotateZ"], transforms3D: ["transformPerspective", "translateZ", "scaleZ", "rotateX", "rotateY"] }, Hooks: { templates: { textShadow: ["Color X Y Blur", "black 0px 0px 0px"], boxShadow: ["Color X Y Blur Spread", "black 0px 0px 0px 0px"], clip: ["Top Right Bottom Left", "0px 0px 0px 0px"], backgroundPosition: ["X Y", "0% 0%"], transformOrigin: ["X Y Z", "50% 50% 0px"], perspectiveOrigin: ["X Y", "50% 50%"] }, registered: {}, register: function () { for (var a = 0; a < v.Lists.colors.length; a++) { var b = "color" === v.Lists.colors[a] ? "0 0 0 1" : "255 255 255 1"; v.Hooks.templates[v.Lists.colors[a]] = ["Red Green Blue Alpha", b] } var c, d, e; if (n) for (c in v.Hooks.templates) if (v.Hooks.templates.hasOwnProperty(c)) { d = v.Hooks.templates[c], e = d[0].split(" "); var f = d[1].match(v.RegEx.valueSplit); "Color" === e[0] && (e.push(e.shift()), f.push(f.shift()), v.Hooks.templates[c] = [e.join(" "), f.join(" ")]) } for (c in v.Hooks.templates) if (v.Hooks.templates.hasOwnProperty(c)) { d = v.Hooks.templates[c], e = d[0].split(" "); for (var g in e) if (e.hasOwnProperty(g)) { var h = c + e[g], i = g; v.Hooks.registered[h] = [c, i] } } }, getRoot: function (a) { var b = v.Hooks.registered[a]; return b ? b[0] : a }, cleanRootPropertyValue: function (a, b) { return v.RegEx.valueUnwrap.test(b) && (b = b.match(v.RegEx.valueUnwrap)[1]), v.Values.isCSSNullValue(b) && (b = v.Hooks.templates[a][1]), b }, extractValue: function (a, b) { var c = v.Hooks.registered[a]; if (c) { var d = c[0], e = c[1]; return b = v.Hooks.cleanRootPropertyValue(d, b), b.toString().match(v.RegEx.valueSplit)[e] } return b }, injectValue: function (a, b, c) { var d = v.Hooks.registered[a]; if (d) { var e, f, g = d[0], h = d[1]; return c = v.Hooks.cleanRootPropertyValue(g, c), e = c.toString().match(v.RegEx.valueSplit), e[h] = b, f = e.join(" ") } return c } }, Normalizations: { registered: { clip: function (a, b, c) { switch (a) { case "name": return "clip"; case "extract": var d; return v.RegEx.wrappedValueAlreadyExtracted.test(c) ? d = c : (d = c.toString().match(v.RegEx.valueUnwrap), d = d ? d[1].replace(/,(\s+)?/g, " ") : c), d; case "inject": return "rect(" + c + ")" } }, blur: function (a, b, c) { switch (a) { case "name": return t.State.isFirefox ? "filter" : "-webkit-filter"; case "extract": var d = parseFloat(c); if (!d && 0 !== d) { var e = c.toString().match(/blur\(([0-9]+[A-z]+)\)/i); d = e ? e[1] : 0 } return d; case "inject": return parseFloat(c) ? "blur(" + c + ")" : "none" } }, opacity: function (a, b, c) { if (n <= 8) switch (a) { case "name": return "filter"; case "extract": var d = c.toString().match(/alpha\(opacity=(.*)\)/i); return c = d ? d[1] / 100 : 1; case "inject": return b.style.zoom = 1, parseFloat(c) >= 1 ? "" : "alpha(opacity=" + parseInt(100 * parseFloat(c), 10) + ")" } else switch (a) { case "name": return "opacity"; case "extract": return c; case "inject": return c } } }, register: function () { function a(a, b, c) { var d = "border-box" === v.getPropertyValue(b, "boxSizing").toString().toLowerCase(); if (d === (c || !1)) { var e, f, g = 0, h = "width" === a ? ["Left", "Right"] : ["Top", "Bottom"], i = ["padding" + h[0], "padding" + h[1], "border" + h[0] + "Width", "border" + h[1] + "Width"]; for (e = 0; e < i.length; e++) f = parseFloat(v.getPropertyValue(b, i[e])), isNaN(f) || (g += f); return c ? -g : g } return 0 } function b(b, c) { return function (d, e, f) { switch (d) { case "name": return b; case "extract": return parseFloat(f) + a(b, e, c); case "inject": return parseFloat(f) - a(b, e, c) + "px" } } } n && !(n > 9) || t.State.isGingerbread || (v.Lists.transformsBase = v.Lists.transformsBase.concat(v.Lists.transforms3D)); for (var c = 0; c < v.Lists.transformsBase.length; c++) !function () { var a = v.Lists.transformsBase[c]; v.Normalizations.registered[a] = function (b, c, e) { switch (b) { case "name": return "transform"; case "extract": return g(c) === d || g(c).transformCache[a] === d ? /^scale/i.test(a) ? 1 : 0 : g(c).transformCache[a].replace(/[()]/g, ""); case "inject": var f = !1; switch (a.substr(0, a.length - 1)) { case "translate": f = !/(%|px|em|rem|vw|vh|\d)$/i.test(e); break; case "scal": case "scale": t.State.isAndroid && g(c).transformCache[a] === d && e < 1 && (e = 1), f = !/(\d)$/i.test(e); break; case "skew": f = !/(deg|\d)$/i.test(e); break; case "rotate": f = !/(deg|\d)$/i.test(e) } return f || (g(c).transformCache[a] = "(" + e + ")"), g(c).transformCache[a] } } }(); for (var e = 0; e < v.Lists.colors.length; e++) !function () { var a = v.Lists.colors[e]; v.Normalizations.registered[a] = function (b, c, e) { switch (b) { case "name": return a; case "extract": var f; if (v.RegEx.wrappedValueAlreadyExtracted.test(e)) f = e; else { var g, h = { black: "rgb(0, 0, 0)", blue: "rgb(0, 0, 255)", gray: "rgb(128, 128, 128)", green: "rgb(0, 128, 0)", red: "rgb(255, 0, 0)", white: "rgb(255, 255, 255)" }; /^[A-z]+$/i.test(e) ? g = h[e] !== d ? h[e] : h.black : v.RegEx.isHex.test(e) ? g = "rgb(" + v.Values.hexToRgb(e).join(" ") + ")" : /^rgba?\(/i.test(e) || (g = h.black), f = (g || e).toString().match(v.RegEx.valueUnwrap)[1].replace(/,(\s+)?/g, " ") } return (!n || n > 8) && 3 === f.split(" ").length && (f += " 1"), f; case "inject": return /^rgb/.test(e) ? e : (n <= 8 ? 4 === e.split(" ").length && (e = e.split(/\s+/).slice(0, 3).join(" ")) : 3 === e.split(" ").length && (e += " 1"), (n <= 8 ? "rgb" : "rgba") + "(" + e.replace(/\s+/g, ",").replace(/\.(\d)+(?=,)/g, "") + ")") } } }(); v.Normalizations.registered.innerWidth = b("width", !0), v.Normalizations.registered.innerHeight = b("height", !0), v.Normalizations.registered.outerWidth = b("width"), v.Normalizations.registered.outerHeight = b("height") } }, Names: { camelCase: function (a) { return a.replace(/-(\w)/g, function (a, b) { return b.toUpperCase() }) }, SVGAttribute: function (a) { var b = "width|height|x|y|cx|cy|r|rx|ry|x1|x2|y1|y2"; return (n || t.State.isAndroid && !t.State.isChrome) && (b += "|transform"), new RegExp("^(" + b + ")$", "i").test(a) }, prefixCheck: function (a) { if (t.State.prefixMatches[a]) return [t.State.prefixMatches[a], !0]; for (var b = ["", "Webkit", "Moz", "ms", "O"], c = 0, d = b.length; c < d; c++) { var e; if (e = 0 === c ? a : b[c] + a.replace(/^\w/, function (a) { return a.toUpperCase() }), p.isString(t.State.prefixElement.style[e])) return t.State.prefixMatches[a] = e, [e, !0] } return [a, !1] } }, Values: { hexToRgb: function (a) { var b, c = /^#?([a-f\d])([a-f\d])([a-f\d])$/i, d = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i; return a = a.replace(c, function (a, b, c, d) { return b + b + c + c + d + d }), b = d.exec(a), b ? [parseInt(b[1], 16), parseInt(b[2], 16), parseInt(b[3], 16)] : [0, 0, 0] }, isCSSNullValue: function (a) { return !a || /^(none|auto|transparent|(rgba\(0, ?0, ?0, ?0\)))$/i.test(a) }, getUnitType: function (a) { return /^(rotate|skew)/i.test(a) ? "deg" : /(^(scale|scaleX|scaleY|scaleZ|alpha|flexGrow|flexHeight|zIndex|fontWeight)$)|((opacity|red|green|blue|alpha)$)/i.test(a) ? "" : "px" }, getDisplayType: function (a) { var b = a && a.tagName.toString().toLowerCase(); return /^(b|big|i|small|tt|abbr|acronym|cite|code|dfn|em|kbd|strong|samp|var|a|bdo|br|img|map|object|q|script|span|sub|sup|button|input|label|select|textarea)$/i.test(b) ? "inline" : /^(li)$/i.test(b) ? "list-item" : /^(tr)$/i.test(b) ? "table-row" : /^(table)$/i.test(b) ? "table" : /^(tbody)$/i.test(b) ? "table-row-group" : "block" }, addClass: function (a, b) { if (a) if (a.classList) a.classList.add(b); else if (p.isString(a.className)) a.className += (a.className.length ? " " : "") + b; else { var c = a.getAttribute(n <= 7 ? "className" : "class") || ""; a.setAttribute("class", c + (c ? " " : "") + b) } }, removeClass: function (a, b) { if (a) if (a.classList) a.classList.remove(b); else if (p.isString(a.className)) a.className = a.className.toString().replace(new RegExp("(^|\\s)" + b.split(" ").join("|") + "(\\s|$)", "gi"), " "); else { var c = a.getAttribute(n <= 7 ? "className" : "class") || ""; a.setAttribute("class", c.replace(new RegExp("(^|s)" + b.split(" ").join("|") + "(s|$)", "gi"), " ")) } } }, getPropertyValue: function (a, c, e, f) { function h(a, c) { var e = 0; if (n <= 8) e = m.css(a, c); else { var i = !1; /^(width|height)$/.test(c) && 0 === v.getPropertyValue(a, "display") && (i = !0, v.setPropertyValue(a, "display", v.Values.getDisplayType(a))); var j = function () { i && v.setPropertyValue(a, "display", "none") }; if (!f) { if ("height" === c && "border-box" !== v.getPropertyValue(a, "boxSizing").toString().toLowerCase()) { var k = a.offsetHeight - (parseFloat(v.getPropertyValue(a, "borderTopWidth")) || 0) - (parseFloat(v.getPropertyValue(a, "borderBottomWidth")) || 0) - (parseFloat(v.getPropertyValue(a, "paddingTop")) || 0) - (parseFloat(v.getPropertyValue(a, "paddingBottom")) || 0); return j(), k } if ("width" === c && "border-box" !== v.getPropertyValue(a, "boxSizing").toString().toLowerCase()) { var l = a.offsetWidth - (parseFloat(v.getPropertyValue(a, "borderLeftWidth")) || 0) - (parseFloat(v.getPropertyValue(a, "borderRightWidth")) || 0) - (parseFloat(v.getPropertyValue(a, "paddingLeft")) || 0) - (parseFloat(v.getPropertyValue(a, "paddingRight")) || 0); return j(), l } } var o; o = g(a) === d ? b.getComputedStyle(a, null) : g(a).computedStyle ? g(a).computedStyle : g(a).computedStyle = b.getComputedStyle(a, null), "borderColor" === c && (c = "borderTopColor"), e = 9 === n && "filter" === c ? o.getPropertyValue(c) : o[c], "" !== e && null !== e || (e = a.style[c]), j() } if ("auto" === e && /^(top|right|bottom|left)$/i.test(c)) { var p = h(a, "position"); ("fixed" === p || "absolute" === p && /top|left/i.test(c)) && (e = m(a).position()[c] + "px") } return e } var i; if (v.Hooks.registered[c]) { var j = c, k = v.Hooks.getRoot(j); e === d && (e = v.getPropertyValue(a, v.Names.prefixCheck(k)[0])), v.Normalizations.registered[k] && (e = v.Normalizations.registered[k]("extract", a, e)), i = v.Hooks.extractValue(j, e) } else if (v.Normalizations.registered[c]) { var l, o; l = v.Normalizations.registered[c]("name", a), "transform" !== l && (o = h(a, v.Names.prefixCheck(l)[0]), v.Values.isCSSNullValue(o) && v.Hooks.templates[c] && (o = v.Hooks.templates[c][1])), i = v.Normalizations.registered[c]("extract", a, o) } if (!/^[\d-]/.test(i)) { var p = g(a); if (p && p.isSVG && v.Names.SVGAttribute(c)) if (/^(height|width)$/i.test(c)) try { i = a.getBBox()[c] } catch (q) { i = 0 } else i = a.getAttribute(c); else i = h(a, v.Names.prefixCheck(c)[0]) } return v.Values.isCSSNullValue(i) && (i = 0), t.debug >= 2 && console.log("Get " + c + ": " + i), i }, setPropertyValue: function (a, c, d, e, f) { var h = c; if ("scroll" === c) f.container ? f.container["scroll" + f.direction] = d : "Left" === f.direction ? b.scrollTo(d, f.alternateValue) : b.scrollTo(f.alternateValue, d); else if (v.Normalizations.registered[c] && "transform" === v.Normalizations.registered[c]("name", a)) v.Normalizations.registered[c]("inject", a, d), h = "transform", d = g(a).transformCache[c]; else { if (v.Hooks.registered[c]) { var i = c, j = v.Hooks.getRoot(c); e = e || v.getPropertyValue(a, j), d = v.Hooks.injectValue(i, d, e), c = j } if (v.Normalizations.registered[c] && (d = v.Normalizations.registered[c]("inject", a, d), c = v.Normalizations.registered[c]("name", a)), h = v.Names.prefixCheck(c)[0], n <= 8) try { a.style[h] = d } catch (k) { t.debug && console.log("Browser does not support [" + d + "] for [" + h + "]") } else { var l = g(a); l && l.isSVG && v.Names.SVGAttribute(c) ? a.setAttribute(c, d) : a.style[h] = d } t.debug >= 2 && console.log("Set " + c + " (" + h + "): " + d) } return [h, d] }, flushTransformCache: function (a) { var b = "", c = g(a); if ((n || t.State.isAndroid && !t.State.isChrome) && c && c.isSVG) { var d = function (b) { return parseFloat(v.getPropertyValue(a, b)) }, e = { translate: [d("translateX"), d("translateY")], skewX: [d("skewX")], skewY: [d("skewY")], scale: 1 !== d("scale") ? [d("scale"), d("scale")] : [d("scaleX"), d("scaleY")], rotate: [d("rotateZ"), 0, 0] }; m.each(g(a).transformCache, function (a) { /^translate/i.test(a) ? a = "translate" : /^scale/i.test(a) ? a = "scale" : /^rotate/i.test(a) && (a = "rotate"), e[a] && (b += a + "(" + e[a].join(" ") + ") ", delete e[a]) }) } else { var f, h; m.each(g(a).transformCache, function (c) { return f = g(a).transformCache[c], "transformPerspective" === c ? (h = f, !0) : (9 === n && "rotateZ" === c && (c = "rotate"), void (b += c + f + " ")) }), h && (b = "perspective" + h + " " + b) } v.setPropertyValue(a, "transform", b) } }; v.Hooks.register(), v.Normalizations.register(), t.hook = function (a, b, c) { var e; return a = f(a), m.each(a, function (a, f) { if (g(f) === d && t.init(f), c === d) e === d && (e = v.getPropertyValue(f, b)); else { var h = v.setPropertyValue(f, b, c); "transform" === h[0] && t.CSS.flushTransformCache(f), e = h } }), e }; var w = function () {
            function a() { return i ? y.promise || null : n } function e(a, e) {
                function f(f) { var n, o; if (i.begin && 0 === A) try { i.begin.call(q, q) } catch (r) { setTimeout(function () { throw r }, 1) } if ("scroll" === D) { var w, x, B, C = /^x$/i.test(i.axis) ? "Left" : "Top", E = parseFloat(i.offset) || 0; i.container ? p.isWrapped(i.container) || p.isNode(i.container) ? (i.container = i.container[0] || i.container, w = i.container["scroll" + C], B = w + m(a).position()[C.toLowerCase()] + E) : i.container = null : (w = t.State.scrollAnchor[t.State["scrollProperty" + C]], x = t.State.scrollAnchor[t.State["scrollProperty" + ("Left" === C ? "Top" : "Left")]], B = m(a).offset()[C.toLowerCase()] + E), l = { scroll: { rootPropertyValue: !1, startValue: w, currentValue: w, endValue: B, unitType: "", easing: i.easing, scrollData: { container: i.container, direction: C, alternateValue: x } }, element: a }, t.debug && console.log("tweensContainer (scroll): ", l.scroll, a) } else if ("reverse" === D) { if (n = g(a), !n) return; if (!n.tweensContainer) return void m.dequeue(a, i.queue); "none" === n.opts.display && (n.opts.display = "auto"), "hidden" === n.opts.visibility && (n.opts.visibility = "visible"), n.opts.loop = !1, n.opts.begin = null, n.opts.complete = null, u.easing || delete i.easing, u.duration || delete i.duration, i = m.extend({}, n.opts, i), o = m.extend(!0, {}, n ? n.tweensContainer : null); for (var F in o) if (o.hasOwnProperty(F) && "element" !== F) { var G = o[F].startValue; o[F].startValue = o[F].currentValue = o[F].endValue, o[F].endValue = G, p.isEmptyObject(u) || (o[F].easing = i.easing), t.debug && console.log("reverse tweensContainer (" + F + "): " + JSON.stringify(o[F]), a) } l = o } else if ("start" === D) { n = g(a), n && n.tweensContainer && n.isAnimating === !0 && (o = n.tweensContainer); var H = function (b, c) { var d, f, g; return p.isFunction(b) && (b = b.call(a, e, z)), p.isArray(b) ? (d = b[0], !p.isArray(b[1]) && /^[\d-]/.test(b[1]) || p.isFunction(b[1]) || v.RegEx.isHex.test(b[1]) ? g = b[1] : p.isString(b[1]) && !v.RegEx.isHex.test(b[1]) && t.Easings[b[1]] || p.isArray(b[1]) ? (f = c ? b[1] : j(b[1], i.duration), g = b[2]) : g = b[1] || b[2]) : d = b, c || (f = f || i.easing), p.isFunction(d) && (d = d.call(a, e, z)), p.isFunction(g) && (g = g.call(a, e, z)), [d || 0, f, g] }, K = function (e, f) { var g, j = v.Hooks.getRoot(e), k = !1, q = f[0], r = f[1], s = f[2]; if (!(n && n.isSVG || "tween" === j || v.Names.prefixCheck(j)[1] !== !1 || v.Normalizations.registered[j] !== d)) return void (t.debug && console.log("Skipping [" + j + "] due to a lack of browser support.")); (i.display !== d && null !== i.display && "none" !== i.display || i.visibility !== d && "hidden" !== i.visibility) && /opacity|filter/.test(e) && !s && 0 !== q && (s = 0), i._cacheValues && o && o[e] ? (s === d && (s = o[e].endValue + o[e].unitType), k = n.rootPropertyValueCache[j]) : v.Hooks.registered[e] ? s === d ? (k = v.getPropertyValue(a, j), s = v.getPropertyValue(a, e, k)) : k = v.Hooks.templates[j][1] : s === d && (s = v.getPropertyValue(a, e)); var u, w, x, y = !1, z = function (a, b) { var c, d; return d = (b || "0").toString().toLowerCase().replace(/[%A-z]+$/, function (a) { return c = a, "" }), c || (c = v.Values.getUnitType(a)), [d, c] }; if (p.isString(s) && p.isString(q)) { g = ""; for (var A = 0, B = 0, C = [], D = []; A < s.length && B < q.length;) { var E = s[A], F = q[B]; if (/[\d\.]/.test(E) && /[\d\.]/.test(F)) { for (var G = E, H = F, J = ".", K = "."; ++A < s.length;) { if (E = s[A], E === J) J = ".."; else if (!/\d/.test(E)) break; G += E } for (; ++B < q.length;) { if (F = q[B], F === K) K = ".."; else if (!/\d/.test(F)) break; H += F } G === H ? g += G : (g += "{" + C.length + "}", C.push(parseFloat(G)), D.push(parseFloat(H))) } else { if (E !== F) break; g += E, A++, B++ } } A === s.length && B === q.length || (t.debug && console.error('Trying to pattern match mis-matched strings ["' + q + '", "' + s + '"]'), g = d), g && (C.length ? (t.debug && console.log('Pattern found "' + g + '" -> ', C, D, s, q), s = C, q = D, w = x = "") : g = d) } g || (u = z(e, s), s = u[0], x = u[1], u = z(e, q), q = u[0].replace(/^([+-\/*])=/, function (a, b) { return y = b, "" }), w = u[1], s = parseFloat(s) || 0, q = parseFloat(q) || 0, "%" === w && (/^(fontSize|lineHeight)$/.test(e) ? (q /= 100, w = "em") : /^scale/.test(e) ? (q /= 100, w = "") : /(Red|Green|Blue)$/i.test(e) && (q = q / 100 * 255, w = ""))); var L = function () { var d = { myParent: a.parentNode || c.body, position: v.getPropertyValue(a, "position"), fontSize: v.getPropertyValue(a, "fontSize") }, e = d.position === I.lastPosition && d.myParent === I.lastParent, f = d.fontSize === I.lastFontSize; I.lastParent = d.myParent, I.lastPosition = d.position, I.lastFontSize = d.fontSize; var g = 100, h = {}; if (f && e) h.emToPx = I.lastEmToPx, h.percentToPxWidth = I.lastPercentToPxWidth, h.percentToPxHeight = I.lastPercentToPxHeight; else { var i = n && n.isSVG ? c.createElementNS("http://www.w3.org/2000/svg", "rect") : c.createElement("div"); t.init(i), d.myParent.appendChild(i), m.each(["overflow", "overflowX", "overflowY"], function (a, b) { t.CSS.setPropertyValue(i, b, "hidden") }), t.CSS.setPropertyValue(i, "position", d.position), t.CSS.setPropertyValue(i, "fontSize", d.fontSize), t.CSS.setPropertyValue(i, "boxSizing", "content-box"), m.each(["minWidth", "maxWidth", "width", "minHeight", "maxHeight", "height"], function (a, b) { t.CSS.setPropertyValue(i, b, g + "%") }), t.CSS.setPropertyValue(i, "paddingLeft", g + "em"), h.percentToPxWidth = I.lastPercentToPxWidth = (parseFloat(v.getPropertyValue(i, "width", null, !0)) || 1) / g, h.percentToPxHeight = I.lastPercentToPxHeight = (parseFloat(v.getPropertyValue(i, "height", null, !0)) || 1) / g, h.emToPx = I.lastEmToPx = (parseFloat(v.getPropertyValue(i, "paddingLeft")) || 1) / g, d.myParent.removeChild(i) } return null === I.remToPx && (I.remToPx = parseFloat(v.getPropertyValue(c.body, "fontSize")) || 16), null === I.vwToPx && (I.vwToPx = parseFloat(b.innerWidth) / 100, I.vhToPx = parseFloat(b.innerHeight) / 100), h.remToPx = I.remToPx, h.vwToPx = I.vwToPx, h.vhToPx = I.vhToPx, t.debug >= 1 && console.log("Unit ratios: " + JSON.stringify(h), a), h }; if (/[\/*]/.test(y)) w = x; else if (x !== w && 0 !== s) if (0 === q) w = x; else { h = h || L(); var M = /margin|padding|left|right|width|text|word|letter/i.test(e) || /X$/.test(e) || "x" === e ? "x" : "y"; switch (x) { case "%": s *= "x" === M ? h.percentToPxWidth : h.percentToPxHeight; break; case "px": break; default: s *= h[x + "ToPx"] } switch (w) { case "%": s *= 1 / ("x" === M ? h.percentToPxWidth : h.percentToPxHeight); break; case "px": break; default: s *= 1 / h[w + "ToPx"] } } switch (y) { case "+": q = s + q; break; case "-": q = s - q; break; case "*": q *= s; break; case "/": q = s / q } l[e] = { rootPropertyValue: k, startValue: s, currentValue: s, endValue: q, unitType: w, easing: r }, g && (l[e].pattern = g), t.debug && console.log("tweensContainer (" + e + "): " + JSON.stringify(l[e]), a) }; for (var L in s) if (s.hasOwnProperty(L)) { var M = v.Names.camelCase(L), N = H(s[L]); if (v.Lists.colors.indexOf(M) >= 0) { var O = N[0], P = N[1], Q = N[2]; if (v.RegEx.isHex.test(O)) { for (var R = ["Red", "Green", "Blue"], S = v.Values.hexToRgb(O), T = Q ? v.Values.hexToRgb(Q) : d, U = 0; U < R.length; U++) { var V = [S[U]]; P && V.push(P), T !== d && V.push(T[U]), K(M + R[U], V) } continue } } K(M, N) } l.element = a } l.element && (v.Values.addClass(a, "velocity-animating"), J.push(l), n = g(a), n && ("" === i.queue && (n.tweensContainer = l, n.opts = i), n.isAnimating = !0), A === z - 1 ? (t.State.calls.push([J, q, i, null, y.resolver]), t.State.isTicking === !1 && (t.State.isTicking = !0, k())) : A++) } var h, i = m.extend({}, t.defaults, u), l = {}; switch (g(a) === d && t.init(a), parseFloat(i.delay) && i.queue !== !1 && m.queue(a, i.queue, function (b) { t.velocityQueueEntryFlag = !0, g(a).delayTimer = { setTimeout: setTimeout(b, parseFloat(i.delay)), next: b } }), i.duration.toString().toLowerCase()) {
                    case "fast": i.duration = 200; break; case "normal": i.duration = r;
                        break; case "slow": i.duration = 600; break; default: i.duration = parseFloat(i.duration) || 1
                } t.mock !== !1 && (t.mock === !0 ? i.duration = i.delay = 1 : (i.duration *= parseFloat(t.mock) || 1, i.delay *= parseFloat(t.mock) || 1)), i.easing = j(i.easing, i.duration), i.begin && !p.isFunction(i.begin) && (i.begin = null), i.progress && !p.isFunction(i.progress) && (i.progress = null), i.complete && !p.isFunction(i.complete) && (i.complete = null), i.display !== d && null !== i.display && (i.display = i.display.toString().toLowerCase(), "auto" === i.display && (i.display = t.CSS.Values.getDisplayType(a))), i.visibility !== d && null !== i.visibility && (i.visibility = i.visibility.toString().toLowerCase()), i.mobileHA = i.mobileHA && t.State.isMobile && !t.State.isGingerbread, i.queue === !1 ? i.delay ? setTimeout(f, i.delay) : f() : m.queue(a, i.queue, function (a, b) { return b === !0 ? (y.promise && y.resolver(q), !0) : (t.velocityQueueEntryFlag = !0, void f(a)) }), "" !== i.queue && "fx" !== i.queue || "inprogress" === m.queue(a)[0] || m.dequeue(a)
            } var h, i, n, o, q, s, u, x = arguments[0] && (arguments[0].p || m.isPlainObject(arguments[0].properties) && !arguments[0].properties.names || p.isString(arguments[0].properties)); p.isWrapped(this) ? (i = !1, o = 0, q = this, n = this) : (i = !0, o = 1, q = x ? arguments[0].elements || arguments[0].e : arguments[0]); var y = { promise: null, resolver: null, rejecter: null }; if (i && t.Promise && (y.promise = new t.Promise(function (a, b) { y.resolver = a, y.rejecter = b })), x ? (s = arguments[0].properties || arguments[0].p, u = arguments[0].options || arguments[0].o) : (s = arguments[o], u = arguments[o + 1]), q = f(q), !q) return void (y.promise && (s && u && u.promiseRejectEmpty === !1 ? y.resolver() : y.rejecter())); var z = q.length, A = 0; if (!/^(stop|finish|finishAll)$/i.test(s) && !m.isPlainObject(u)) { var B = o + 1; u = {}; for (var C = B; C < arguments.length; C++) p.isArray(arguments[C]) || !/^(fast|normal|slow)$/i.test(arguments[C]) && !/^\d/.test(arguments[C]) ? p.isString(arguments[C]) || p.isArray(arguments[C]) ? u.easing = arguments[C] : p.isFunction(arguments[C]) && (u.complete = arguments[C]) : u.duration = arguments[C] } var D; switch (s) { case "scroll": D = "scroll"; break; case "reverse": D = "reverse"; break; case "finish": case "finishAll": case "stop": m.each(q, function (a, b) { g(b) && g(b).delayTimer && (clearTimeout(g(b).delayTimer.setTimeout), g(b).delayTimer.next && g(b).delayTimer.next(), delete g(b).delayTimer), "finishAll" !== s || u !== !0 && !p.isString(u) || (m.each(m.queue(b, p.isString(u) ? u : ""), function (a, b) { p.isFunction(b) && b() }), m.queue(b, p.isString(u) ? u : "", [])) }); var E = []; return m.each(t.State.calls, function (a, b) { b && m.each(b[1], function (c, e) { var f = u === d ? "" : u; return f !== !0 && b[2].queue !== f && (u !== d || b[2].queue !== !1) || void m.each(q, function (c, d) { if (d === e) if ((u === !0 || p.isString(u)) && (m.each(m.queue(d, p.isString(u) ? u : ""), function (a, b) { p.isFunction(b) && b(null, !0) }), m.queue(d, p.isString(u) ? u : "", [])), "stop" === s) { var h = g(d); h && h.tweensContainer && f !== !1 && m.each(h.tweensContainer, function (a, b) { b.endValue = b.currentValue }), E.push(a) } else "finish" !== s && "finishAll" !== s || (b[2].duration = 1) }) }) }), "stop" === s && (m.each(E, function (a, b) { l(b, !0) }), y.promise && y.resolver(q)), a(); default: if (!m.isPlainObject(s) || p.isEmptyObject(s)) { if (p.isString(s) && t.Redirects[s]) { h = m.extend({}, u); var F = h.duration, G = h.delay || 0; return h.backwards === !0 && (q = m.extend(!0, [], q).reverse()), m.each(q, function (a, b) { parseFloat(h.stagger) ? h.delay = G + parseFloat(h.stagger) * a : p.isFunction(h.stagger) && (h.delay = G + h.stagger.call(b, a, z)), h.drag && (h.duration = parseFloat(F) || (/^(callout|transition)/.test(s) ? 1e3 : r), h.duration = Math.max(h.duration * (h.backwards ? 1 - a / z : (a + 1) / z), .75 * h.duration, 200)), t.Redirects[s].call(b, b, h || {}, a, z, q, y.promise ? y : d) }), a() } var H = "Velocity: First argument (" + s + ") was not a property map, a known action, or a registered redirect. Aborting."; return y.promise ? y.rejecter(new Error(H)) : console.log(H), a() } D = "start" } var I = { lastParent: null, lastPosition: null, lastFontSize: null, lastPercentToPxWidth: null, lastPercentToPxHeight: null, lastEmToPx: null, remToPx: null, vwToPx: null, vhToPx: null }, J = []; m.each(q, function (a, b) { p.isNode(b) && e(b, a) }), h = m.extend({}, t.defaults, u), h.loop = parseInt(h.loop, 10); var K = 2 * h.loop - 1; if (h.loop) for (var L = 0; L < K; L++) { var M = { delay: h.delay, progress: h.progress }; L === K - 1 && (M.display = h.display, M.visibility = h.visibility, M.complete = h.complete), w(q, "reverse", M) } return a()
        }; t = m.extend(w, t), t.animate = w; var x = b.requestAnimationFrame || o; return t.State.isMobile || c.hidden === d || c.addEventListener("visibilitychange", function () { c.hidden ? (x = function (a) { return setTimeout(function () { a(!0) }, 16) }, k()) : x = b.requestAnimationFrame || o }), a.Velocity = t, a !== b && (a.fn.velocity = w, a.fn.velocity.defaults = t.defaults), m.each(["Down", "Up"], function (a, b) { t.Redirects["slide" + b] = function (a, c, e, f, g, h) { var i = m.extend({}, c), j = i.begin, k = i.complete, l = {}, n = { height: "", marginTop: "", marginBottom: "", paddingTop: "", paddingBottom: "" }; i.display === d && (i.display = "Down" === b ? "inline" === t.CSS.Values.getDisplayType(a) ? "inline-block" : "block" : "none"), i.begin = function () { 0 === e && j && j.call(g, g); for (var c in n) if (n.hasOwnProperty(c)) { l[c] = a.style[c]; var d = v.getPropertyValue(a, c); n[c] = "Down" === b ? [d, 0] : [0, d] } l.overflow = a.style.overflow, a.style.overflow = "hidden" }, i.complete = function () { for (var b in l) l.hasOwnProperty(b) && (a.style[b] = l[b]); e === f - 1 && (k && k.call(g, g), h && h.resolver(g)) }, t(a, n, i) } }), m.each(["In", "Out"], function (a, b) { t.Redirects["fade" + b] = function (a, c, e, f, g, h) { var i = m.extend({}, c), j = i.complete, k = { opacity: "In" === b ? 1 : 0 }; 0 !== e && (i.begin = null), e !== f - 1 ? i.complete = null : i.complete = function () { j && j.call(g, g), h && h.resolver(g) }, i.display === d && (i.display = "In" === b ? "auto" : "none"), t(this, k, i) } }), t
    }(window.jQuery || window.Zepto || window, window, window ? window.document : void 0)
});


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//Global "Super" object contains all neccessary functionality used on this web page //////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
S = {
    root: '' //where to load assets from (e.g. '/datasilk/js/')
};
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

$(window).on('resize', function () { S.window.changed = true; });
$(window).on('scroll', function () { S.window.changed = true; });



S.ajax = {
    //class used to make simple web service posts to the server
    queue: [],

    post: function (url, data, callback, error, json, opts) {
        var options = {
            method: "POST",
            data: JSON.stringify(data),
            url: '/api/' + url,
            contentType: "text/plain; charset=utf-8",
            dataType:'html',
            success: function (d) {
                var txt = '';
                if (typeof d.responseText != 'undefined') { txt = d.responseText; } else { txt = d;}
                S.ajax.runQueue(); 
                if(typeof callback == 'function'){callback(txt);}
            },
            error: function (xhr, status, err) {
                if (typeof error == 'function') { error(xhr, status, err); }
                S.ajax.runQueue();
            }
        }
        if (opts) {
            //user-specified options
            if (opts.contentType) { options.contentType = opts.contentType; }
            if (opts.method) { options.method = opts.method; }
            if (opts.url) { options.url = opts.url; }
        }
        if (json == true) { options.dataType = 'json'; }
        S.ajax.queue.push(options);
        if (S.ajax.queue.length == 1) {
            $.ajax(options);
        }
    },

    inject: function (response) {
        var elem = $(response.selector);
        if (elem.length > 0 && response.html != '') {
            switch (response.type) {
                case 0: //replace
                    elem.html(response.html);
                    break;
                case 1: //append
                    elem.append(response.html);
                    break;
                case 2: //before
                    elem.before(response.html);
                    break;
                case 3: //after
                    elem.after(response.html);
                    break;
            }
        }

        //add any CSS to the page
        if (response.css && response.css != '') {
            S.util.css.add(response.cssid, response.css);
        }

        //finally, execute callback javascript
        if (response.javascript && response.javascript != '') {
            var js = new Function(response.javascript);
            js();
        }
    },

    runQueue: function () {
        S.ajax.queue.shift();
        if (S.ajax.queue.length > 0) {
            $.ajax(S.ajax.queue[0]);
        }
    }
};
S.accordion = {
    load: function (options) {
        opts = {
            container: '.accordion',
            target: '.accordion > .title',
            ontoggle: null
        };

        if (options && options.container) { opts.container == options.container; }
        if (options && options.target) { opts.target == options.target; }
        if (options && options.ontoggle) { opts.ontoggle == options.ontoggle; }
        $('.accordion > .title').off('click').on('click', (e) => {
            S.accordion.toggle(e);
            if (typeof ontoggle == 'function') { ontoggle(e, $(e.target).hasClass('expanded')); }
        });
    },

    toggle: function (e) {
        $(e.target).parents('.accordion').first().toggleClass('expanded');
    }
};

S.accordion.load();
S.clipboard = {
    copy: function(str) {
        const el = document.createElement('textarea');
        el.value = str;
        document.body.appendChild(el);
        el.select();
        document.execCommand('copy');
        document.body.removeChild(el);
    }
}
S.loader = function (options) {
    var options = options ? options : {}
    var opts = {
        padding: options.padding ? options.padding : '',
        size: options.size ? options.size : ''
    };

    return '<div class="loader icon' +
        (opts.size != '' ? ' ' + opts.size : '') +
        '"' +
        (opts.padding ? ' style="padding:' + opts.padding + '"' : '') +
        '><svg viewBox="0 0 36 36">' +
        '<use xlink:href="#icon-loader" x="0" y="0" width="36" height="36"></use>' +
        '</svg></div>';
}
S.message = {
    show: function(element, type, msg, fadein, hideDelay, fadeout) {
        var types = 'error warning alert';
        var el = $(element);
        if (type != '' && type != null) {
            el.removeClass(types).addClass(type);
        } else {
            el.removeClass(types);
        }
        el.find('span').html(msg);
        if (fadein === true) {
            el.css({ opacity: 0, overflow:'hidden' }).show();
            var h = el.height();
            el.css({ height: 0, marginTop: 10, marginBottom: 10, paddingTop:0, paddingBottom:0 });
            el.animate({ opacity: 1, height: h, marginTop: 10, marginBottom: 10, paddingTop: 7, paddingBottom: 7 },
                { duration: 333, easing: 'easeInSine' });
        } else {
            el.css({ opacity: 1, height:'auto' }).show();
        }
        if (hideDelay != null) {
            setTimeout(function () {
                if (fadeout == true) {
                    el.animate({ opacity: 0 }, {
                        duration: 333,
                        complete: function () {
                            el.hide();
                        }
                    });
                } else { el.hide(); }
            }, hideDelay);
        }
    },

    error: {
        generic:'An error has occurred. Please contact support.'
    }
}
S.popup = {
    elem: null, options: null,

    show: function (title, html, options) {
        if (options == null) { options = {}; }
        var opts = {
            width: options.width != null ? options.width : 300,
            maxWidth: options.maxWidth != null ? options.maxWidth : null,
            padding: options.padding != null ? options.padding : 0,
            offsetHeight: options.offsetHeight != null ? options.offsetHeight : 0,
            offsetTop: options.offsetTop != null ? options.offsetTop : 0,
            position: options.position != null ? options.position : 'center',
            close: options.close != null ? options.close : true,
            className: options.className != null ? options.className : ''
        };
        this.options = opts;

        var win = S.window.pos();
        var div = document.createElement('div');
        var forpopup = $('body > .for-popup');
        var popup = $(div);
        div.className = 'popup box ' + opts.className;

        popup.css({ width: opts.width });
        if (opts.maxWidth != null) { popup.css({ maxWidth: opts.maxWidth }); }
        popup.addClass(opts.position);
        if (opts.offsetHeight > 0) {
            popup.css({ Marginbottom: opts.offsetHeight });
        }
        if (opts.offsetTop.toString().indexOf('%') > 0) {
            popup.css({ top: opts.offsetTop });
        } else if (Number(opts.offsetTop) == opts.offsetTop) {
            if (opts.offsetTop > 0) {
                popup.css({ top: win.scrolly + ((win.h - 300) / 3) + opts.offsetTop });
            }
        }
        if (opts.padding > 0) {
            forpopup.css({ padding: opts.padding });
        }

        var view = new S.view($('#template_popup').html(), {
            title: title,
            body: html
        }, "#", "#");
        popup.html(view.render());
        this.elem = popup;

        $('body > .for-popup .popup').remove();
        forpopup.removeClass('hide').append(div);

        //set up events
        $(window).on('resize', S.popup.resize);
        $(window).on('scroll', S.popup.resize);

        if (opts.close == true) {
            $('.popup .btn-close a').on('click', function () {
                S.popup.hide();
            });
        }

        S.popup.resize();
    },

    hide: function () {
        //remove events
        $('body > .for-popup').addClass('hide');
        $(window).off('resize', S.popup.resize);
        $(window).off('scroll', S.popup.resize);
    },

    bg: function (e) {
        if (e.target == $('.bg.for-popup')[0]) { S.popup.hide(); }
    },

    resize: function () {
        var win = S.window.pos();
        var pos = S.popup.elem.position();
        pos.height = S.popup.elem.height();
        S.popup.elem.css({ maxHeight: win.height - (S.popup.options.padding * 2), top: S.popup.options.offsetTop.toString().indexOf('%') > 0 ? S.popup.options.offsetTop : win.scrolly + ((win.h - pos.height) / 3) + S.popup.options.offsetTop });
    }
}
/*
example: var view = new S.view('<div>{{hello}}</div>', {hello:'world'});
         view.render();
*/
S.viewTags = { startChar: '{{', endChar: '}}' }; //overridable property

S.view = function (html, vars, tagStartChar, tagEndChar) {
    //tagStartChar & tagEndChar is optional, defines the symbols (#)
    //to use when searching for view variable placeholders
    this.html = html;
    this.vars = vars;
    if (tagStartChar) {
        this.tagStartChar = tagStartChar;
    } else { this.tagStartChar = S.viewTags.startChar; }
    if (tagEndChar) {
        this.tagEndChar = tagEndChar;
    } else { this.tagEndChar = S.viewTags.endChar; }
}

S.view.prototype.render = function () {
    var a = 0, b = 0, c = 0, d = 0;
    var tagslen = this.tagStartChar.length + this.tagEndChar.length;
    var endlen = this.tagEndChar.length;
    var htm = this.html;
    var ischanged = true;
    for (var key in this.vars) {
        ischanged = true;
        while (ischanged) {
            ischanged = false;
            //check for view closing first
            a = htm.indexOf(this.tagStartChar + '/' + key + this.tagEndChar);
            if (a >= 0) {
                //found a group of html to show or hide based on view element boolean value
                b = a + tagslen + key.length + 1;
                c = htm.indexOf(this.tagStartChar + key);
                d = htm.indexOf(this.tagEndChar, c + 1);
                if (c >= 0 && d > c) {
                    if (this.vars[key] === false) {
                        //hide group of html
                        htm = htm.substr(0, c) + htm.substr(b);
                        ischanged = true;
                    } else if (this.vars[key] === true) {
                        //show group of html
                        htm = htm.substr(0, c) + htm.substr(d + endlen, a - (d + endlen)) + htm.substr(b);
                        ischanged = true;
                    }
                    continue;
                }
            }
            //check for view element to replace with a value
            if (ischanged == false) {
                if (htm.indexOf(this.tagStartChar + key + this.tagEndChar) >= 0) {
                    htm = htm.replace(this.tagStartChar + key + this.tagEndChar, this.vars[key]);
                    ischanged = true;
                }
            }
        }
    }
    return htm;
}
S.svg = {
    load: function (url) {
        S.ajax.post('', {}, function (d) {
            $('.svgicons').append(d);
        }, null, false, { url: url, method: 'GET', contentType: 'image/svg' });
    }
};

S.upload = {
    file: function (file, url, onprogress, oncomplete, onerror) {
        const xhr = new XMLHttpRequest();
        const formdata = new FormData();

        xhr.open("POST", url, true);
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                if (xhr.status == 200) {
                    if (typeof oncomplete == 'function') {
                        oncomplete(xhr);
                    }
                } else if (xhr.status == 500) {
                    hasError();
                }
            }
        };
        xhr.onerror = function (e) {
            if(e.error != null) { hasError(e); }
        };

        function hasError() {
            if (typeof onerror == 'function') {
                onerror(xhr);
            }
        }
        xhr.upload.onprogress = onprogress;
        formdata.append('file', file);
        xhr.send(formdata);
    }
}
S.util = {
    js: {
        load: function (file, id, callback, error) {
            //add javascript file to DOM
            if (document.getElementById(id)) { if (callback) { callback(); } return false; }
            var body = document.body;
            var script = document.createElement('script');
            script.type = 'text/javascript';
            script.src = file;
            script.id = id;
            script.onload = callback;
            script.onerror = error;
            body.appendChild(script);
        }
    },
    css: {
        load: function (file, id) {
            //download CSS file and load onto the page
            if (document.getElementById(id)) { return false; }
            var head = document.getElementsByTagName('head')[0];
            var link = document.createElement('link');
            link.rel = 'stylesheet';
            link.type = 'text/css';
            link.href = file;
            link.id = id;
            head.appendChild(link);
        },

        add: function (id, css) {
            //add raw CSS to the page inside a style tag
            $('#' + id).remove();
            $('head').append('<style id="' + id + '" type="text/css">' + css + "</style>");
        },
    },
    str: {
        isNumeric: function (str) {
            return !isNaN(parseFloat(str)) && isFinite(str);
        },
        Capitalize: function (str) {
            var list = str.split(' ');
            for (var x = 0; x < list.length; x++) {
                list[x] = list[x].charAt(0).toUpperCase() + list[x].slice(1);
            }
            return list.join(' ');
        }
    },

    element: {
        getClassId: function (elem, prefix) {
            if (elem.length > 0) { elem = elem[0]; }
            if (elem.className.length <= 0) { return null; }
            if (prefix == null) { prefix = 'id-'; }
            var id = elem.className.split(' ').filter(function (c) { return c.indexOf(prefix) == 0; });
            if (id.length > 0) {
                return parseInt(id[0].replace(prefix, ''));
            }
            return null;
        }
    },

    location: {
        queryString: function (key, url) {
            if (!url) url = window.location.href;
            key = key.replace(/[\[\]]/g, "\\$&");
            var regex = new RegExp("[?&]" + key + "(=([^&#]*)|&|#|$)"),
                results = regex.exec(url);
            if (!results) return null;
            if (!results[2]) return '';
            return decodeURIComponent(results[2].replace(/\+/g, " "));
        }
    }
};

S.math = {
    intersect: function (a, b) {
        //checks to see if rect (a) intersects with rect (b)
        if (b.left < a.right && a.left < b.right && b.top < a.bottom) {
            return a.top < b.bottom;
        } else {
            return false;
        }
    },
    clamp: function (num, min, max) {
        return num <= min ? min : num >= max ? max : num;
    }
};

S.array = {
    indexOfProperty(array, propertyName, value) {
        for (var i = 0; i < array.length; i += 1) {
            if (array[i][propertyName]) {
                if (array[i][propertyName] === value) {
                    return i;
                }
            }
        }
        return -1;
    }
};

S.iframe = function (selector) {
    var iframe = $(selector)[0];
    var doc = null;
    if (iframe.contentDocument) { doc = iframe.contentDocument; }
    else if (iframe.contentWindow) { doc = iframe.contentWindow; }
    return {
        document: doc,
        window: iframe.contentWindow
    };
};



S.util.color = {
    rgbToHex: function (color) {
        function toHex(num) {
            return ('0' + parseInt(num).toString(16)).slice(-2);
        }
        var hex = '';
        if (color.indexOf('rgb') >= 0) {
            var c = color.replace('rgb', '').replace('a', '').replace('(', '').replace(')', '').replace(/\s/g, '').split(',');
            hex = "#" + toHex(c[0]) + toHex(c[1]) + toHex(c[2]);
        } else { hex = color; }
        return hex;
    },

    argbToInt: function (a, r, g, b) {
        return (a << 24) + (r << 16) + (g << 8) + (b);
    }
};
S.window = {
    w: 0, h: 0, scrollx: 0, scrolly: 0, z: 0, changed: true,

    pos: function (scrollOnly) {
        if (this.changed == false && !scrollOnly) { return this; }
        this.changed = false;
        var w = window;
        var e = document.documentElement;
        var b = document.body;

        //get window scroll x & y positions
        this.scrollx = w.scrollX;
        this.scrolly = w.scrollY;
        if (typeof this.scrollx == 'undefined') {
            this.scrollx = b.scrollLeft;
            this.scrolly = b.scrollTop;
            if (typeof this.scrollx == 'undefined') {
                this.scrollx = w.pageXOffset;
                this.scrolly = w.pageYOffset;
                if (typeof this.scrollx == 'undefined') {
                    this.z = GetZoomFactor();
                    this.scrollx = Math.round(e.scrollLeft / this.z);
                    this.scrolly = Math.round(e.scrollTop / this.z);
                }
            }
        }
        if (scrollOnly) { return this; } //no need to update width & height

        //get windows width & height
        this.w = w.innerWidth || e.clientWidth || b.clientWidth;
        this.h = w.innerHeight || e.clientHeight || b.clientHeight;
        return this;
    }
};
(function () {
    var defaults = {
        //default options
        dropTargets: [], //array of elements that files can be dropped into
        buttons: [], //array of buttons that opens a file dialog
        url: '', //URL to upload files to
        method: 'POST', //method to use when uploading
        multipleUploads: true, //allow user to select multiple files in the file dialog
        directoryDialog: false, //allow user to select a directory instead of files
        parallelUploads: 2, //total uploads to process at a given time
        maxFilesize: 1024 * 10, //bytes * megabytes
        fileTypes: '', //filter specific file extensions
        autoUpload: true, //uploads files once selected or dropped
        capture: false, //used to capture a file from a "media capture mechanism", such as a webcam

        //thumbnails
        thumbnailWidth: 320,
        thumbnailHeight: 240,

        //events
        onUploadStart: null, //called for every upload in queue. (args = iFormFile[], xhr, FormData)
        onUploadProgress: null, //called in intervals when upload is in progress. (args = event, percent)
        onUploadComplete: null, 
        onQueueComplete: null
    };

    function uploader(options) {
        //instance options
        this.dropTargets = options.dropTargets || defaults.dropTargets;
        this.buttons = options.buttons || defaults.buttons;
        this.url = options.url || defaults.url;
        this.method = options.method || defaults.method;
        this.multipleUploads = options.multipleUploads === true ? options.multipleUploads : defaults.multipleUploads;
        this.parallelUploads = options.parallelUploads || defaults.parallelUploads;
        this.maxFilesize = options.maxFilesize || defaults.maxFilesize;
        this.fileTypes = options.fileTypes || defaults.fileTypes;
        this.autoUpload = options.autoUpload === true ? options.autoUpload : defaults.autoUpload;
        this.capture = options.capture === true ? options.capture : defaults.capture;

        //thumbnails
        this.thumbnailWidth = options.thumbnailWidth || defaults.thumbnailWidth;
        this.thumbnailHeight = options.thumbnailHeight || defaults.thumbnailHeight;

        //events
        this.onUploadStart = options.onUploadStart || defaults.onUploadStart;
        this.onUploadProgress = options.onUploadProgress || defaults.onUploadProgress;
        this.onUploadComplete = options.onUploadComplete || defaults.onUploadComplete;
        this.onQueueComplete = options.onQueueComplete || defaults.onQueueComplete;

        //files
        this.queue = [];

        //attach drop targets to drop event
        if (typeof this.dropTargets == 'Object') {
            this.dropTargets = [this.dropTargets];
        }
        for (var x = 0; x < this.dropTargets.length; x++) {

        }

        //attach buttons to click event
        if (typeof this.buttons == 'object' && this.buttons.style) {
            this.buttons = [this.buttons];
        }
        for (var x = 0; x < this.buttons.length; x++) {
            this.buttons[x].addEventListener('click', this.click.bind(this));
        }

        //generate hidden input field
        document.body.insertAdjacentHTML('beforeend',
            '<input type="file" id="hdnuploader"' +
            (this.multipleUploads == true ? ' multiple= "true"' : '') +
            (this.fileTypes != '' ? ' accept= "' + this.fileTypes + '"' : '') +
            (this.capture == true ? ' capture="true"' : '') +
            (this.directoryDialog == true ? ' webkitdirectory="true"' : '') +
            ' style= "display:none" ></input > '
        );

        //set up events for hidden input field
        var input = document.getElementById('hdnuploader');
        input.addEventListener('change', uploadChanged.bind(this));
    }

    uploader.prototype.click = function() {
        //show file dialog
        var input = document.getElementById('hdnuploader');
        input.click();
    }

    function uploadDrop() {
        //add dropped files to upload queue
    }

    function uploadChanged(e) {
        //add files to queue
        addFilesToQueue.call(this, e.target.files);
    }

    function addFilesToQueue(files) {
        for (var x = 0; x < files.length; x++) {
            if (this.queue.filter((a) => a.name == files[x].name).length == 0) {
                this.queue.push(files[x]);
            }
        }
        if (this.autoUpload === true) {
            //auto upload after adding new files to queue
            this.upload();
        }
    }

    uploader.prototype.upload = function () {
        //creates an XHR object and uploads files in queue
        var xhr = new XMLHttpRequest();
        xhr.addEventListener("progress", uploadProgress.bind(this));
        xhr.addEventListener("load", uploadComplete.bind(this));
        xhr.addEventListener("error", uploadError.bind(this));
        xhr.addEventListener("abort", uploadAbort.bind(this));

        //append files from queue
        var data = new FormData();
        var files = this.queue.splice(0, this.parallelUploads);
        //raise event so user can manipulate xhr or data if needed
        if (typeof this.onUploadStart == 'function') {
            this.onUploadStart(files, xhr, data);
        }
        
        for (var x = 0; x < files.length; x++) {
            data.append('file[' + x + ']', files[x], files[x].name);
        }

        //begin uploading
        xhr.open(this.method, this.url);
        xhr.send(data);
    }

    function uploadProgress(e) {
        if (e.lengthComputable) {
            var perc = e.loaded / e.total;

            //raise event so user can process progress
            if (typeof this.onUploadProgress == 'function') {
                this.onUploadProgress(e, perc);
            }
        }
    }

    function uploadComplete(e) {
        //raise event so user can process upload complete
        if (typeof this.onUploadComplete == 'function') {
            this.onUploadComplete();
        }
        if (this.queue.length == 0) {
            //raise event so user can process queue complete
            if (typeof this.onQueueComplete == 'function') {
                this.onQueueComplete();
            }
        } else {
            this.upload();
        }
    }

    function uploadError(e) {

    }

    function uploadAbort(e) {

    }
    
    //finally, expose uploader to the window as a global object
    window.launchPad = function (options) {
        return new uploader(options);
    };
})();