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
    replace = require('gulp-replace'),
    del = require('del'),
    config = require('./App/config.json'),
    exec = require('child_process').exec,
    babel = require('gulp-babel'),
    gzip = require('gulp-gzip');
    
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
    release: 'App/bin/Release/net5.0/',
    publish: 'App/bin/Release/Saber/',
    publishapp: 'App/bin/Release/Saber/App/',
    sql: {
        release: 'Sql/bin/Release/'
    },
    vendors: 'App/Vendors/'
};

//working paths
paths.working = {
    js: {
        platform: [
            paths.scripts + 'utility/velocity.min.js',
            paths.scripts + 'selector/selector.js',
            paths.scripts + 'platform/_super.js', // <---- Datasilk Core Js: S object
            paths.scripts + 'platform/ajax.js', //   <---- Optional platform features
            paths.scripts + 'platform/accordion.js',
            paths.scripts + 'platform/clipboard.js',
            //paths.scripts + 'platform/drag.js',
            paths.scripts + 'platform/drag.sort.js',
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
        app: [
            paths.app + '**/*.js',
            '!' + paths.app + 'Vendors/**/*.js',
            '!' + paths.app + '**/node_modules/*.js'
        ],
        utility: [
            paths.scripts + 'utility/*.js',
            paths.scripts + 'utility/**/*.js',
            '!' + paths.scripts + 'utility/**/node_modules/*',
            //'!' + paths.app + 'utility/**/*.less'
        ],
        editor: [
            paths.scripts + 'editor/_super.js',
            paths.scripts + 'editor/resize.js',
            paths.scripts + 'editor/dropmenu.js',
            paths.scripts + 'editor/newwindow.js',
            paths.scripts + 'editor/changed.js',
            paths.scripts + 'editor/error.js',
            paths.scripts + 'editor/message.js',
            paths.scripts + 'editor/save.js',
            paths.scripts + 'editor/file.js',
            paths.scripts + 'editor/folder.js',
            paths.scripts + 'editor/sessions.js',
            paths.scripts + 'editor/tabs.js',
            paths.scripts + 'editor/explorer.js',
            paths.scripts + 'editor/filebar.js',
            paths.scripts + 'editor/codebar.js',
            paths.scripts + 'editor/components.js',
            paths.scripts + 'editor/fields.js',
            paths.scripts + 'editor/language.js',
            paths.scripts + 'editor/users.js',
            paths.scripts + 'editor/security.js',
            paths.scripts + 'editor/pagesettings.js',
            paths.scripts + 'editor/websettings.js',
            paths.scripts + 'editor/datasources.js',
            paths.scripts + 'editor/analytics.js',
            paths.scripts + 'editor/resources.js',
            paths.scripts + 'editor/hotkeys.js',
            paths.scripts + 'editor/signalr.js',
            paths.scripts + 'editor/events.js',
            paths.scripts + 'editor/utility.js',
            paths.scripts + 'editor/_init.js'
        ],
        iframe: paths.scripts + 'iframe.js',
        vendors: {
            editor: paths.vendors + '**/editor.js'
        }
    },

    less: {
        platform: [
            paths.css + 'platform.less'
        ],
        app: [
            paths.app + '**/*.less',
            '!' + paths.app + '**/node_modules/*',
            '!' + paths.app + 'Vendors/**/*.less'
        ],
        themes: paths.css + 'themes/*.less',
        tapestry: [
            paths.css + 'tapestry/tapestry.less',
            paths.css + 'tapestry/less/theme.less',
            paths.css + 'tapestry/less/util.less'
        ],
        utility: [
            paths.css + 'utility/*.less',
            '!' + paths.css + 'utility/**/node_modules/*.less'
        ],
        vendors: {
            editor: paths.vendors + '**/editor.less'
        }
    },

    css: {
        themes: paths.themes + '**/*.css',
        app: [
            paths.app + '**/*.css',
            '!' + paths.app + 'Vendors/**/*.css',
            '!' + paths.app + '**/node_modules/*.css'
        ],
        iframe:paths.app + 'CSS/iframe.css'
    },

    vendors: {
        resources: {
            js: [
                paths.app + 'vendors/**/*.js',
                '!' + paths.app + 'vendors/**/gulpfile.js',
                '!' + paths.app + 'vendors/**/editor.js',
                '!' + paths.app + 'vendors/**/node_modules/**/*.*'
            ],
            media: [
                paths.app + 'vendors/**/*.css',
                paths.app + 'vendors/**/*.svg',
                paths.app + 'vendors/**/*.jpg',
                paths.app + 'vendors/**/*.png',
                paths.app + 'vendors/**/*.gif',
                '!' + paths.app + 'vendors/**/node_modules/**/*.*'
            ]
        },
        less: [
            paths.app + 'vendors/**/*.less',
            '!' + paths.app + 'vendors/**/editor.less',
            '!' + paths.app + 'vendors/**/node_modules/**/*.less'
        ]
    },

    exclude: {
        app: [
            '!' + paths.app + 'wwwroot/**/*',
            '!' + paths.app + 'Content/**/*',
            '!' + paths.app + 'CSS/**/*',
            '!' + paths.app + 'CSS/*',
            '!' + paths.app + 'Scripts/**/*',
            '!' + paths.app + 'obj/**/*',
            '!' + paths.app + 'bin/**/*',
            '!' + paths.app + '**/node_modules/*'
        ]
    }
};

//compiled paths
paths.compiled = {
    platform: paths.webroot + 'editor/js/platform.js',
    editor: paths.webroot + 'editor/js/editor.js',
    js: paths.webroot + 'editor/js/',
    css: paths.webroot + 'editor/css/',
    app: paths.webroot + 'editor/css/',
    themes: paths.webroot + 'editor/css/themes/',
    vendors: paths.webroot + 'editor/vendors/'
};

//tasks for compiling javascript //////////////////////////////////////////////////////////////
gulp.task('js:app', function () {
    var pathlist = [...paths.working.js.app, ...paths.working.exclude.app];
    var p = gulp.src(pathlist)
    .pipe(rename(function (path) {
        path.dirname = path.dirname.toLowerCase();
        path.basename = path.basename.toLowerCase();
        path.extname = path.extname.toLowerCase();
    }));
    if (prod == true) {
        p = p.pipe(babel({
            presets: ['@babel/env']
        })).pipe(uglify());
    }
    p = p.pipe(gzip({ append: false }));
    return p.pipe(gulp.dest(paths.compiled.js, { overwrite: true }));
});

gulp.task('js:platform', function () {
    var p = gulp.src(paths.working.js.platform, { base: '.' })
        .pipe(concat(paths.compiled.platform));
    if (prod == true) {
        p = p.pipe(babel({
            presets: ['@babel/env']
        })).pipe(uglify());
    }
    p = p.pipe(gzip({ append: false }));
    return p.pipe(gulp.dest('.', { overwrite: true }));
});

gulp.task('js:editor', function () {
    var p = gulp.src(paths.working.js.editor, { base: '.' })
        .pipe(concat(paths.compiled.editor));
    if (prod == true) {
        p = p.pipe(babel({
            presets: ['@babel/env']
        })).pipe(uglify());
    }
    p = p.pipe(gzip({ append: false }));
    return p.pipe(gulp.dest('.', { overwrite: true }));
});

gulp.task('js:utility', function () {
    var p = gulp.src(paths.working.js.utility)
        .pipe(gzip({ append: false }))
    .pipe(gulp.dest(paths.compiled.js + 'utility'));

    return gulp.src([paths.scripts + 'utility/**/*.*', '!' + paths.scripts + 'utility/*.js', '!' + paths.scripts + 'utility/**/*.js'])
        .pipe(gulp.dest(paths.compiled.js + 'utility'));
});

gulp.task('js:iframe', function () {
    return gulp.src(paths.working.js.iframe)
        .pipe(gzip({ append: false }))
        .pipe(gulp.dest(paths.compiled.js));
});

gulp.task('js:selector', function () {
    return gulp.src(paths.scripts + 'selector/selector.js')
        .pipe(gzip({ append: false }))
        .pipe(gulp.dest(paths.compiled.js));
});

gulp.task('js', gulp.series('js:app', 'js:platform', 'js:editor', 'js:utility', 'js:iframe', 'js:selector'));

//tasks for compiling LESS & CSS /////////////////////////////////////////////////////////////////////
gulp.task('less:app', function () {
    var pathlist = [...paths.working.less.app, ...paths.working.exclude.app];
    var p = gulp.src(pathlist)
        .pipe(less())
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    //if(prod == true){ p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.app, { overwrite: true }));
});

gulp.task('less:platform', function () {
    var p = gulp.src(paths.working.less.platform)
        .pipe(less());
    //if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.css, { overwrite: true }));
});

gulp.task('less:themes', function () {
    var p = gulp.src(paths.working.less.themes)
        .pipe(less());
    //if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.css + 'themes', { overwrite: true }));
});

gulp.task('less:utility', function () {
    var p = gulp.src(paths.working.less.utility)
        .pipe(less());
    //if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.css + 'themes', { overwrite: true }));
});

gulp.task('css:themes', function () {
    var p = gulp.src(paths.working.css.themes)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    //if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.themes, { overwrite: true }));
});

gulp.task('css:app', function () {
    var pathlist = [...paths.working.css.app, ...paths.working.exclude.app];
    var p = gulp.src(pathlist)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    //if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.app, { overwrite: true }));
});

gulp.task('css:iframe', function () {
    var p = gulp.src(paths.working.css.iframe);
    //if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.css, { overwrite: true }));
});

gulp.task('less', gulp.series('less:platform', 'less:app', 'less:themes', 'less:utility'));

gulp.task('css', gulp.series('css:themes', 'css:app', 'css:iframe'));

//tasks for compiling default website content ////////////////////////////////////////////
gulp.task('website:less', function () {
    var p = gulp.src(paths.app + 'Content/pages/*.less')
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss()); }
    p.pipe(gulp.dest(paths.webroot + 'content/pages/', { overwrite: true }));

    p = gulp.src(paths.app + 'Content/partials/*.less')
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.webroot + 'content/partials/', { overwrite: true }));
});

gulp.task('website:css', function () {
    var p = gulp.src(paths.app + 'Content/website.less')
        .pipe(less());
    if (prod == true) { p = p.pipe(cleancss()); }
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

//copy vendor resources ///////////////////////////////////////////////////////////////////
gulp.task('vendors:resources', function () {
    var p = gulp.src(paths.working.vendors.resources.media)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));

    var p = gulp.src(paths.working.vendors.resources.js)
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));

    if (prod == true) { p = p.pipe(uglify()); }
    p = p.pipe(gzip({ append: false }));
     
    return p.pipe(gulp.dest(paths.compiled.vendors, { overwrite: true }));
});

gulp.task('vendors:less', function () {
    var p = gulp.src(paths.working.vendors.less)
        .pipe(less())
        .pipe(rename(function (path) {
            path.dirname = path.dirname.toLowerCase();
            path.basename = path.basename.toLowerCase();
            path.extname = path.extname.toLowerCase();
        }));
    if (prod == true) { p = p.pipe(cleancss()); }
    return p.pipe(gulp.dest(paths.compiled.vendors, { overwrite: true }));
});

gulp.task('vendors:editor.js', function () {
    var p = gulp.src(paths.working.js.vendors.editor, { base: '.' })
        .pipe(concat(paths.compiled.js + 'vendors-editor.js'));
    if (prod == true) {
        p = p.pipe(babel({
            presets: ['@babel/env']
        })).pipe(uglify());
    }
    p = p.pipe(gzip({ append: false }));
    return p.pipe(gulp.dest('.', { overwrite: true }));
});

gulp.task('vendors:editor.less', function () {
    var p = gulp.src(paths.working.less.vendors.editor, { base: '.' })
        .pipe(less())
        .pipe(concat(paths.compiled.css + 'vendors-editor.css'));
    return p.pipe(gulp.dest('.', { overwrite: true }));
});

gulp.task('vendors', gulp.series('vendors:resources', 'vendors:less', 'vendors:editor.js', 'vendors:editor.less'));

//default task ////////////////////////////////////////////////////////////////////////////
gulp.task('default', gulp.series('js', 'less', 'css', 'icons', 'vendors'));

//specific file task //////////////////////////////////////////////////////////////////////
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
        p = p.pipe(cleancss());
    }
    return p.pipe(gulp.dest(outputDir, { overwrite: true }));
});

//watch task /////////////////////////////////////////////////////////////////////
gulp.task('watch', function () {
    //watch platform JS
    gulp.watch(paths.working.js.platform, gulp.series('js:platform'));

    //watch editor JS
    gulp.watch(paths.working.js.editor, gulp.series('js:editor'));

    //watch app JS
    var pathjs = [...paths.working.js.app, ...paths.working.exclude.app.map(a => a + '*.js')];
    gulp.watch(pathjs, gulp.series('js:app'));

    //watch utility JS
    gulp.watch(paths.working.js.utility, gulp.series('js:utility'));

    //watch iframe JS
    gulp.watch(paths.working.js.iframe, gulp.series('js:iframe'));

    //watch vendors/**/editor.js
    gulp.watch(paths.working.js.vendors.editor, gulp.series('vendors:editor.js'))

    //watch vendors/**/editor.less
    gulp.watch(paths.working.less.vendors.editor, gulp.series('vendors:editor.less'))

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
    var pathcss = [...paths.working.css.app, ...paths.working.exclude.app.map(a => a + '*.css')];
    gulp.watch(pathcss, gulp.series('css:app'));

    //watch iframe CSS
    gulp.watch(paths.working.css.iframe, gulp.series('css:iframe'));

});

//publish task ////////////////////////////////////////////////////////////////////
function publishStep1(platform) {
    if (platform != null) { platform = '-' + platform + '/';}
    //copy data to publish folder
    gulp.src(['Publish/README.md'])
        .pipe(gulp.dest(paths.publish));

    gulp.src([
        'App/wwwroot/**/*',
        'App/wwwroot/*',
        'App/Content/temp/*',
        'App/Content/temp/**',
        'App/Views/**/*.html',
        'App/Vendors/README.md'
    ], { base: 'App' })
        .pipe(gulp.dest(paths.publishapp));

    gulp.src([paths.release + platform + 'Content/temp/config.prod.json'])
        .pipe(gulp.dest(paths.publishapp));

    return gulp.src([
        paths.release + platform + '*',
        paths.release + platform + '**'
    ], { base: paths.release + platform })
        .pipe(gulp.dest(paths.publishapp));
}

gulp.task('publish:step-1', function () {
    return publishStep1();
});
gulp.task('publish:step-2', function () {
    //copy sql .pipe(replace("{{version}}", version_new))

    gulp.src(paths.sql.release + 'Saber_Create.sql')
        .pipe(replace(':setvar DatabaseName "Sql"', ':setvar DatabaseName "Saber"'))
        .pipe(replace(':setvar DefaultFilePrefix "Sql"', ':setvar DefaultFilePrefix "Saber"'))
        .pipe(gulp.dest(paths.publish + 'Sql'));

    return gulp.src([paths.sql.release + 'Saber.dacpac'])
        .pipe(gulp.dest(paths.publish + 'Sql'));
});

gulp.task('publish:step-3', function () {
    //delete unwanted files from release folder
    return del([
        paths.publishapp + 'Vendors/*',
        paths.publishapp + 'Vendors/**',
        '!' + paths.publishapp + 'Vendors/README.md',
        paths.publishapp + 'wwwroot/**',
        '!' + paths.publishapp + 'wwwroot/editor',
        '!' + paths.publishapp + 'wwwroot/editor/**/*',
        paths.publishapp + 'wwwroot/editor/js/vendors',
        paths.publishapp + 'wwwroot/editor/css/vendors',
        paths.publishapp + 'Content/**',
        '!' + paths.publishapp + 'Content/temp',
        paths.publishapp + 'Content/temp/README.md',
        paths.publishapp + 'CSS',
        paths.publishapp + 'Scripts',
        paths.publishapp + 'config.json',
        paths.publishapp + 'web.*.config',
    ]);
});

gulp.task('publish', gulp.series('publish:step-1', 'publish:step-2', 'publish:step-3'));