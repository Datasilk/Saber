![Saber Logo](http://www.markentingh.com/projects/saber/logo.png)

# Saber

A simple, straight forward CMS and IDE. 

Saber was built with a focus on traditional web development by utilizing HTML, CSS (LESS), and Javascript in their respective file formats. On top of that, Saber gives the developer the ability to make live updates to their website while using an integrated development environment (IDE) inside their favorite web browser.

## Requirements

* Visual Studio 2019
* ASP.NET Core 3.0
* SQL Server 2017 (or greater)
* Node.js
* Gulp

## Installation

1. Clone the repository:

    ```git clone --recurse-submodules http://github.com/Datasilk/Saber```

2. Run command `npm install`
3. Run command `gulp default` to generate all required `css` & `js` files into the public `wwwroot` folder
4. In Visual Studio, build then publish the **Sql** project to SQL Server
5. Open `config.json` file and update the Sql connection string
6. Click Play in Visual Studio & navigate to https://localhost:7070

#### Docker Support
Saber also supports Docker. In order for Saber to work with Docker in Windows, you must first install and run [Docker Desktop](https://docs.docker.com/docker-for-windows/). 
1. Open `config.docker.json` file and update the Sql connection string Initial Catalog along with the User ID & Password you've created in Sql Server that has access to your Saber database
2. Click **Play** in Visual Studio after selecting the **Docker** launch command from the drop down

#### IIS Support
You will need to make a simple code change in `Program.cs` to support IIS integration.
1. In `Program.cs`, remove or comment the following code:

``` csharp
.UseKestrel(
    options =>
    {
        options.Limits.MaxRequestBodySize = null;
        //options.ListenAnyIP(80); //for docker
    }
)
```
2. In `Program.cs`, uncomment the following code:

``` csharp
.UseIISIntegration()
```
----

![Saber IDE](http://www.markentingh.com/projects/saber/saber-html-file.jpg)
*Screenshot of Saber's Editor UI*

## Features
Build web pages from within your web browser using a built-in IDE for editing HTML, CSS, LESS, & Javascript files.

#### Navigate to any URL 
Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML & CSS within the built-in IDE. The editor initially opens 3 files (HTML, LESS, & JS) that are resources for the web page being viewed.

#### Create & Modify Website Resources
Use the **file browser** within the built-in IDE to open website resources (HTML, CSS, LESS, & JS files) in new tabs. Use the *File* drop down menu to open the file browser or create new files & folders. The initial folder structure is described below: 

* **wwwroot** is a public folder where you can upload files & images to utilize within your website
* **pages** is a server-side folder used to store all pages belonging to your website. Clicking on an HTML page in the file browser will navigate to that page within your website for editing.
* **partials** is a server-side folder used to store HTML files that can be included within web pages throughout your website (such as *header.html* & *footer.html* files)
* **website.less** is compiled into a CSS file and loaded on every page within your website

#### Upload Files & Photos
You can upload image files & other resources for a specific web page or within the **wwwroot** folder to be used globally. Manage your uploads within an image-gallery style file browser.

#### Multi-lingual Page Content
* Generate text fields by adding *mustache* variables to your HTML page (e.g. `<div>{{article-title}}</div>`)
* Fill out the text fields with your content, which will replace the variables in the HTML page. (e.g. `Insert Article Title Here`)
* Select which language the content will be written in and allow visitors to change their desired language 
* Use Markdown syntax in text fields that will later be rendered as HTML
* Display a language selection drop down list on your website by adding the following HTML code:
```
<form id="changelang" method="post">
	<select name="lang" onchange="changelang.submit()">{{language-options}}</select>
</form>
```

#### Include partial HTML files
Use *mustache* variables to include html files inside other html files and render robust web pages. 
	
For example:
	
```
{{header "Partials/UI/header.html"}}
```
`header` is a variable name and the relative file path is in quotes.
> Note: The relative path to all files are case-sensitive 

#### Template Web Pages
You can create a template web page (e.g. `https://yoursite.com/support/template`) and design the template page to be used as a starting point when creating new sub-pages. When navigating to a new URL (e.g. `https://yoursite.com/support/my-new-page`, if the template URL exists within the same path (e.g. `https://yoursite.com/support/template`), the new URL's web page will copy the design & content of the template URL's web page. This is useful when managing complex websites such as a wiki, blog, or storefront.

#### Include Vendor Plugins
Use *mustache* variables to load a custom vendor plugin within your web page
	
For example:

```
{{page-list path:"blog", length:"4"}}
```
The above example will display a list of blog pages that exists within your website. 

Before this can be achieved, though, you must install the [PageList](https://github.com/Datasilk/Saber-PageList/) plugin into the `/App/Vendors` folder.

##### Develop Vendor Plugins
You can learn how to develop Vendor plugins and find a list of supported Vendor plugins and links to their repositories online at [/Vendor/README.md](Vendor/README.md).

##### Installing a Vendor Plugin
All vendor plugins **must** be installed within the `/App/Vendors` folder. For example:

```git clone https://github.com/Datasilk/Saber-CORS App/Vendors/CORS```

#### Code Editor
Saber uses [Monaco](https://microsoft.github.io/monaco-editor/) as its code editor

* Minimap next to scrollbar
* Intellisense for HTML, CSS, LESS, & Javascript
* Syntax Highlighting for HTML, CSS, LESS, & Javascript
* Code Folding
* Commands (F1)

#### Shortcut Keys
* Ctrl + S (save)
* Escape (toggle editor / website preview)
* F1 (Text Editor Command window)

#### User Security
Saber includes a robust security system so administrators can give users permissions to specific features & web pages.

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
* Manage a list of acceptable meta title prefixes and suffixes to make sure your website has consistentpage titles
    * Select which prefix or suffix to add to individual web page titles & descriptions via the page settings
* Create multiple headers & footers for your website and select which ones to use within each individual page's settings
* Include custom Javascript files on specific pages within your website

## Under The Hood
Saber uses many technologies developed by [Mark Entingh](http://www.github.com/markentingh), including [Datasilk Core MVC](http://www.github.com/datasilk/core) as the MVC middleware for ASP.NET Core, [Tapestry](http://www.github.com/websilk/tapestry) for frontend CSS UI design, [Datasilk Core JS](http://www.github.com/datasilk/corejs) as a frontend JavaScript framework, and [Selector](http://www.github.com/websilk/selector) as a replacement for jQuery at only 5kb in size.

## Future Development
* Publish content using other formats besides HTML & CSS, such as uploading a PDF, DOC, DOCX, MP3 (for auto-transcribing), or Excel spreadsheet.
* Upload a cover photo to use when sharing with Facebook, Twitter, and other social platform
* Other optional meta data fields used within the page settings
  * Fields from schema.org (JSON-LD)
  * product related fields (price, saleprice, colors, specs)
