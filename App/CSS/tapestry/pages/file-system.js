(function(){
//generate different elements for all three interfaces
var filelist =  [
                {label:'Home', hasfolder: false, color:'blue', sort:'01', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Login', hasfolder: false, color:'yellow', sort:'02', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Contact Us', hasfolder: false, color:'yellow', sort:'03', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Error 404', hasfolder: false, color:'red', sort:'04', icondelete:false, iconsettings:true, iconadd:false, iconopen: true},
                {label:'Access Denied', hasfolder: false, color:'red', sort:'05', icondelete:false, iconsettings:true, iconadd:false, iconopen: true},
                {label:'Blog', hasfolder: true, color:'empty', sort:'99', icondelete:true, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Projects', hasfolder: true, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Biography', hasfolder: true, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Products', hasfolder: true, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'History', hasfolder: true, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Timeline', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Job Resume', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Friends', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Live Chat', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Locations', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Map', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Subscription', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Newsletter', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Forum', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Catalog', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Documentation', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Store', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Search', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Latest News', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Media Kit', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'References', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Awards', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Terms & Conditions', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Support', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Our Team', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Dashboard', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'Site Map', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true},
                {label:'RSS Feeds', hasfolder: false, color:'empty', sort:'99', icondelete:false, iconsettings:true, iconadd:true, iconopen: true}
                ];
filelist.sort(function(a, b){
    var c = (a.hasfolder ? '1' : '2') + a.sort.toString() + a.label;
    var d = (b.hasfolder ? '1' : '2') + b.sort.toString() + b.label;
    if (c < d) {
        return -1;
    }
    if (c > d) {
        return 1;
    }
    return 0;
});

//generate file list
var fileui = document.getElementById('filelist_ui');
var filecontainer = document.getElementById('filelist');
htm = fileui.innerHTML;
html = [''];
for(var y = 0; y < filelist.length; y++){
    html[0] =  renderTemplate(htm, 
            [
            ['{{id}}', y], 
            ['{{label}}', filelist[y].label], 
            ['{{color}}', filelist[y].color],
            ['{{iconfolder}}', filelist[y].hasfolder],
            ['{{icondelete}}', filelist[y].icondelete],
            ['{{iconsettings}}', filelist[y].iconsettings],
            ['{{iconadd}}', filelist[y].iconadd],
            ['{{iconopen}}', filelist[y].iconopen]
            ]);
    addHtm('li',filecontainer, html[0]);
}
fileui.parentNode.removeChild(fileui);

//resize the file list
Tapestry.on(window,'resize', resizeFileList);

function resizeFileList(){
    filecontainer.style.maxHeight = (window.innerHeight - 100) + 'px';
}

resizeFileList();
})();