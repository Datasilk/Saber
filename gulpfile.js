'use strict';

// fetch command line arguments
const arg = (argList => {

    let arg = {}, a, opt, thisOpt, curOpt;
    for (a = 0; a < argList.length; a++) {

        thisOpt = argList[a].trim();
        opt = thisOpt.replace(/^\-+/, '');

        if (opt === thisOpt) {

            // argument value
            if (curOpt) arg[curOpt] = opt;
            curOpt = null;

        }
        else {

            // argument name
            curOpt = opt;
            arg[curOpt] = true;

        }

    }

    return arg;

})(process.argv);

//includes
var gulp = require('gulp'),
    concat = require('gulp-concat'),
    uglify = require('gulp-uglify'),
    compile = require('google-closure-compiler-js').gulp(),
    cleancss = require('gulp-clean-css'),
    less = require('gulp-less'),
    rename = require('gulp-rename'),
    merge = require('merge-stream'),
    changed = require('gulp-changed'),
    config = require('./App/config.json');
    
//get config variables from config.json
var environment = config.environment;

//determine environment
var prod = false;
if (environment != 'dev' && environment != 'development' && environment != null) {
    //using staging or production environment
    prod = true;
}

//paths
var paths = {
    scripts: './App/Scripts/',
    css: './App/CSS/',
    app: './App/',
    webroot: './App/wwwroot/',
};

//working paths
paths.working = {
    js: {
        platform: [
            paths.scripts + 'selector/selector.js',
            paths.scripts + 'utility/velocity.min.js',
            paths.scripts + 'platform/_super.js', // <---- Datasilk Core Js: S object
            paths.scripts + 'platform/ajax.js', //   <---- Optional platform features
            //paths.scripts + 'platform/loader.js',
            paths.scripts + 'platform/message.js',
            //paths.scripts + 'platform/polyfill.js',
            paths.scripts + 'platform/popup.js',
            paths.scripts + 'platform/scaffold.js',
            paths.scripts + 'platform/svg.js',
            paths.scripts + 'platform/util.js',
            //paths.scripts + 'platform/util.color.js',
            //paths.scripts + 'platform/util.file.js',
            //paths.scripts + 'platform/validate.js',
            paths.scripts + 'platform/window.js', //  <---- End of Optional features
            paths.scripts + 'utility/launchpad/launchpad.js'
        ],
        app: paths.app + '**/*.js',
        utility: [
            paths.scripts + 'utility/*.*',
            paths.scripts + 'utility/**/*.*'
        ]
    },

    less: {
        platform: [
            paths.css + 'platform.less',
            paths.app + 'Partials/UI/header.less'
        ],
        website: paths.css + 'website.less',
        app: [
            paths.app + '**/*.less'
        ],
        themes: paths.css + 'themes/*.less',
        tapestry: paths.css + 'tapestry/tapestry.less',
        utility: paths.css + 'utility/*.less'
    },

    css: {
        utility: paths.css + 'utility/**/*.css',
        themes: paths.themes + '**/*.css',
        app: paths.app + '**/*.css'
    },

    exclude: {
        app: [
            '!' + paths.app + 'wwwroot/**/',
            '!' + paths.app + 'Content/**/',
            '!' + paths.app + 'CSS/**/',
            '!' + paths.app + 'CSS/',
            '!' + paths.app + 'Scripts/**/'
        ]
    }
};

//compiled paths
paths.compiled = {
    platform: paths.webroot + 'js/platform.js',
    js: paths.webroot + 'js/',
    css: paths.webroot + 'css/',
    app: paths.webroot + 'css/',
    themes: paths.webroot + 'css/themes/'
};

//tasks for compiling javascript //////////////////////////////////////////////////////////////
gulp.task('js:app', function () {
    var pathlist = paths.working.exclude.app.slice(0);
    pathlist.unshift(paths.working.js.app);
    var p = gulp.src(pathlist)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));

    if (prod == true) { p = p.pipe(uglify()); }
    return p.pipe(gulp.dest(paths.compiled.js, { overwrite: true }));
});

gulp.task('js:platform', function () {
    var p = gulp.src(paths.working.js.platform, { base: '.' })
        .pipe(concat(paths.compiled.platform));
    if (prod == true) { p = p.pipe(uglify()); }
    return p.pipe(gulp.dest('.', { overwrite: true }));
});

gulp.task('js:utility', function () {
    //check file changes & replace changed files in destination
    return gulp.src(paths.working.js.utility)
        .pipe(changed(paths.compiled.js + 'utility'))
        .pipe(gulp.dest(paths.compiled.js + 'utility'));
});

gulp.task('js', function () {
    gulp.start('js:app');
    gulp.start('js:platform');
    gulp.start('js:utility');
});

//tasks for compiling LESS & CSS /////////////////////////////////////////////////////////////////////
gulp.task('less:app', function () {
    var pathlist = paths.working.exclude.app.slice(0);
    for (var x = paths.working.less.app.length - 1; x >= 0; x--) {
        pathlist.unshift(paths.working.less.app[x]);
    }
    var p = gulp.src(pathlist)
        .pipe(less())
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    if(prod == true){ p = p.pipe(cleancss({compatibility: 'ie8'})); }
    return p.pipe(gulp.dest(paths.compiled.app, { overwrite: true }));
});

gulp.task('less:platform', function () {
    var p = gulp.src(paths.working.less.platform)
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.compiled.css, { overwrite: true }));
});

gulp.task('less:website', function () {
    var p = gulp.src(paths.working.less.website)
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.compiled.css, { overwrite: true }));
});

gulp.task('less:themes', function () {
    var p = gulp.src(paths.working.less.themes)
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.compiled.css + 'themes', { overwrite: true }));
});

gulp.task('less:utility', function () {
    var p = gulp.src(paths.working.less.utility)
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.compiled.css + 'themes', { overwrite: true }));
});

gulp.task('css:themes', function () {
    var p = gulp.src(paths.working.css.themes)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.compiled.themes, { overwrite: true }));
});

gulp.task('css:app', function () {
    var pathlist = paths.working.exclude.app.slice(0);
    pathlist.unshift(paths.working.css.app);
    var p = gulp.src(pathlist)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.compiled.app, { overwrite: true }));
});

gulp.task('css:utility', function () {
    var p = gulp.src(paths.working.css.utility)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.compiled.css + 'utility', { overwrite: true }));
});

gulp.task('less', function () {
    gulp.start('less:platform');
    gulp.start('less:app');
    gulp.start('less:themes');
    gulp.start('less:utility');
});

gulp.task('css', function () {
    gulp.start('css:themes');
    gulp.start('css:app');
    gulp.start('css:utility');
});

//tasks for compiling default website content ////////////////////////////////////////////
gulp.task('default:website:less', function () {
    var p = gulp.src(paths.app + 'Content/pages/*.less')
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.webroot + 'content/pages/', { overwrite: true }));
});

gulp.task('default:website:js', function () {
    return gulp.src(paths.app + 'Content/pages/*.js')
        .pipe(gulp.dest(paths.webroot + 'content/pages/'));
});
gulp.task('default:website', ['default:website:less', 'default:website:js', 'less:website', 'js:app']);

//default task /////////////////////////////////////////////////////////////////////
gulp.task('default', ['js', 'less', 'css']);

//specific file task /////////////////////////////////////////////////////////////////////
gulp.task('file', function () {
    var path = (arg.path || arg.p).toLowerCase();
    var pathlist = path.split('/');
    var file = pathlist[pathlist.length - 1];
    var dir = pathlist.join('/').replace(file,'');
    var ext = file.split('.', 2)[1];
    var outputDir = paths.webroot + dir;
    console.log(path);
    console.log(file);
    console.log(ext);
    console.log(outputDir);
    var p = gulp.src('./App/' + path, { base: './App/' + dir });
    if (prod == true && ext == 'js') { p = p.pipe(uglify()); }
    if (ext == 'less') { p = p.pipe(less()); }
    if (prod == true && (ext == 'css' || ext == 'less')) {
        p = p.pipe(cleancss({ compatibility: 'ie8' }));
    }
    return p.pipe(gulp.dest(outputDir, { overwrite: true }));
});

//watch task /////////////////////////////////////////////////////////////////////
gulp.task('watch', function () {
    //watch platform JS
    gulp.watch(paths.working.js.platform, ['js:platform']);

    //watch app JS
    var pathjs = paths.working.exclude.app.slice(0);
    for (var x = 0; x < pathjs.length; x++) {
        pathjs[x] += '*.js';
    }
    pathjs.unshift(paths.working.js.app);
    gulp.watch(pathjs, ['js:app']);

    //watch utility JS
    gulp.watch(paths.working.js.utility, ['js:utility']);

    //watch app LESS
    var pathless = paths.working.exclude.app.slice(0);
    for (var x = 0; x < pathless.length; x++) {
        pathless[x] += '*.less';
    }
    for (var x = paths.working.less.app.length - 1; x >= 0; x--) {
        pathless.unshift(paths.working.less.app[x]);
    }
    gulp.watch(pathless, ['less:app']);

    //watch platform LESS
    gulp.watch([
        paths.working.less.platform,
        paths.working.less.tapestry
    ], ['less:platform']);

    //watch website LESS
    gulp.watch([
        [
            paths.working.less.website,
            paths.app + 'Partials/**/*.less'
        ]
    ], ['less:website']);

    //watch themes LESS
    gulp.watch([
        paths.working.less.themes
    ], ['less:themes', 'less:platform']);

    //watch utility LESS
    gulp.watch([
        paths.working.less.utility
    ], ['less:utility']);

    //watch app CSS
    var pathcss = paths.working.exclude.app.slice(0);
    for (var x = 0; x < pathcss.length; x++) {
        pathcss[x] += '*.css';
    }
    pathcss.unshift(paths.working.css.app);
    gulp.watch(pathcss, ['css:app']);

    //watch themes CSS
    gulp.watch([
        paths.working.css.themes
    ], ['css:themes']);

    //watch utility CSS
    gulp.watch([
        paths.working.css.utility
    ], ['css:utility']);
});