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

    ```git clone --recurse-submodules http://github.com/datasilk/saber YourProjectName```

	> NOTE: replace `YourProjectName` with the name of your project

2. Replace all case-sensitive instances of `Saber` to `YourProjectName` and `saber` to `yourprojectname` in all files within the repository
3. Rename file `Saber.sln` to `YourProjectName.sln` and file `App/Saber.csproj` to `App/YourProjectName.csproj`
2. Run command ```npm install```
3. Run command ```gulp default```
4. In Visual Studio, publish the SQL project to SQL Server 2016 (or greater), with your own database name
5. Open `config.json` and make sure the database connection string for property `SqlServerTrusted` points to your database.
6. Click Play in Visual Studio 2017


## Features
* Build web pages from within your web browser using a built-in IDE ([Monaco](https://microsoft.github.io/monaco-editor/)) for editing HTML, CSS, LESS, & Javascript
* Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML & CSS within the built-in IDE
* Multi-lingual Page Content
	* Generate text fields by adding variables to your HTML page (e.g. `<div>{{article-title}}</div>`)
	* Fill out the text fields with your content, which will replace the variables on the HTML page.
	* Select which language the content will be written in and allow visitors to change their desired language 
	* Use Markdown syntax in text fields that will be rendered as HTML
* Browse server-side resources (HTML, CSS, LESS, & JS files) and open them in the code editor
* 


## Future Development
* Publish content using other formats besides HTML & CSS, such as writing markdown or uploading a PDF file.