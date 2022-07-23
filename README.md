![Saber Logo](http://www.markentingh.com/projects/saber/logo.png)

# Saber

A simple, straight forward CMS and IDE. 

Saber gives software engineers the ability to focus on traditional web development by writing HTML, CSS (LESS), and Javascript in their respective file formats. On top of that, Saber gives software engineers the ability to make live updates to their website while using an integrated development environment (IDE) inside their favorite web browser.

## Requirements

* Visual Studio 2019
* ASP.NET Core 5.0
* SQL Server 2017 (or greater)
* Node.js
* Gulp

## Installation

1. Clone the repository and get all submodules:

    ```
    git clone --recurse-submodules http://github.com/Datasilk/Saber
    git submodule foreach git checkout master
    git submodule foreach git submodule update --init
    ```

2. Run command `npm install`
3. Run command `gulp default`
4. In Visual Studio, build then publish the **Sql** project to SQL Server
5. Open `config.json` file and update the Sql connection string
6. Click Play in Visual Studio & navigate to https://localhost:7070

#### Docker Support
Saber also supports Docker. In order for Saber to work with Docker in Windows, you must first install and run [Docker Desktop](https://docs.docker.com/docker-for-windows/). 
1. Copy `/App/Content/temp/config.docker.json` to `/App` folder and open the file to update the Sql connection string Initial Catalog along with the User ID & Password you've created in Sql Server that has access to your Saber database
2. Click **Play** in Visual Studio after selecting the **Docker** launch command from the drop down

#### IIS Support
Saber now supports IIS natively
----

![Saber IDE](http://www.markentingh.com/projects/saber/saber-html-file.jpg)
*Screenshot of Saber's Editor UI*

## Features
Build web pages from within your web browser using a built-in IDE for editing HTML, CSS, LESS, & JavaScript files.

#### Navigate to any URL 
Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML, CSS, & JavaScript within the built-in IDE. The editor initially opens 3 files (HTML, LESS, & JS) that are associated with the web page being viewed.

#### Create & Modify Website Resources
Use the **file browser** within the built-in IDE to open website resources (HTML, CSS, LESS, & JS files) in new tabs. Use the *File* drop down menu to open the file browser or create new files & folders. The initial folder structure is described below: 

* **wwwroot** is a public-facing folder where you can upload files & images to utilize within your website
* **pages** is a protected folder used to store all pages belonging to your website. Clicking on an HTML page in the file browser will navigate to that page within your website for editing.
* **partials** is a protected folder used to store HTML files that can be included within web pages throughout your website (such as *header.html* & *footer.html* files)
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
	<select name="lang" onchange="changelang.submit()">{{languages.options}}</select>
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
You can create a template web page (e.g. `https://yoursite.com/support/template`) and design the template page to be used as a starting point when creating new sub-pages. When navigating to a new URL within your website (e.g. `https://yoursite.com/support/my-new-page`), Saber will copy & load the design & content from the associated template web page when the URL is accessed for the first time. This is useful when managing complex websites such as a wiki, blog, or storefront.

#### Include Vendor Plugins
Use *mustache* variables to load a custom vendor plugin within your web page.
	
For example:

```
{{page-list (path:"blog", length:"4")}}
```
The above example will display a list of blog pages that exists within your website. 

Before this can be achieved, though, you must install the [PageList](https://github.com/Datasilk/Saber-PageList/) plugin into the `/App/Vendors` folder.

##### Develop Vendor Plugins
You can learn how to develop Vendor plugins and find a list of supported Vendor plugins and links to their repositories online at [saber.datasilk.io](https://saber.datasilk.io/developers.html) and [/Vendor/README.md](Vendor/README.md).

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
* F2 Page Content tab
* F3 Page Settings tab
* F4 Page Resouces tab
* F6 Website Settings tab
* F7 User Management tab
* F8 Security Groups tab
* F9 toggle File Browser

#### User Security
Saber includes a robust security system so administrators can give users permissions to specific features & web pages.
Create Security Groups to manage permissions to features within Saber, then assign users to your Security Groups. 
Assign one or more Security Groups to a web page to make the page secure. All secure pages that are accessed by users without proper permissions will be shown `pages/access-denied.html` instead of the desired page.

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
* Include custom Javascript & Stylesheet files on specific pages within your website or on every page within your website
* Create lists of content on your web pages by using the List Component to select one or more partial views to render for your list items.

## Under The Hood
Saber uses many technologies developed by [Mark Entingh](http://www.github.com/markentingh), including [Datasilk Core MVC](http://www.github.com/datasilk/core) as the MVC middleware for ASP.NET Core, [Tapestry](http://www.github.com/datasilk/tapestry) for frontend CSS UI design, [Datasilk Core JS](http://www.github.com/datasilk/corejs) as a frontend JavaScript framework, and [Selector](http://www.github.com/datasilk/selector) as a replacement for jQuery at only 5kb in size.

## Future Development
* Publish content using other formats besides HTML & CSS, such as uploading a PDF, DOC, DOCX, MP3 (for auto-transcribing), or Excel spreadsheet.
* Upload a cover photo to use when sharing with Facebook, Twitter, and other social platform
* Other optional meta data fields used within the page settings
  * Fields from schema.org (JSON-LD)
  * product related fields (price, saleprice, colors, specs)
