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
        cy.deleteFolder('content/partials/lists', true);

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

        //add list item to bottom of home page /////////////////////////////////////
        cy.selectTab('content/pages/home.html');

        //select last line of code
        cy.getCode().then((homehtml) => {
            cy.monaco().then((editor) => {
                var lines = editor.getModel().getLineCount();
                var listname = 'test';
                editor.revealLine(lines);
                editor.setPosition({ column: 1, lineNumber: lines });
                cy.getEditor().type('{end}{enter}');
                cy.getEditor().find('.tab-components').click();
                cy.getEditor().find('.component-item[data-key="list"]').click();
                cy.getEditor().find('#component_id').type(listname);
                cy.intercept('POST', '/api/Files/Dir').as('select-partial');
                //select partial container
                cy.getEditor().find('.param-container .select-partial button').click();
                cy.wait('@select-partial').then((s) => {
                    expect(s.response.statusCode).to.eq(200);
                });
                var pathId = getPathId('Content/partials/lists');
                cy.getEditor().find('.modal-browser .fileid-' + pathId).click();
                pathId = getPathId('Content/partials/lists/gallery.html');
                cy.getEditor().find('.modal-browser .fileid-' + pathId).click();

                //select partial view
                cy.getEditor().find('.param-partial .add-list-item a').click();
                cy.wait('@select-partial').then((s) => {
                    expect(s.response.statusCode).to.eq(200);
                });
                pathId = getPathId('Content/partials/lists');
                cy.getEditor().find('.modal-browser .fileid-' + pathId).click();
                pathId = getPathId('Content/partials/lists/gallery-item.html');
                cy.getEditor().find('.modal-browser .fileid-' + pathId).click();

                //generate list
                cy.getEditor().find('.component-configure .save-component').click();
                cy.saveFile();

                //navigate to page content tab
                cy.getEditor().find('.tab-content-fields').click();

                //add list item #1 (upload image, set title)
                cy.intercept('POST', '/api/ContentFields/Render').as('render-fields');
                cy.intercept('POST', '/api/PageResources/Render').as('render-uploads');
                cy.intercept('POST', '/Upload/Resources').as('upload-file');
                cy.getEditor().find('.field_list-' + listname + ' .add-list-item > div').click();
                cy.wait('@render-fields').then((s) => {expect(s.response.statusCode).to.eq(200);});
                cy.getEditor().find('.popup.show .field_image button').click();
                cy.uploadFiles('cypress/uploads/list/01.jpg');
                cy.wait('@upload-file').then((s) => {expect(s.response.statusCode).to.eq(200);});
                cy.wait('@render-uploads').then((s) => { expect(s.response.statusCode).to.eq(200); });
                cy.getEditor().find('.popup.show img[alt="image 01.jpg"]').click();
                cy.getEditor().find('.popup.show input.button.apply').click();
                cy.getEditor().find('.popup.show #field_title').type('Nissan Frontier @ Nimbus backyard');
                cy.getEditor().find('.popup.show .has-content-fields button.apply').click();
                cy.wait(60 * 1000);

                //remove files & folders related to test
                cy.deleteFile('content/partials/lists/gallery.html');
                cy.deleteFile('content/partials/lists/gallery-item.html');
                cy.prevFolder();
                cy.deleteFolder('content/partials/lists');
                cy.prevFolder();

                //set homepage back to original source code
                cy.selectTab('content/pages/home.html');
                cy.writeCode(homehtml);
                cy.saveFile();
            });
        });

        //////////////////////////////////////////////////////////////////////////
    });
});



function getPathId(path) {
    return path.replace(/\//g, '_').replace(/\./g, '_');
}