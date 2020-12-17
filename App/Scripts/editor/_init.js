S.editor.init = function () {
    this.initialized = true;
    this.visible = true;

    //generate path
    var path = window.location.pathname.toLowerCase();
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
    S('.page-name').attr('href', path).html(path);

    //initialize code editor
    var editor = null;
    if (this.useCodeEditor == true) {
        switch (this.type) {
            case 0: //monaco
                require.config({ paths: { 'vs': '/editor/js/utility/monaco/min/vs' } });
                //show loading animation
                S('.editor-loading').html(S.loader());
                require(['vs/editor/editor.main'], function () {
                    editor = monaco.editor.create(document.getElementById('editor'), {
                        value: '',
                        theme: "vs" + (this.theme != '' ? '-' + S.editor.theme : ''),
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
                    S('.editor-loading').remove();
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
    this.resize();

    //add button events
    S('.tab-drop-menu').on('click', S.editor.dropmenu.show);
    S('.bg-overlay').on('click', S.editor.dropmenu.hide);
    S('.editor-drop-menu .item-browse').on('click', S.editor.explorer.show);
    S('.editor-drop-menu .item-save').on('click', S.editor.save);
    S('.editor-drop-menu .item-save-as').on('click', S.editor.saveAs);
    S('.editor-drop-menu .item-content-fields').on('click', function () { S.editor.filebar.fields.show(true); });
    S('.editor-drop-menu .item-page-resources').on('click', S.editor.filebar.resources.show);
    S('.editor-drop-menu .item-page-settings').on('click', S.editor.filebar.settings.show);
    S('.editor-drop-menu .item-user-management').on('click', S.editor.users.show);
    S('.editor-drop-menu .item-security').on('click', S.editor.security.show);
    S('.editor-drop-menu .item-analytics').on('click', S.editor.analytics.show);
    S('.editor-drop-menu .item-web-settings').on('click', S.editor.websettings.show);
    S('.editor-drop-menu .item-new-file').on('click', S.editor.file.create.show);
    S('.editor-drop-menu .item-new-folder').on('click', S.editor.folder.create.show);
    S('.editor-drop-menu .item-new-window').on('click', S.editor.newWindow);
    S('.editor-drop-menu .item-live-preview').attr('href', path + '?live');
    S('.tab-components').on('click', S.editor.components.show);
    S('.tab-content-fields').on('click', S.editor.filebar.fields.show);
    S('.tab-file-code').on('click', S.editor.filebar.code.show);
    S('.tab-page-settings').on('click', S.editor.filebar.settings.show);
    S('.tab-page-resources').on('click', S.editor.filebar.resources.show);
    S('.tab-preview').on('click', S.editor.filebar.preview.show);
    S('.edit-bar').on('mousedown', function (e) {
        if (e.target != S('.edit-bar')[0]) { return; }
        if (S.editor.Rhino) {
            S.editor.Rhino.drag();
        }
    });

    //add drop down events
    S('.editor #lang').on('change', S.editor.fields.load);

    //add window resize event
    S(window).on('resize', S.editor.resizeWindow);

    //register hotkeys
    S(window).on('keydown', S.editor.hotkey.pressed);

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
};


//set up editor tab
S('.editor-tab').on('click', S.editor.filebar.preview.hide);

//register hotkeys for preview mode
S(window).on('keydown', S.editor.hotkey.pressedPreview);