![Saber Logo](http://www.markentingh.com/projects/saber/logo.png)

# Saber

A simple, straight forward CMS and IDE. 

Saber was built with a focus on traditional web development by utilizing HTML, CSS (LESS), and Javascript in their respective file formats. On top of that, Saber gives the developer the ability to make live updates to their website while using an integrated development environment (IDE) inside their favorite web browser.

## Requirements

* Visual Studio 2019
* ASP.NET Core 3.0
* SQL Server 2008 (or greater)
* Node.js
* Gulp

## Installation

1. Clone the repository:

    ```git clone --recurse-submodules http://github.com/datasilk/saber```

2. In Visual Studio, build & publish the SQL project to SQL Server with your own database name
3. Select the "Saber" project from within the Solution Explorer, then click the drop down arrow to the right-hand side of the **Play** button and select the **App** launch command, then click the **Play** button. This will generate a `config.json` file and will also generate the published website using a template. When the output shows the message `Running Saber Server in development environment`, click the Stop button.
4. Open the new `config.json` file and update the Sql connection string
5. Run command `npm install`
6. Run command `gulp default` to generate all required `css` & `js` files into the public `wwwroot` folder
7. Run command `gulp website` to copy all `html`, `less`, `js`, and media files for the newly generated website into the public `wwwroot` folder
8. Click Play in Visual Studio & navigate to https://localhost:7070

#### Docker Support
Saber also supports Docker. In order for Saber to work with Docker in Windows, you must first install and run [Docker Desktop](https://docs.docker.com/docker-for-windows/). Then, click **Play** in Visual Studio after selecting the **Docker** launch command from the drop down.

![Saber IDE](http://www.markentingh.com/projects/saber/saber-html-file.jpg)
*Screenshot of Saber's Editor UI*

## Features
Build web pages from within your web browser using a built-in IDE for editing HTML, CSS, LESS, & Javascript files.

#### Navigate to any URL 
Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML & CSS within the built-in IDE. The editor initially opens 3 files (HTML, LESS, & JS) that are resources for the web page being viewed.

#### Create & Modify Website Resources
Use the **file browser** within the built-in IDE to open website resources (HTML, CSS, LESS, & JS files) in new tabs. Use the *File* drop down menu to open the file browser or create new files & folders. The initial folder structure is described below: 

* **wwwroot** is a public folder where you can upload files & images to utilize within your website
* **partials** is a server-side folder used for partial HTML files that are included within web pages throughout your website (such as  *header.html* & *footer.html* files)
* **CSS/website.less** is compiled to `wwwroot/css/website.css` using `gulp` and is loaded on every page within your website
* **Scripts** is a server-side folder that contains various Javascript libraries used within the Saber editor
	* **Scripts/website.js** is loaded on every page within your website and can be modified within *Saber's Editor IDE*

#### Upload Files & Photos
You can upload image files & other resources for a specific web page or within the **wwwroot** folder to be used globally. Manage your uploads within a photo-gallery-style file explorer.

#### Multi-lingual Page Content
* Generate text fields by adding *mustache* variables to your HTML page (e.g. `<div>{{article-title}}</div>`)
* Fill out the text fields with your content, which will replace the variables in the HTML page. (e.g. `Article Title`)
* Select which language the content will be written in and allow visitors to change their desired language 
* Use Markdown syntax in text fields that will later be rendered as HTML

#### Include partial HTML files
Use *mustache* variables to include html files inside other html files and render robust web pages. 
	
For example:
	
```
{{header "Partials/UI/header.html"}}
```
`header` is a custom variable name and the file path is in quotes

#### Template Web Pages
You can create a template web page (e.g. `https://yoursite.com/support/template`) and design the template page to be used when creating new sub-pages. When navigating to a new URL (e.g. `https://yoursite.com/support/my-new-page`, if the template URL exists within the same path (e.g. `https://yoursite.com/support/template`), the new URL's web page will copy the design & content of the template URL's web page. This is useful when managing complex websites such as a wiki, blog, or storefront.

> Q&A: Where can I create Template pages? They can be created within any URL path in your website

#### Include Vendor Components
Use *mustache* variables to load a custom vendor plugin within your web page
	
For example:

```
{{page-list path:"blog", length:"4"}}
```
The above example will display a list of blog pages that exists within your website. 

Before this can be achieved, though, you must install the [PageList](https://github.com/Datasilk/Saber-PageList/) plugin into the `/App/Vendor` folder.

##### Vendor Components Library
You can find a list of Vendor Components and links to their repositories online at [/App/Vendor/README.md](App/Vendor/README.md).

##### Installing a Vendor Component
All vendor components **must** be installed within the `/App/Vendor` folder. For example:

```git clone https://github.com/Datasilk/Saber-CORS App/Vendor/CORS```

#### Code Editor
Saber uses [Monaco](https://microsoft.github.io/monaco-editor/) as its code editor

* Minimap next to scrollbar
* Intellisense for HTML, CSS, LESS, & Javascript
* Syntax Highlighting for HTML, CSS, LESS, & Javascript
* Code Folding
* Commands (F1)

#### Shortcut Keys
* Ctrl + S (save)
* Escape (toggle editor / preview)
* F1 (Text Editor Command window)

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-html-closeup.jpg)

Edit page resources for any URL within your domain name

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-file-browser.jpg)

Browse server-side files and edit them via the IDE within Saber

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-scrollbar.jpg)

Use a minimap to scroll through your code faster than ever

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-photo-gallery.jpg)

Upload images & other resources

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-content-fields.jpg)

Generate form fields by writing HTML variables (e.g. `<h2>{{hero-title}}</h2>`) and use them to fill out content for your web pages

### Even More Features!
* Upload & manage photos & files related to a specific web page
* Manage a list of acceptable meta title & description prefixes and suffixes.
    * Select which prefix or suffix to add to individual web page titles & descriptions via the page settings
* Create multiple headers & footers for your website and select which ones to use within each individual page's settings

## Under The Hood
Saber uses many technologies developed by [Mark Entingh](http://www.github.com/markentingh), including [Datasilk Core MVC](http://www.github.com/datasilk/core) as the MVC middleware for ASP.NET Core, [Tapestry](http://www.github.com/websilk/tapestry) for frontend CSS UI design, [Datasilk Core JS](http://www.github.com/datasilk/corejs) as a frontend JavaScript framework, and [Selector](http://www.github.com/websilk/selector) as a replacement for jQuery at only 5kb in size.

## Future Development
* Publish content using other formats besides HTML & CSS, such as uploading a PDF, DOC, DOCX, MP3 (for auto-transcribing), or Excel spreadsheet.
* Upload a cover photo to use when sharing with Facebook, Twitter, and other social platforms
* Other optional meta data fields used within the page settings
  * Fields from schema.org (JSON-LD)
  * product related fields (price, saleprice, colors, specs)