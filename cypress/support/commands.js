Cypress.Commands.add('login', () => {
    cy.visit('/login');
    cy.get('#email').type('tester@test.com');
    cy.get('#password').type('test123456');
    cy.intercept('POST', '/api/User/Authenticate').as('login');
    cy.intercept('GET', '/home').as('redirect');
    cy.get('#login').click();
    cy.wait('@login').then(({ response }) => {
        expect(response.statusCode).to.eq(200);
    });
    cy.wait('@redirect').then(({ response }) => {
        expect(response.statusCode).to.eq(200);
    });
});

//load editor for the first time
Cypress.Commands.add('loadEditor', () => {
    cy.intercept('GET', '/Editor*').as('editor');
    cy.get('body').type('{esc}');
    cy.wait('@editor').then((s) => {
        expect(s.response.statusCode).to.eq(200);
    });
    cy.wait(2000);
});

//toggle the editor
Cypress.Commands.add('toggleEditor', () => {
    cy.get('body').type('{esc}');
});

//get editor iframe window
Cypress.Commands.add('getEditor', () => {
    return cy.get('#editor-iframe')
        .its('0.contentDocument.body').should('not.be.empty')
        .then(cy.wrap);
});


//get editor Window object (which contains S.editor object)
Cypress.Commands.add('Saber', (callback) => {
    cy.get('#editor-iframe').then(a => {
        callback(a[0].contentWindow);
    });
});

//get Monaco editor instance from Saber current selected tab
Cypress.Commands.add('monaco', (callback) => {
    cy.Saber((saber) => {
        callback(saber.S.editor.instance);
    });
});

//write code in the Monaco editor
Cypress.Commands.add('writeCode', (text) => {
    cy.monaco((editor) => {
        const range = editor.getModel().getFullModelRange();
        editor.setSelection(range);
        editor.getModel().setValue(text);
    });
});

//toggle file browser
Cypress.Commands.add('toggleBrowser', () => {
    cy.getEditor().find('.menu-item-view > .row').click();
    cy.getEditor().find('.menu-item-view .item-browse').click();
    cy.getEditor().find('.file-browser menu', { timeout: 3000 }).should('not.be.empty');
});

//create new file
Cypress.Commands.add('newFile', (name, path) => {
    cy.getEditor().find('.menu-item-file > .row').click();
    cy.getEditor().find('.item-new-file').click();
    cy.getEditor().find('#newfilename').type(name);
    cy.getEditor().find('#newfilepath').clear().type(path);
    cy.intercept('POST', '/api/Files/NewFile').as('create-file');
    cy.getEditor().find('.popup.show input.button.apply').click();
    cy.wait('@create-file').then((s) => {
        expect(s.response.statusCode).to.eq(200);
    });
});

//create new folder
Cypress.Commands.add('newFolder', (name, path) => {
    cy.getEditor().find('.menu-item-file > .row').click();
    cy.getEditor().find('.item-new-folder').click();
    cy.getEditor().find('#newfolder').type(name);
    cy.getEditor().find('#newfolderpath').clear().type(path);
    cy.intercept('POST', '/api/Files/NewFolder').as('create-folder');
    cy.getEditor().find('.popup.show input.button.apply').click();
    cy.wait('@create-folder').then((s) => {
        expect(s.response.statusCode).to.eq(200);
    });
});

function getPathId(path) {
    return path.replace(/\//g, '_').replace(/\./g, '_');
}

//open file
Cypress.Commands.add('openFile', (path) => {
    cy.getEditor().find('.row.type-file[data-path="' + path + '"]').click();
    var path_id = getPathId(path);
    cy.getEditor().find('.tab-' + path_id).should('have.class', 'selected');
});

//open folder
Cypress.Commands.add('viewFolder', (path) => {
    cy.getEditor().find('.row.type-folder[data-path="' + path + '"]').click();
    cy.getEditor().find('.file-browser .browser-path').should('contains.text', path);
});

//previous folder
Cypress.Commands.add('prevFolder', () => {
    cy.getEditor().find('.row.fileid-goback').click();
});

//delete file
Cypress.Commands.add('deleteFile', (path) => {
    cy.intercept('POST', '/api/Files/DeleteFile').as('delete-file');
    cy.getEditor().find('.row.type-file[data-path="' + path + '"] .delete-btn').click();
    cy.wait('@delete-file').then((s) => {
        expect(s.response.statusCode).to.eq(200);
        var path_id = getPathId(path);
        cy.getEditor().find('.tab-' + path_id).should('not.exist');
    });
});

//delete folder
Cypress.Commands.add('deleteFolder', (path) => {
    cy.intercept('POST', '/api/Files/DeleteFolder').as('delete-folder');
    cy.getEditor().find('.row.type-folder[data-path="' + path + '"] .delete-btn').click();
    cy.wait('@delete-folder').then((s) => {
        expect(s.response.statusCode).to.eq(200);
    });
});

//save file contents
Cypress.Commands.add('saveFile', () => {
    cy.intercept('POST', '/api/Files/SaveFile').as('save-file');
    cy.getEditor().type('{ctrl+s}');
    cy.wait('@save-file').then((s) => {
        expect(s.response.statusCode).to.eq(200);
    });
});
