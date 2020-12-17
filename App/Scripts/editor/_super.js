S.editor = {
    type: 0, //0 = Monaco, 1 = Ace
    instance: null,
    sessions: {},
    viewstates: {},
    selected: '',
    path: '',
    theme: 'dark',
    sect: S('.sections'),
    div: S('.code-editor'),
    divFields: S('.content-fields'),
    divBrowser: S('.file-browser'),
    initialized: false,
    savedTabs: [],
    Rhino: null,
    visible: false,
    useCodeEditor: false
};

//used by vendors to extend Saber
S.vendor = {};