const fs = require('fs');
if (!fs.existsSync('./App/config.json')) {
    fs.copyFileSync('./App/Content/temp/config.json', './App/config.json');
    fs.copyFileSync('./App/Content/temp/config.docker.json', './App/config.docker.json');
}