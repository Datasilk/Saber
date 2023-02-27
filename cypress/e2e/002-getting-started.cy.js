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
        var path = 'content/partials/lists';
        cy.viewFolder(path);
        cy.newFile('gallery.html', path);
        cy.newFile('gallery-item.html', path);

        //open gallery.html and write code
        path = 'content/partials/lists/gallery.html';
        cy.openFile(path);
        cy.writeCode('' + 
`<div class="gallery">
    <div class="lg-img"></div>
    {{list}}
</div>`
        );
        //save gallery.html
        cy.saveFile();

        //open gallery-item.html and write code
        path = 'content/partials/lists/gallery-item.html';
        cy.openFile(path);
        cy.writeCode('' +
`<div class="gallery-item">
    <img data-src="{{image}}" alt="{{title}}" title="{{title}}">
</div>`
        );
        //save gallery-item.html
        cy.saveFile();

        //add list item to bottom of home page


        //remove files & folders related to test
        cy.deleteFile('content/partials/lists/gallery.html');
        cy.deleteFile('content/partials/lists/gallery-item.html');
        cy.prevFolder();
        cy.deleteFolder('content/partials/lists');
        cy.prevFolder();
    });
})