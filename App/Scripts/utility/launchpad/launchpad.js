(function () {
    var defaults = {
        //default options
        dropTargets: [], //array of elements that files can be dropped into
        buttons: [], //array of buttons that opens a file dialog
        multipleUploads: true,
        parallelUploads: 2,
        maxFilesize: 1024 * 10, //bytes * megabytes
        fileTypes: [], //filter specific file extensions
        autoUpload: true, //uploads files once selected or dropped

        //thumbnails
        thumbnailWidth: 320,
        thumbnailHeight: 240,

        //events
        onUploadStart: null,
        onUploadProgress: null,
        onUploadComplete: null,
        onQueueComplete: null
    };

    function uploader(options) {
        //instance options
        this.dropTargets = options.dropTargets || defaults.dropTargets;
        this.buttons = options.buttons || defaults.buttons;
        this.multipleUploads = options.multipleUploads || defaults.multipleUploads;
        this.parallelUploads = options.parallelUploads || defaults.parallelUploads;
        this.maxFilesize = options.maxFilesize || defaults.maxFilesize;
        this.fileTypes = options.fileTypes || defaults.fileTypes;
        this.autoUpload = options.autoUpload || defaults.autoUpload;

        //thumbnails
        this.thumbnailWidth = options.thumbnailWidth || defaults.thumbnailWidth;
        this.thumbnailHeight = options.thumbnailHeight || defaults.thumbnailHeight;

        //events
        this.onUploadStart = options.onUploadStart || defaults.onUploadStart;
        this.onUploadProgress = options.onUploadProgress || defaults.onUploadProgress;
        this.onUploadComplete = options.onUploadComplete || defaults.onUploadComplete;
        this.onQueueComplete = options.onQueueComplete || defaults.onQueueComplete;

        //attach drop targets to drop event
        if (typeof this.dropTargets == 'Object') {
            this.dropTargets = [this.dropTargets];
        }
        for (var x = 0; x < this.dropTargets.length; x++) {

        }

        //attach buttons to click event
        console.log(typeof this.buttons);
        if (typeof this.buttons == 'object') {
            this.buttons = [this.buttons];
        }
        for (var x = 0; x < this.buttons.length; x++) {
            console.log(this.buttons[x]);
            this.buttons[x].addEventListener('click', uploadClick);
        }

        //generate hidden input field
        document.body.insertAdjacentHTML('beforeend', '<input type="file" id="hdnuploader" multiple="multiple" style="display:none"></input>');

        //set up events for hidden input field
        var input = document.getElementById('hdnuploader');
        input.addEventListener('select', filesSelected);
    }

    function uploadClick() {
        //show file dialog
        console.log('click');
        var input = document.getElementById('hdnuploader');
        input.click();
    }

    function uploadDrop() {
        //add dropped files to upload queue
    }

    function filesSelected(e) {
        console.log(e);
    }
    
    //finally, expose uploader to the window as a global object
    window.launchPad = uploader;
})();