# LaunchPad
A JavaScript library for seamlessly uploading multiple files from a web page.

### Example
```
    var fn = launchPad({
        buttons: document.getElementById('upload'),
        dropTargets: document.getElementById('droptarget'),
        url: '/upload',
        onUploadStart: function (files, xhr, data) {
            data.append('articleId', id);
        }
    });
```

### Options
| Argument |  | Default | Description |
| --- | --- | --- | --- |
| `dropTargets` | Optional | | An array of DOM elements used to drop files into from the user's PC |
| `buttons` | Optional | | An array of DOM elements used to display a file dialog (on click) |
| `url` | Required | | The URL to upload files to |
| `method` | Optional | `"POST"` | The method used to upload files with |
| `multipleUploads` | Optional | `true` | Allows the user to select multiple files within the file dialog |
| `directoryDialog` | Optional | `false` | Will display a directory dialog instead of a file dialog |
| `parallelUploads` | Optional | `2` | When uploading multiple files, determines how many files to include in each trip to the server |
| `maxFilesize` | Optional | `10240` | Determines the maximum filesize (in bytes) allowed to upload. NOTE: You should also configure your web server to limit request size for security purposes |
| `fileTypes` | Optional | | Restrict the user to upload specific file types. NOTE: You should also configure your web server to restrict acceptable file types for security purposes |
| `autoUpload` | Optional | `true` | Determines if files will be automatically uploaded to the web server after being selected, or if the user will need to take an extra action in order to upload files manually |
| `capture` | Optional | `false` | If true, the user will be prompted to use a device, such as a web cam or microphone, to generate a file to upload on the fly (instead of using a file dialog) |
| `thumbnailWidth` | Optional | `320` | Width of a thumbnail that will be generated if the user selects an image to upload |
| `thumbnailHeight` | Optional | `240` | Height of a thumbnail that will be generated if the user selects an image to upload |
| **Events** |
| `onUploadStart` | Optional | | An event raised before each trip to the server. Arguments include: iFormFile[], xhr, & FormData |
| `onUploadProgress` | Optional | | An event raised in intervals during the progress of an upload to the server. Arguments include: Event, percent |
| `onUploadComplete` | Optional | | An event raised every time a payload to the server completes |
| `onQueueComplete` | Optional | | An event raised when the entire upload queue is complete |

### Methods
| Name | Description |
| --- | --- |
| `upload` | Begins uploading queue to the specified URL. If `autoUpload` is set to `false`, use this method to manually upload files.  |

> NOTE: In the example above, `fn` refers to SpaceX' Falcon 9 Rocket for comedic purposes.

