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
* Build web pages from within your web browser using a built-in IDE for editing HTML, CSS, LESS, & Javascript
* Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML & CSS within the built-in IDE

## Future Development
* Publish content using other formats besides HTML & CSS, such as writing markdown or uploading a PDF file.
* ##### Simple Content Editor
  Add content to your web pages by filling out text fields within a generic form instead of writing content directly within the raw HTML code.
  This works by adding variables inside the HTML code (e.g. `{{article-title}}`), which will generate form fields within the editor
  that can easily be updated by content writers who may not be well versed in writing HTML.
	* Generated forms will have multi-lingual support, allowing writers to select a language before filling out form fields. 
	* Generated text fields will support markdown & HTML.
	* Allow user to select a supported language by executing a simple JavaScript method from an anchor link: `S.language.change('spanish');`
	* Add all country flags to images folder so web developers can easily generate a language selection menu for their website