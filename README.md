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
* Build web pages from within the web browser using a built-in IDE for editing HTML, CSS, LESS, & Javascript
* Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML & CSS