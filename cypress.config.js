const { defineConfig } = require("cypress");

module.exports = defineConfig({
  e2e: {
        baseUrl: 'http://localhost:7070',
        defaultCommandTimeout: 1000
  },
});
