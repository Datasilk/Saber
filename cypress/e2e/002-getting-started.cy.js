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
    <div class="lg-item"><img/></div>
    <div class="items">{{list}}</div>
    <div class="buttons">{{item-buttons}}</div>
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
`);

        //save gallery.html
        cy.saveFile();

        //open gallery-item.html and write code
        path = 'content/partials/lists/gallery-item.html';
        cy.openFile(path);
        cy.writeCode('' +
            `<div class="gallery-item">
    <img data-src="{{image}}" alt="{{title}}" title="{{title}}">
</div>`);

        //save gallery-item.html
        cy.saveFile();

        //open website.js to add code that loads image thumbnails into gallery
        cy.openWebsiteJs();
        cy.writeCode('' +
`(function(){
    var gallery = {
        load: function(selector){
            var imgs = document.querySelectorAll(selector + ' .items img');
            var mainimg = document.querySelectorAll(selector + ' .lg-item img')[0];
            for(let x = 0; x < imgs.length; x++){
                let img = imgs[x];
                let imgurl = img.getAttribute('data-src');
                img.src = imgurl;
                if(x == 0){
                    console.log(img);
                    console.log(imgurl);
                    mainimg.src = imgurl;
                }
                img.onclick = function(e){
                    mainimg.src = imgurl;
                };
            }
        }
    };
    window.gallery = gallery;
    gallery.load('.gallery');
})();`);

        //save website.js
        cy.saveFile();

        //open website.less to add styling for the list component
        cy.openWebsiteLess();
        cy.insertCode('' +
`.gallery{width:100%; max-width:1280px; margin:0 auto; padding:0 20px;
    .lg-item{overflow:hidden; max-height:550px;}
    .gallery-item{display:inline-block; padding:5px;}
    .buttons{
        .item-button{display:inline-block; cursor:pointer; width:100%; max-width:400px; padding:4px 15px; background-color:#2c4369; border-radius:5px; margin:0 4px 4px 0;
            &:hover{background-color:#26477e;}    
        }
    }
}`
        , null, (websiteless) => {
            //save website.less
            cy.saveFile();

            //add list to bottom of home page /////////////////////////////////////
            cy.selectTab('content/pages/home.html');

            //select last line of code
            cy.getCode().then((homehtml) => {
                //keep homehtml in memory for when we set home.html back to original html markup
                cy.monaco().then((editor) => {
                    var lines = editor.getModel().getLineCount();
                    var listname = 'test';
                    editor.revealLine(lines);
                    editor.setPosition({ column: 1, lineNumber: lines });
                    cy.getEditor().type('{end}{enter}');
                    cy.getEditor().find('.tab-components').click();
                    cy.getEditor().find('.component-item[data-key="list"]').click();
                    cy.getEditor().find('#component_id').type(listname);
                    cy.getEditor().find('#param_key').type('title');
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
                    cy.intercept('POST', '/api/ContentFields/Render').as('render-fields');
                    cy.intercept('POST', '/api/PageResources/Render').as('render-uploads');
                    cy.intercept('POST', '/Upload/Resources').as('upload-file');

                    //add list items
                    function addListItem(file, title) {
                        cy.getEditor().find('.field_list-' + listname + ' .add-list-item > div').click();
                        cy.wait('@render-fields').then((s) => { expect(s.response.statusCode).to.eq(200); });
                        cy.getEditor().find('.popup.show .field_image button').click();
                        cy.uploadFiles('cypress/uploads/list/' + file);
                        cy.wait('@upload-file').then((s) => { expect(s.response.statusCode).to.eq(200); });
                        cy.wait('@render-uploads').then((s) => { expect(s.response.statusCode).to.eq(200); });
                        cy.getEditor().find('.popup.show img[alt="image ' + file + '"]').click();
                        cy.getEditor().find('.popup.show input.button.apply').click();
                        cy.getEditor().find('.popup.show #field_title').type(title);
                        cy.getEditor().find('.popup.show .has-content-fields button.apply').click();
                    }

                    var listItems = [
                        { file: '01.jpg',  summary: 'Nissan Frontier @ Nimbus backyard' },
                        { file: '02.jpg',  summary: 'Downtown Austin, TX during summer' },
                        { file: '03.webp', summary: 'Final Fantasy 6, Esper Chaos Wave' },
                        { file: '04.jpg',  summary: 'City Girl deer music inspired art' },
                        { file: '05.png',  summary: 'Mark Entingh web designer in 2000' },
                        { file: '06.jpg',  summary: 'Wolf Pup 18TO 2022 interior model' }
                    ];

                    listItems.forEach(item => {
                        addListItem(item.file, item.summary);
                    });

                    //check to see if gallery is rendered correctly on web page
                    cy.intercept('POST', '/api/Page/Render').as('render-page');
                    cy.toggleEditor();
                    cy.wait('@render-page').then((s) => { expect(s.response.statusCode).to.eq(200); });
                    cy.get('.website .content .gallery').should('exist');


                    var i = 0;
                    listItems.forEach(item => {
                        i++;
                        cy.get('.gallery-item img[data-src="/images/' + item.file + '"]').should('exist');
                        cy.get('.gallery .buttons .item-' + i).should('exist');
                    });

                    //go back into editor & clean up
                    cy.toggleEditor();

                    //remove all list items
                    cy.getEditor().find('.field_list-' + listname + ' .tab-list-items > div').click();

                    function removeTopListItem() {
                        cy.getEditor().find('.field_list-' + listname + ' .list-items li:nth-child(1) .close-btn').click();
                    }

                    listItems.forEach(item => {
                        removeTopListItem();
                    });
                    cy.savePageContent();

                    //remove files & folders related to test
                    cy.deleteFile('content/partials/lists/gallery.html');
                    cy.deleteFile('content/partials/lists/gallery-item.html');
                    cy.prevFolder();
                    cy.deleteFolder('content/partials/lists');
                    cy.prevFolder();

                    //remove images related to test
                    cy.viewFolder('wwwroot');
                    cy.viewFolder('wwwroot/images');
                    listItems.forEach(item => {
                        cy.deleteMedia(item.file);
                    });

                    //hide file browser
                    cy.toggleBrowser();

                    //set home.html back to original html markup
                    cy.selectTab('content/pages/home.html');
                    cy.writeCode(homehtml);
                    cy.saveFile();

                    //remove all code from website.js
                    cy.selectTab('website.js');
                    cy.writeCode('');
                    cy.saveFile();

                    //set website.less back to original LESS
                    cy.selectTab('website.less');
                    cy.writeCode(websiteless);
                    cy.saveFile();
                });
            });

        //////////////////////////////////////////////////////////////////////////
        });
    });
});

function getPathId(path) {
    return path.replace(/\//g, '_').replace(/\./g, '_');
}