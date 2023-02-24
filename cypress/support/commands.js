Cypress.Commands.add('login', () => {
    cy.visit('/login');
    cy.get('#email').type('tester@test.com');
    cy.get('#password').type('test123456');
    cy.intercept('POST', '/api/User/Authenticate').as('login');
    cy.get('#login').click();
    cy.wait('@login').then(({ response }) => {
        expect(response.statusCode).to.eq(200);
    });
});

Cypress.Commands.add('toggleEditor', () => {
    cy.get('body').type('{esc}');
});