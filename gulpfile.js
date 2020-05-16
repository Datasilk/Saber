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
    cleancss = require('gulp-clean-css'),
    less = require('gulp-less'),
    rename = require('gulp-rename'),
    merge = require('merge-stream'),
    changed = require('gulp-changed'),
    config = require('./App/config.json'),
    exec = require('child_process').exec;
    
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
    scripts: 'App/Scripts/',
    css: 'App/CSS/',
    app: 'App/',
    webroot: 'App/wwwroot/',
};

//working paths
paths.working = {
    js: {
        platform: [
            paths.scripts + 'selector/selector.js',
            paths.scripts + 'utility/velocity.min.js',
            paths.scripts + 'platform/_super.js', // <---- Datasilk Core Js: S object
            paths.scripts + 'platform/ajax.js', //   <---- Optional platform features
            paths.scripts + 'platform/accordion.js',
            paths.scripts + 'platform/clipboard.js',
            paths.scripts + 'platform/loader.js',
            paths.scripts + 'platform/message.js',
            //paths.scripts + 'platform/polyfill.js',
            paths.scripts + 'platform/popup.js',
            paths.scripts + 'platform/view.js',
            paths.scripts + 'platform/svg.js',
            paths.scripts + 'platform/upload.js',
            paths.scripts + 'platform/util.js',
            paths.scripts + 'platform/util.color.js',
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
            paths.css + 'platform.less'
        ],
        app: [
            paths.app + '**/*.less'
        ],
        themes: paths.css + 'themes/*.less',
        tapestry: [
            paths.css + 'tapestry/tapestry.less',
            paths.css + 'tapestry/less/theme.less',
            paths.css + 'tapestry/less/util.less'
        ],
        utility: paths.css + 'utility/*.less'
    },

    css: {
        utility: [
            paths.css + 'utility/**/*.css',
            paths.scripts + 'utility/**/*.css'
        ],
        themes: paths.themes + '**/*.css',
        app: paths.app + '**/*.css'
    },

    exclude: {
        app: [
            '!' + paths.app + 'wwwroot/**/*',
            '!' + paths.app + 'Content/**/*',
            '!' + paths.app + 'CSS/**/*',
            '!' + paths.app + 'CSS/*',
            '!' + paths.app + 'Scripts/**/*',
            '!' + paths.app + 'obj/**/*',
            '!' + paths.app + 'bin/**/*'
        ]
    }
};

//compiled paths
paths.compiled = {
    platform: paths.webroot + 'editor/js/platform.js',
    js: paths.webroot + 'editor/js/',
    css: paths.webroot + 'editor/css/',
    app: paths.webroot + 'editor/css/',
    themes: paths.webroot + 'editor/css/themes/'
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

gulp.task('js', gulp.series('js:app', 'js:platform', 'js:utility'));

//tasks for compiling LESS & CSS /////////////////////////////////////////////////////////////////////
gulp.task('less:app', function () {
    var pathlist = [...paths.working.less.app, ...paths.working.exclude.app.map(a => a + '*.less')];
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

gulp.task('less', gulp.series('less:platform', 'less:app', 'less:themes', 'less:utility'));

gulp.task('css', gulp.series('css:themes', 'css:app', 'css:utility'));

//tasks for compiling default website content ////////////////////////////////////////////
gulp.task('website:less', function () {
    var p = gulp.src(paths.app + 'Content/pages/*.less')
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    p.pipe(gulp.dest(paths.webroot + 'content/pages/', { overwrite: true }));

    p = gulp.src(paths.app + 'Content/partials/*.less')
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.webroot + 'content/partials/', { overwrite: true }));
});

gulp.task('website:css', function () {
    var p = gulp.src(paths.app + 'CSS/website.less')
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss({ compatibility: 'ie8' })); }
    return p.pipe(gulp.dest(paths.webroot + 'css/', { overwrite: true }));
});

gulp.task('website:js', function () {
    var p = gulp.src(paths.app + 'Content/pages/*.js')
        .pipe(gulp.dest(paths.webroot + 'content/pages/', { overwrite: true }));
    p = gulp.src(paths.app + 'Content/partials/*.js')
        .pipe(gulp.dest(paths.webroot + 'content/partials/', { overwrite: true }));
    return p;
});
gulp.task('website', gulp.series('website:less', 'website:js', 'website:css'));

//generate icons //////////////////////////////////////////////////////////////////////////
gulp.task('icons', function () {
    exec('gulp svg --gulpfile Images/SVG/gulpfile.js', function (error, stdout, stderr) { });
    return gulp.src(paths.app + 'Content/pages/*.js');
});

//default task ////////////////////////////////////////////////////////////////////////////
gulp.task('default', gulp.series('js', 'less', 'css', 'icons'));

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
    gulp.watch(paths.working.js.platform, gulp.series('js:platform'));

    //watch app JS
    var pathjs = [paths.working.js.app, ...paths.working.exclude.app.map(a => a + '*.js')];
    gulp.watch(pathjs, gulp.series('js:app'));

    //watch utility JS
    gulp.watch(paths.working.js.utility, gulp.series('js:utility'));

    //watch app LESS
    var pathless = [...paths.working.less.app, ...paths.working.exclude.app.map(a => a + '*.less')];
    gulp.watch(pathless, gulp.series('less:app'));

    //watch platform LESS
    gulp.watch([
        ...paths.working.less.platform,
        ...paths.working.less.tapestry
    ], gulp.series('less:platform'));

    //watch themes LESS
    gulp.watch([
        paths.working.less.themes
    ], gulp.series('less:themes', 'less:platform'));

    //watch app CSS
    var pathcss = [paths.working.css.app, ...paths.working.exclude.app.map(a => a + '*.css')];
    gulp.watch(pathcss, gulp.series('css:app'));

});