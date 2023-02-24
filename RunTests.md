# Run Tests on Saber
#### A comprehensive guide to creating & running end-to-end (E2E) tests for Saber

E2E tests are a critical part of releasing new versions of Saber into the wild. When developing new features and 
modifying existing features within Saber, we need to take into account whether or not E2E tests should be
created and/or modified.

Not every feature requires an E2E test, but we should consider what type of features should be tested when
creating a new release for Saber.

### Creating Cypress Specs
When an admin runs `runtests.bat` to create a new release for Saber, the `cypress/e2e/all.cy.js` Cypress spec
is executed and all imported specs are run. So, if you are going to create a new Cypress spec, you should include
an `import` for that spec within the `cypress/e2e/all.cy.js` file so that your tests are run when it is time to 
test all features for Saber before the next release.

### Testing your Spec
You can test your spec at any time by running the command `npx cypress open` to manually run any of your test,
or execute `npx cypress run --spec cypress/e2e/path/to/my-spec.cy.js` to run your test without a GUI.
You should make sure that Saber is running on port `7070`.

If you run Saber with the `RUNTESTS=1` environment variable, Saber will utilize the **Saber-Test** database and
reset the database when Saber starts up, wiping all data from all tables & resetting all sequences in the database. 

Alternatively, you can build Saber using the *Release* configuration, then
run `gulp publish`, and finally run `cd bin/Release/Saber` and `docker compose up` to run Saber in a Docker container.
