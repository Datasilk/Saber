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
    {{item-buttons}}
</div>

<div class="paging-buttons">
    <div class="back-button {{back-disabled}}disabled{{/back-disabled}}">
        <a href="{{back-url}}" title="Previous Page">{{back-number}}</a>
    </div>
    {{page-buttons}}
    <div class="next-button {{next-disabled}}disabled{{/next-disabled}}">
        <a href="{{next-url}}" title="Next Page">{{next-number}}</a>
    </div>
</div>
<div class="paging-info">
    Found {{total-results}} total results.
    Displaying {{displayed-results}} results, starting at {{starting-result}} thru {{ending-result}}.
    Currently on page {{current-page}} of {{total-pages}} total pages
</div>
{{item-button-template}}
    <div class="item-button item-{{item-number}} {{selected}}selected{{/selected}}">
        {{item-label}}
    </div>
{{/item-button-template}}
{{page-button-template}}
    <div class="page-button page-{{page-number}} {{selected}}selected{{/selected}}">
        <a href="{{page-url}}" title="Jump to Page {{page-number}}">{{page-number}}</a>
    </div>
{{/page-button-template}}
`
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
        cy.selectTab('content/pages/home.html');
        cy.insertCode('{{list}}');


        //remove files & folders related to test
        cy.deleteFile('content/partials/lists/gallery.html');
        cy.deleteFile('content/partials/lists/gallery-item.html');
        cy.prevFolder();
        cy.deleteFolder('content/partials/lists');
        cy.prevFolder();
    });
})