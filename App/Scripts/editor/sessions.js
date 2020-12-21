S.editor.sessions = {
    selected: '',

    add: function (id, mode, code, select) {
        var editor = S.editor.instance;
        switch (S.editor.type) {
            case 0: //monaco ////////////////////////////////////////////////////
                require(['vs/editor/editor.main'], function () {
                    var session = monaco.editor.createModel(code, mode);
                    S.editor.sessions[id] = session;
                    S.editor.viewstates[id] = null;
                    if (S.editor.selected != '') {
                        S.editor.sessions.saveViewState(S.editor.fileId(S.editor.selected));
                    }
                    if (select !== false) {
                        S.editor.instance.saveViewState();
                        S.editor.instance.setModel(session);
                        S.editor.filebar.code.show();
                        S.editor.codebar.update();
                    }
                    session.onDidChangeContent((e) => { S.editor.changed(); });
                });
                break;
            case 1: //ace ///////////////////////////////////////////////////////
                var session = new S.editor.EditSession(code);
                session.setMode("ace/mode/" + mode);
                session.on('change', S.editor.changed);
                S.editor.sessions[id] = session;

                if (select !== false) {
                    S.editor.filebar.code.show();
                    editor.setSession(session);
                    editor.clearSelection();
                    S.editor.codebar.update();
                    S.editor.resize();
                    setTimeout(function () {
                        S.editor.resize();
                    }, 200);
                    editor.focus();
                }
                break;
        }

    },

    remove: function (id) {
        if (S.editor.sessions[id] != null) {
            S.editor.sessions[id].dispose();
            delete S.editor.sessions[id];
        }
    },

    saveViewState(id) {
        //save previous viewstate
        switch (S.editor.type) {
            case 0: //monaco
                S.editor.viewstates[id] = S.editor.instance.saveViewState();
                break;
        }
    },

    restoreViewState(id) {
        if (S.editor.viewstates[id]) {
            switch (S.editor.type) {
                case 0: //monaco
                    S.editor.instance.restoreViewState(S.editor.viewstates[id]);
                    break;
            }
        }
    }
};