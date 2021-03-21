S.editor.init = function () {
    this.initialized = true;
    this.visible = true;

    //generate path
    var path = '/' + S.editor.queryString(window.location.href, 'path');
    if (path == '/') { path = '/home'; }
    if (path.substr(path.length - 1, 1) == '/') {
        //remove leading slash
        path = path.substr(0, path.length - 1);
    }
    this.path = 'content' + path;
    var paths = this.path.split('/').filter(a => a != '');
    var file = paths[paths.length - 1];
    var dir = paths.join('/').replace(file, '');
    var fileparts = paths[paths.length - 1].split('.', 2);
    $('.page-name').attr('href', path).html(path);

    //initialize code editor
    var editor = null;
    if (this.useCodeEditor == true) {
        switch (this.type) {
            case 0: //monaco
                require.config({ paths: { 'vs': '/editor/js/utility/monaco/min/vs' } });

                //show loading animation
                $('.editor-loading').html(S.loader());

                //load monaco editor
                require(['vs/editor/editor.main'], function () {

                    //create syntax highlighter for html + mustache
                    monaco.languages.register({ id: 'html-mustache' });

                    //register token provider (Monarch) for html + mustache
                    monaco.languages.setMonarchTokensProvider('html-mustache', {
                        defaultToken: '',
                        tokenPostfix: '.html',
                        ignoreCase: true,

                        tokenizer: {
                            root: [
                                //mustache
                                [/({{)([\w\-]+)/, ['mustache', { token: 'mustache-tag', next: '@mustache' }]],

                                //html
                                [/<!DOCTYPE/, 'metatag', '@doctype'],
                                [/<!--/, 'comment', '@comment'],
                                [/(<)((?:[\w\-]+:)?[\w\-]+)(\s*)(\/>)/, ['delimiter', 'tag', '', 'delimiter']],
                                [/(<)(script)/, ['delimiter', { token: 'tag', next: '@script' }]],
                                [/(<)(style)/, ['delimiter', { token: 'tag', next: '@style' }]],
                                [/(<)((?:[\w\-]+:)?[\w\-]+)/, ['delimiter', { token: 'tag', next: '@otherTag' }]],
                                [/(<\/)((?:[\w\-]+:)?[\w\-]+)/, ['delimiter', { token: 'tag', next: '@otherTag' }]],
                                [/</, 'delimiter']
                            ],

                            mustache: [
                                [/"([^"]*)"/, 'mustache-value'],
                                [/[\w\-]+/, 'mustache-name'],
                                [/:/, 'delimiter'],
                                [/,/, 'delimiter'],
                                [/}}/, 'mustache', '@pop']
                            ],

                            //html tokens
                            doctype: [
                                [/[^>]+/, 'metatag.content'],
                                [/>/, 'metatag', '@pop'],
                            ],

                            comment: [
                                [/-->/, 'comment', '@pop'],
                                [/[^-]+/, 'comment.content'],
                                [/./, 'comment.content']
                            ],

                            otherTag: [
                                [/\/?>/, 'delimiter', '@pop'],
                                [/"([^"]*)"/, 'attribute.value'],
                                [/'([^']*)'/, 'attribute.value'],
                                [/[\w\-]+/, 'attribute.name'],
                                [/=/, 'delimiter'],
                                [/[ \t\r\n]+/], // whitespace
                            ],

                            // -- BEGIN <script> tags handling

                            // After <script
                            script: [
                                [/type/, 'attribute.name', '@scriptAfterType'],
                                [/"([^"]*)"/, 'attribute.value'],
                                [/'([^']*)'/, 'attribute.value'],
                                [/[\w\-]+/, 'attribute.name'],
                                [/=/, 'delimiter'],
                                [/>/, { token: 'delimiter', next: '@scriptEmbedded', nextEmbedded: 'text/javascript' }],
                                [/[ \t\r\n]+/], // whitespace
                                [/(<\/)(script\s*)(>)/, ['delimiter', 'tag', { token: 'delimiter', next: '@pop' }]]
                            ],

                            // After <script ... type
                            scriptAfterType: [
                                [/=/, 'delimiter', '@scriptAfterTypeEquals'],
                                [/>/, { token: 'delimiter', next: '@scriptEmbedded', nextEmbedded: 'text/javascript' }], // cover invalid e.g. <script type>
                                [/[ \t\r\n]+/], // whitespace
                                [/<\/script\s*>/, { token: '@rematch', next: '@pop' }]
                            ],

                            // After <script ... type =
                            scriptAfterTypeEquals: [
                                [/"([^"]*)"/, { token: 'attribute.value', switchTo: '@scriptWithCustomType.$1' }],
                                [/'([^']*)'/, { token: 'attribute.value', switchTo: '@scriptWithCustomType.$1' }],
                                [/>/, { token: 'delimiter', next: '@scriptEmbedded', nextEmbedded: 'text/javascript' }], // cover invalid e.g. <script type=>
                                [/[ \t\r\n]+/], // whitespace
                                [/<\/script\s*>/, { token: '@rematch', next: '@pop' }]
                            ],

                            // After <script ... type = $S2
                            scriptWithCustomType: [
                                [/>/, { token: 'delimiter', next: '@scriptEmbedded.$S2', nextEmbedded: '$S2' }],
                                [/"([^"]*)"/, 'attribute.value'],
                                [/'([^']*)'/, 'attribute.value'],
                                [/[\w\-]+/, 'attribute.name'],
                                [/=/, 'delimiter'],
                                [/[ \t\r\n]+/], // whitespace
                                [/<\/script\s*>/, { token: '@rematch', next: '@pop' }]
                            ],

                            scriptEmbedded: [
                                [/<\/script/, { token: '@rematch', next: '@pop', nextEmbedded: '@pop' }],
                                [/[^<]+/, '']
                            ],

                            // -- END <script> tags handling


                            // -- BEGIN <style> tags handling

                            // After <style
                            style: [
                                [/type/, 'attribute.name', '@styleAfterType'],
                                [/"([^"]*)"/, 'attribute.value'],
                                [/'([^']*)'/, 'attribute.value'],
                                [/[\w\-]+/, 'attribute.name'],
                                [/=/, 'delimiter'],
                                [/>/, { token: 'delimiter', next: '@styleEmbedded', nextEmbedded: 'text/css' }],
                                [/[ \t\r\n]+/], // whitespace
                                [/(<\/)(style\s*)(>)/, ['delimiter', 'tag', { token: 'delimiter', next: '@pop' }]]
                            ],

                            // After <style ... type
                            styleAfterType: [
                                [/=/, 'delimiter', '@styleAfterTypeEquals'],
                                [/>/, { token: 'delimiter', next: '@styleEmbedded', nextEmbedded: 'text/css' }], // cover invalid e.g. <style type>
                                [/[ \t\r\n]+/], // whitespace
                                [/<\/style\s*>/, { token: '@rematch', next: '@pop' }]
                            ],

                            // After <style ... type =
                            styleAfterTypeEquals: [
                                [/"([^"]*)"/, { token: 'attribute.value', switchTo: '@styleWithCustomType.$1' }],
                                [/'([^']*)'/, { token: 'attribute.value', switchTo: '@styleWithCustomType.$1' }],
                                [/>/, { token: 'delimiter', next: '@styleEmbedded', nextEmbedded: 'text/css' }], // cover invalid e.g. <style type=>
                                [/[ \t\r\n]+/], // whitespace
                                [/<\/style\s*>/, { token: '@rematch', next: '@pop' }]
                            ],

                            // After <style ... type = $S2
                            styleWithCustomType: [
                                [/>/, { token: 'delimiter', next: '@styleEmbedded.$S2', nextEmbedded: '$S2' }],
                                [/"([^"]*)"/, 'attribute.value'],
                                [/'([^']*)'/, 'attribute.value'],
                                [/[\w\-]+/, 'attribute.name'],
                                [/=/, 'delimiter'],
                                [/[ \t\r\n]+/], // whitespace
                                [/<\/style\s*>/, { token: '@rematch', next: '@pop' }]
                            ],

                            styleEmbedded: [
                                [/<\/style/, { token: '@rematch', next: '@pop', nextEmbedded: '@pop' }],
                                [/[^<]+/, '']
                            ],

		                    // -- END <style> tags handling
                        }
                    });

                    //define theme for mustache code
                    monaco.editor.defineTheme('mustache', {
                        base: "vs" + (this.theme != '' ? '-' + S.editor.theme : ''),
                        inherit: true,
                        rules: [
                            { token: 'mustache', foreground: '3cb73a' },
                            { token: 'mustache-tag', foreground: '84c383' },
                            { token: 'mustache-value', foreground: 'c2cea2' },
                            { token: 'mustache-name', foreground: 'c6e8c5' },
                        ]
                    });

                    //create editor
                    editor = monaco.editor.create(document.getElementById('editor'), {
                        value: '',
                        theme: 'mustache',
                        autoIndent: false,
                        automaticLayout: true,
                        colorDecorators: true,
                        dragAndDrop: false,
                        folding: true,
                        formatOnPaste: false,
                        glyphMargin: false,
                        mouseWheelZoom: true,
                        parameterHints: true,
                        showFoldingControls: 'always'
                    });
                    editor.onMouseUp((e) => { S.editor.codebar.update(); });
                    S.editor.instance = editor;

                    //hide loading animation
                    $('.editor-loading').remove();
                });
                break;

            case 1: //ace
                editor = ace.edit("editor");
                editor.setTheme("ace/theme/xcode");
                editor.setOptions({
                    //enableEmmet: true
                });
                S.editor.EditSession = require("ace/edit_session").EditSession;

                //add editor key bindings
                editor.commands.addCommand({
                    name: "showKeyboardShortcuts",
                    bindKey: { win: "Ctrl-h", mac: "Command-h" },
                    exec: function (editor) {
                        ace.config.loadModule("ace/ext/keybinding_menu", function (module) {
                            module.init(editor);
                            S.editor.instance.showKeyboardShortcuts()
                        })
                    }
                });
                this.instance = editor;
                break;
        }
    }

    //resize code editor
    this.resize.window();

    //add button events
    $('.bg-overlay').on('click', S.editor.dropmenu.hide);
    $('.top-menu .menu-bar > li').on('click', S.editor.dropmenu.show);
    $('.top-menu .menu-bar > li').hover('', {}, S.editor.topmenu.hover, () => { });
    $('.top-menu .menu-bar .item-browse').on('click', S.editor.explorer.show);
    $('.top-menu .menu-bar .item-save').on('click', S.editor.save);
    $('.top-menu .menu-bar .item-save-as').on('click', S.editor.saveAs);
    $('.top-menu .menu-bar .item-content-fields').on('click', function () { S.editor.filebar.fields.show(true); });
    $('.top-menu .menu-bar .item-page-resources').on('click', S.editor.filebar.resources.show);
    $('.top-menu .menu-bar .item-page-settings').on('click', S.editor.filebar.settings.show);
    $('.top-menu .menu-bar .item-user-management').on('click', S.editor.users.show);
    $('.top-menu .menu-bar .item-security').on('click', S.editor.security.show);
    $('.top-menu .menu-bar .item-datasources').on('click', S.editor.datasources.show);
    $('.top-menu .menu-bar .item-analytics').on('click', S.editor.analytics.show);
    $('.top-menu .menu-bar .item-web-settings').on('click', S.editor.websettings.show);
    $('.top-menu .menu-bar .item-new-file').on('click', S.editor.file.create.show);
    $('.top-menu .menu-bar .item-new-folder').on('click', S.editor.folder.create.show);
    $('.top-menu .menu-bar .item-new-window').on('click', S.editor.newWindow);
    $('.top-menu .menu-bar .item-new-tab a').attr('href', path);
    $('.top-menu .menu-bar .item-live-preview a').attr('href', path + '?live');
    $('.top-menu .menu-bar .item-web-stylesheets').on('click', () => { S.editor.websettings.show('stylesheets'); });
    $('.top-menu .menu-bar .item-web-scripts').on('click', () => { S.editor.websettings.show('scripts'); });
    $('.top-menu .menu-bar .item-web-icons').on('click', () => { S.editor.websettings.show('icons'); });
    $('.top-menu .menu-bar .item-web-email').on('click', () => { S.editor.websettings.show('email-settings'); });
    $('.top-menu .menu-bar .item-web-pass').on('click', () => { S.editor.websettings.show('password-settings'); });
    $('.top-menu .menu-bar .item-web-dev-keys').on('click', () => { S.editor.websettings.show('dev-keys'); });
    $('.top-menu .menu-bar .item-web-plugins').on('click', () => { S.editor.websettings.show('plugins'); });
    $('.tab-components').on('click', S.editor.components.show);
    $('.tab-content-fields').on('click', S.editor.filebar.fields.show);
    $('.tab-file-code').on('click', S.editor.filebar.code.show);
    $('.tab-page-settings').on('click', S.editor.filebar.settings.show);
    $('.tab-page-resources').on('click', S.editor.filebar.resources.show);
    $('.tab-preview').on('click', S.editor.filebar.preview.show);
    $('.edit-bar').on('mousedown', function (e) {
        if (e.target != $('.edit-bar')[0]) { return; }
        if (S.editor.Rhino) {
            S.editor.Rhino.drag();
        }
    });

    //add drop down events
    $('.editor #lang').on('change', S.editor.fields.load);

    //add window resize event
    $(window).on('resize', S.editor.resize.window);

    //register hotkeys
    $(window).on('keydown', S.editor.hotkey.pressed);

    //register explorer routes
    S.editor.explorer.routes = [
        {}
    ];

    //finally, load content resources that belong to the page
    if (this.useCodeEditor == true) {
        var tabs = [dir + fileparts[0] + '.html', dir + fileparts[0] + '.less', dir + fileparts[0] + '.js'];
        if (this.savedTabs.length > 0) {
            tabs = tabs.concat(this.savedTabs);
        }
        //get saved tabs from server
        S.ajax.post('Files/GetOpenedTabs', {},
            function (d) {
                tabs = tabs.concat(JSON.parse(d));
                openTabs();
            },
            function (err) {
                openTabs();
            }
        );

        function openTabs() {
            S.editor.explorer.openResources(tabs,
                function () {
                    setTimeout(function () {
                        S.editor.codebar.status('Ready');
                        S.editor.codebar.update();
                    }, 500);
                }
            );
        }
    } else {
        //load file browser & wwwroot instead
        S.editor.explorer.show();
        S.editor.explorer.dir('wwwroot');
    }

    //initialize JavaScript binding into Rhinoceros (if available)
    if (typeof CefSharp != 'undefined') {
        (async function () {
            await CefSharp.BindObjectAsync("Rhino", "bound");
            S.editor.Rhino = Rhino;

            //change color scheme of Rhino window
            S.editor.filebar.preview.hide();
        })();
    }

    S.editor.filebar.preview.hide();
};

//set up editor tab
$('.editor-tab').on('click', S.editor.filebar.preview.hide);

//register hotkeys for preview mode
$(window).on('keydown', S.editor.hotkey.pressedPreview);