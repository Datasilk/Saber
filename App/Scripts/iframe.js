//load editor into iframe
(() => {
    var container = document.getElementsByClassName('editor-iframe')[0];
    var iframe = document.getElementById('editor-iframe');

    iframe.addEventListener('load', () => {
        iframe.contentWindow.S.editor.init();
    });

    window.addEventListener('resize', () => {
        resizeIframe();
    });

    function resizeIframe(){
        requestAnimationFrame(() => {
            var h = window.innerHeight;
            iframe.style.height = h + 'px';
        });
    }
    resizeIframe();

    window.addEventListener('keydown', (e) => {
        if (e.ctrlKey == false && e.altKey == false && e.shiftKey == false) {
            switch (e.key) {
                case 'Escape': //escape key
                    if (container.style.display == 'block') {
                        container.style.display = 'none';
                        iframe.contentWindow.S.editor.filebar.preview.show();
                    } else {
                        container.style.display = 'block';
                        resizeIframe();
                        iframe.contentWindow.S.editor.filebar.preview.hide();
                    }
                    break;
            }
        }
    }, false);

    window.addScript = function (script) {
        var js = new Function(script);
        js();
    }
})();