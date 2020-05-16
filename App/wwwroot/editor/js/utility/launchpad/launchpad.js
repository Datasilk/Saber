(function () {
    var defaults = {
        //default options
        dropTargets: [], //array of elements that files can be dropped into
        buttons: [], //array of buttons that opens a file dialog
        url: '', //URL to upload files to
        method: 'POST', //method to use when uploading
        multipleUploads: true, //allow user to select multiple files in the file dialog
        directoryDialog: false, //allow user to select a directory instead of files
        parallelUploads: 2, //total uploads to process at a given time
        maxFilesize: 1024 * 10, //bytes * megabytes
        fileTypes: '', //filter specific file extensions
        autoUpload: true, //uploads files once selected or dropped
        capture: false, //used to capture a file from a "media capture mechanism", such as a webcam

        //thumbnails
        thumbnailWidth: 320,
        thumbnailHeight: 240,

        //events
        onUploadStart: null, //called for every upload in queue. (args = iFormFile[], xhr, FormData)
        onUploadProgress: null, //called in intervals when upload is in progress. (args = event, percent)
        onUploadComplete: null, 
        onQueueComplete: null
    };

    function uploader(options) {
        //instance options
        this.dropTargets = options.dropTargets || defaults.dropTargets;
        this.buttons = options.buttons || defaults.buttons;
        this.url = options.url || defaults.url;
        this.method = options.method || defaults.method;
        this.multipleUploads = options.multipleUploads === true ? options.multipleUploads : defaults.multipleUploads;
        this.parallelUploads = options.parallelUploads || defaults.parallelUploads;
        this.maxFilesize = options.maxFilesize || defaults.maxFilesize;
        this.fileTypes = options.fileTypes || defaults.fileTypes;
        this.autoUpload = options.autoUpload === true ? options.autoUpload : defaults.autoUpload;
        this.capture = options.capture === true ? options.capture : defaults.capture;

        //thumbnails
        this.thumbnailWidth = options.thumbnailWidth || defaults.thumbnailWidth;
        this.thumbnailHeight = options.thumbnailHeight || defaults.thumbnailHeight;

        //events
        this.onUploadStart = options.onUploadStart || defaults.onUploadStart;
        this.onUploadProgress = options.onUploadProgress || defaults.onUploadProgress;
        this.onUploadComplete = options.onUploadComplete || defaults.onUploadComplete;
        this.onQueueComplete = options.onQueueComplete || defaults.onQueueComplete;

        //files
        this.queue = [];

        //attach drop targets to drop event
        if (typeof this.dropTargets == 'Object') {
            this.dropTargets = [this.dropTargets];
        }
        for (var x = 0; x < this.dropTargets.length; x++) {

        }

        //attach buttons to click event
        if (typeof this.buttons == 'object' && this.buttons.style) {
            this.buttons = [this.buttons];
        }
        for (var x = 0; x < this.buttons.length; x++) {
            this.buttons[x].addEventListener('click', this.click.bind(this));
        }

        //generate hidden input field
        document.body.insertAdjacentHTML('beforeend',
            '<input type="file" id="hdnuploader"' +
            (this.multipleUploads == true ? ' multiple= "true"' : '') +
            (this.fileTypes != '' ? ' accept= "' + this.fileTypes + '"' : '') +
            (this.capture == true ? ' capture="true"' : '') +
            (this.directoryDialog == true ? ' webkitdirectory="true"' : '') +
            ' style= "display:none" ></input > '
        );

        //set up events for hidden input field
        var input = document.getElementById('hdnuploader');
        input.addEventListener('change', uploadChanged.bind(this));
    }

    uploader.prototype.click = function() {
        //show file dialog
        var input = document.getElementById('hdnuploader');
        input.click();
    }

    function uploadDrop() {
        //add dropped files to upload queue
    }

    function uploadChanged(e) {
        //add files to queue
        addFilesToQueue.call(this, e.target.files);
    }

    function addFilesToQueue(files) {
        for (var x = 0; x < files.length; x++) {
            if (this.queue.filter((a) => a.name == files[x].name).length == 0) {
                this.queue.push(files[x]);
            }
        }
        if (this.autoUpload === true) {
            //auto upload after adding new files to queue
            this.upload();
        }
    }

    uploader.prototype.upload = function () {
        //creates an XHR object and uploads files in queue
        var xhr = new XMLHttpRequest();
        xhr.addEventListener("progress", uploadProgress.bind(this));
        xhr.addEventListener("load", uploadComplete.bind(this));
        xhr.addEventListener("error", uploadError.bind(this));
        xhr.addEventListener("abort", uploadAbort.bind(this));

        //append files from queue
        var data = new FormData();
        var files = this.queue.splice(0, this.parallelUploads);
        //raise event so user can manipulate xhr or data if needed
        if (typeof this.onUploadStart == 'function') {
            this.onUploadStart(files, xhr, data);
        }
        
        for (var x = 0; x < files.length; x++) {
            data.append('file[' + x + ']', files[x], files[x].name);
        }

        //begin uploading
        xhr.open(this.method, this.url);
        xhr.send(data);
    }

    function uploadProgress(e) {
        if (e.lengthComputable) {
            var perc = e.loaded / e.total;

            //raise event so user can process progress
            if (typeof this.onUploadProgress == 'function') {
                this.onUploadProgress(e, perc);
            }
        }
    }

    function uploadComplete(e) {
        //raise event so user can process upload complete
        if (typeof this.onUploadComplete == 'function') {
            this.onUploadComplete();
        }
        if (this.queue.length == 0) {
            //raise event so user can process queue complete
            if (typeof this.onQueueComplete == 'function') {
                this.onQueueComplete();
            }
        } else {
            this.upload();
        }
    }

    function uploadError(e) {

    }

    function uploadAbort(e) {

    }
    
    //finally, expose uploader to the window as a global object
    window.launchPad = function (options) {
        return new uploader(options);
    };
})();