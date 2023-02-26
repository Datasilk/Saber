describe('Getting Started', () => {
    beforeEach(() => {
        cy.on('window:confirm', () => true);
    });

    it("Create List Component", () => {
        cy.login();
        cy.loadEditor();
        cy.toggleBrowser();

        //open partials folder
        cy.viewFolder('content/partials');

        //create new partial files for list container & items
        cy.newFolder('lists', 'content/partials');
        var listspath = 'content/partials/lists';
        cy.viewFolder(listspath);
        cy.newFile('gallery.html', listspath);
        cy.newFile('gallery-item.html', listspath);
        cy.deleteFile(listspath + '/gallery.html');
        cy.deleteFile(listspath + '/gallery-item.html');
    });
})