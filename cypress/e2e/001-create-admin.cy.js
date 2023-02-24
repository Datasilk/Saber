describe('Login As Administrator & View Saber Editor', () => {
    it('Create Administrator Account', () => {
        cy.visit('http://localhost:7070/login');
        cy.get('#name').type('Cypress Tester');
        cy.get('#email').type('tester@test.com');
        cy.get('#password').type('test123456');
        cy.get('#password2').type('test123456');
        cy.intercept('POST', '/api/User/CreateAdminAccount').as('create-admin-account');
        cy.get('#btncreate').click();
        cy.get('#loginform');
    });
    it("View Saber's Editor", () => {
        cy.login();
        cy.toggleEditor();
    });
})