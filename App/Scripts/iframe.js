//load editor into iframe
(() => {
    var container = document.getElementsByClassName('editor-iframe')[0];
    var iframe = document.getElementById('editor-iframe');
    var tab = document.getElementsByClassName('editor-tab')[0];

    tab.addEventListener('click', () => {
        loadEditor();
        container.style.display = 'block';
    });

    function loadEditor() {
        if (iframe.getAttribute('src') == 'about:blank') {
            iframe.setAttribute('src', '/Editor?path=' + iframe.getAttribute('data-path'));
        }
    }
    if (preloadEditor == '1') {
        loadEditor();
    }

    window.addEventListener('keydown', showEditor, false);

    function showEditor(e) {
        if (e.ctrlKey == false && e.altKey == false && e.shiftKey == false) {
            switch (e.key) {
                case 'Escape': //escape key
                    if (container.style.display == 'block') {
                        container.style.display = 'none';
                        iframe.contentWindow.S.editor.preview.show();
                    } else {
                        loadEditor();
                        container.style.display = 'block';
                        if (iframe.contentWindow.S) {
                            iframe.contentWindow.S.editor.preview.hide();
                        }
                    }
                    break;
            }
        }
    }

    window.addScript = function (script) {
        var js = new Function(script);
        js();
    }
})();