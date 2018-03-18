![Saber Logo](http://www.markentingh.com/projects/saber/logo.png)

# Saber

A simple, straight forward CMS.

## Requirements

* Visual Studio 2017
* ASP.NET Core 2.0
* SQL Server 2016
* Node.js
* Gulp

## Installation

1. Clone the repository:

    ```git clone --recurse-submodules http://github.com/datasilk/saber```

2. Run command ```npm install```
3. Run command ```gulp default```
4. In Visual Studio, publish the SQL project to SQL Server 2016 (or greater), with your own database name
5. Open `config.json` and make sure the database connection string for property `SqlServerTrusted` points to your database.
6. Click Play in Visual Studio 2017

![Saber IDE](http://www.markentingh.com/projects/saber/saber-html-file.jpg)


## Features
* Build web pages from within your web browser using a built-in IDE for editing HTML, CSS, LESS, & Javascript
* Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML & CSS within the built-in IDE
* Browse server-side resources (HTML, CSS, LESS, & JS files) and open them in new tabs within the code editor
* The editor initially opens 3 files (HTML, LESS, & JS) that are resources for the web page being viewed
	* after editing & saving changes to page resources, a live update happens to the page when switching to preview mode
* Multi-lingual Page Content
	* Generate text fields by adding variables to your HTML page (e.g. `<div>{{article-title}}</div>`)
	* Fill out the text fields with your content, which will replace the variables in the HTML page.
	* Select which language the content will be written in and allow visitors to change their desired language 
	* Use Markdown syntax in text fields that will later be rendered as HTML
* Create new files & folders on the server
* Include html files inside other html files using the following syntax `{{header "Partials/UI/header.html"}}` (`header` is a custom variable name and the file path is in quotes)
* Shortcut Keys: Ctrl + S (save), escape (toggle editor / preview), F1 (Monaco editor command window)
* Choose between [Monaco](https://microsoft.github.io/monaco-editor/) & [Ace](https://ace.c9.io/) as your preferred code editor
    * Minimap next to scrollbar (Monaco only)
    * Syntax Highlighting for HTML, CSS, LESS, & Javascript
    * Code Folding


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

![Saber IDE](http://www.markentingh.com/projects/saber/saber-content-fields.jpg)

Generate form fields by writing HTML variables (e.g. `<h2>{{hero-title}}</h2>`) and use them to fill out content for your web pages



## Under The Hood
Saber uses many technologies developed by [Mark Entingh](http://www.github.com/markentingh), including [Datasilk Core](http://www.github.com/datasilk/core) as an MVC framework for ASP.NET Core, [Tapestry](http://www.github.com/websilk/tapestry) for frontend CSS UI design, [Datasilk Core JS](http://www.github.com/datasilk/corejs) as a frontend JavaScript framework, and [Selector](http://www.github.com/websilk/selector) as a replacement for jQuery at only 5kb in size.

## Future Development
* Upload & manage photos & files related to a specific web page
* Publish content using other formats besides HTML & CSS, such as uploading a PDF, DOC, DOCX, MP3 (for auto-transcribing), or Excel spreadsheet.
* Manage list of acceptable meta title & description prefixes and suffixes.
    * Select which prefix or suffix to add to individual web page titles & descriptions via the page settings
* Create multiple headers & footers for your website and select which ones to use within each individual page's settings
* Upload a cover photo to use when sharing with Facebook, Twitter, and other social platforms
* Other optional meta data fields used within the page settings
  * Fields from schema.org (JSON-LD)
  * product related fields (price, saleprice, colors, specs)
* Create template files (HTML, LESS, JS) for a specific page (e.g. `/blog`) and they will be used to generate initial files for new sub-pages (e.g. `/blog/2018/03/17/A-New-Day`)