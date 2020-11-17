S.editor = {
    type: 0, //0 = Monaco, 1 = Ace
    instance: null,
    sessions: {},
    viewstates: {},
    selected: '',
    path: '',
    theme: 'dark',
    sect: $('.sections'),
    div: $('.code-editor'),
    divFields: $('.content-fields'),
    divBrowser: $('.file-browser'),
    initialized: false,
    savedTabs: [],
    Rhino: null,
    visible: false
};