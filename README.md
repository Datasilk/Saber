![Saber Logo](http://www.markentingh.com/projects/saber/sword-logo.png)

# Saber

A simple, straight forward CMS and website builder. 

Saber gives software engineers the ability to focus on traditional web development by writing HTML, CSS (LESS), and Javascript 
in their respective file formats from an integrated development environment (IDE) within their Saber website.

Saber is lightweight, blazing fast, with a "zero footprint" policy, and has a very robust plugin system. Saber is meant for 
enterprise-level web application development.

## Requirements

* Visual Studio 2022
* ASP.NET 6.0
* SQL Server 2017 (or greater)
* SQL Server Management Studio (SSMS)
* Node.js
* Gulp

## Installation (from source code)

1. Clone the repository and get all submodules:

    ```
    git clone http://github.com/Datasilk/Saber MySaberProject --recursive
    ```
    * NOTE: If you plan on making changes to Saber, you must make sure all submodules are not in a detached HEAD state
    ```
    git submodule foreach git checkout master
    git submodule foreach git submodule foreach git checkout master
    ```

2. Run command `npm install`
3. Run command `gulp default`
4. In Visual Studio, build then publish the **Sql** project to SQL Server
5. If you are running **MS SQL Server** on the same PC, you can set up your database to use a trusted connection.
    1. In **SSMS**, under **Security > Logins**, create a new user for **NT AUTHORITY\NETWORK SERVICE**, and within the user properties window, select **User Mappings**, then check the Saber database, and check the following database membership roles for the Saber database: `db_datareader`, `db_datawriter`, `db_owner`
6. Copy `/App/Content/temp/config.json` to `/App/config.json`, then open the file and update the Sql connection string
7. Click Play in Visual Studio & navigate to https://localhost:7070

## Installation (from release)

1. Get the latest release of Saber at https://www.github.com/Datasilk/Saber/releases
2. Extract the release zip file
3. Create/Update your MSSQL Database
    * To **Create a new database**, execute the file `Sql/Saber_Create.sql` using **SSMS**. You may want to open the file first and change the following lines to your own database name:
        ``` sql
        :setvar DatabaseName "Saber"
        :setvar DefaultFilePrefix "Saber"
        ```
    * To **Update an existing database**, use the file `Sql/Saber.dacpac`. In **Microsoft SQL Server Management Studio**, right-click your existing Saber database and select **Tasks > Upgrade Data-tier Application** and follow the upgrade wizard.
4. Choose which platform you wish use Saber on below (Docker, IIS, Windows, or Linux)

#### Docker Support
Saber also supports Docker. In order for Saber to work with Docker in Windows, you must first install and run [Docker Desktop](https://docs.docker.com/docker-for-windows/). 
* Running Docker from Visual Studio:
    1. Copy `/App/Content/temp/config.docker.json` to `/App` folder and open the file to update the Sql connection string Initial Catalog along with the User ID & Password you've created in Sql Server that has access to your Saber database
    2. Click **Play** in Visual Studio after selecting the **Docker** launch command from the drop down
* Running Docker from Release:
    1. execute `docker compose up` to run Saber after extracting the contents of the release zip file

#### IIS Support
Saber now supports IIS natively (in-process & out-of-process)

1. Install the [.NET Core Hosting Bundle](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/) to allow IIS to support .NET Core applications
2. Create a new website in Internet Information Services (IIS)
    * Under **Sites**, right-click and select **Add Website..**
    * Under **Application Pools**, right-click the application pool associated with your new website and select **Basic Settings...**, then change **.NET CLR version** to **No Managed Code**
    * Under **Application Pools**, right-click the application pool associated with your new website and select **Advanced Settings...**, then change **Identity** to **NetworkService**
3. Copy all files & folders from the `App` folder to your IIS website folder for Saber.
4. Open `web.config` in your IIS website folder and change `hostingModel="OutOfProcess"` to `hostingModel="InProcess"`
5. Copy `/Content/temp/config.prod.json` to `/config.prod.json`.
6. Open `config.prod.json` and modify the database connection string
7. If you are running **MS SQL Server** on the same PC as IIS, you can set up your database to use a trusted connection.
    *. In **SSMS**, under **Security > Logins**, create a new user for **NT AUTHORITY\NETWORK SERVICE**, and within the user properties window, select **User Mappings**, then check the Saber database, and check the following database membership roles for the Saber database: `db_datareader`, `db_datawriter`, `db_owner`
    *. Open `config.prod.json` and modify the database connection string, remove `User Id` and `Password` fields, then add `Trusted_Connection=true;`
8. Open a web browser and navigate to your new website. 

    > NOTE: If you receive an In-Process Start Failure, you may need to open `config.prod.json` and change your database connection string to point to the correct database. Make sure to restart your website in IIS after making changes to your config file.


#### Windows Support
Saber can be run from PowerShell with the following command `./Saber.exe` from the release `App` folder

#### Linux Support
Saber can be run with the following command `./saber` from the release `App` folder

----

![Saber IDE](http://www.markentingh.com/projects/saber/saber-html-file-2.jpg)
*Screenshot of Saber's Editor UI*

## Features
Build web pages from within your web browser using a built-in IDE for editing HTML, CSS, LESS, & JavaScript files.

#### Navigate to any URL 
Convert any URL within your website to a valid web page simply by navigating to the URL and writing some HTML, CSS, & JavaScript within the built-in IDE. The editor initially opens 3 files (HTML, LESS, & JS) that are associated with the web page being viewed.

#### Create & Modify Website Resources
Use the **file browser** within the built-in IDE to open website resources (HTML, CSS, LESS, & JS files) in new tabs. Use the *View* drop down menu to open the file browser and the **File** drop down menu to create new files & folders. The initial folder structure is described below: 

* **wwwroot** is a public-facing folder where you can upload files & images to utilize within your website
* **pages** is a protected folder used to store all pages belonging to your website. Clicking on an HTML page in the file browser will navigate to that page within your website for editing.
* **partials** is a protected folder used to store HTML files that can be included within web pages throughout your website (such as *header.html* & *footer.html* files)
* **website.less** is compiled into a CSS file and loaded on every page within your website

#### Upload Files & Photos
You can upload image files & other resources for a specific web page or within the **wwwroot** folder to be used globally. Manage your uploads within an image-gallery style file browser.

#### Multi-lingual Page Content
* Generate a form that contains text fields by adding *mustache* variables to your HTML page (e.g. `<div>{{article-title}}</div>`)
* Fill out the text fields with your content, which will replace the variables in the HTML page. (e.g. `Insert Article Title Here`)
* Select which language the content will be written in and allow visitors to change their desired language 
* Write content using Markdown syntax  that will later be rendered as HTML

#### Use Partial Views
Create HTML files as Partial Views within your website's `/partials` folder and then use *mustache* variables to 
inject your partial views into web pages to render robust, dynamic content across your website.
	
For example:
	
```
{{sidebar "partials/UI/sidebar.html"}}
```
`sidebar` is the mustache variable name and the relative file path is located within quotes.
> Note: The relative path to all files are case-sensitive 

#### Zero Footprint
When users visit your website, Saber will only render content that you've developed. Your users will not have to
download any resources related to Saber's platform or IDE and so your website will truly feel like it was hand-crafted.

#### List Component
Create dynamic lists of content for your pages by combining a **Partial View** and a **Data Source**. For example, 
you could use the *Web Pages* data source to display a filtered list of web pages on your blog sidebar. 
You could also create a custom database of content using the [DataSets](https://www.github.com/Datasilk/Saber-DataSets) 
plugin and utilize your datasets as a data source for the List component. You could also just manually create new items 
for your list instead of using a data source.

#### Template Web Pages
You can create a template web page (e.g. `https://yoursite.com/support/template`) and design the template page to be 
used as a starting point when creating new sub-pages. When navigating to a new URL within your website 
(e.g. `https://yoursite.com/support/my-new-page`), Saber will copy & load the design & content from the associated 
template web page when the URL is accessed for the first time. Then, you can customize the content of your new page quickly
and efficiently. This is useful when managing complex websites such as a wiki, blog, or storefront.

#### Include Vendor Plugins
Saber supports 3rd-party plugins, including a few plugins developed [in-house](https://github.com/orgs/Datasilk/repositories?q=saber). 
You can find a list of plugins @ [saber.datasilk.io](https://saber.datasilk.io/plugins.html).

##### Develop Vendor Plugins
You can learn how to develop Vendor plugins and find a list of supported Vendor plugins and links to their repositories 
online at [saber.datasilk.io](https://saber.datasilk.io/developers.html) and [/Vendor/README.md](Vendor/README.md).

##### Installing a Vendor Plugin
All vendor plugins **must** be installed within the `/App/Vendors` folder. For example:

```git clone https://github.com/Datasilk/Saber-DataSets App/Vendors/DataSets```

#### Code Editor
Saber uses [Monaco](https://microsoft.github.io/monaco-editor/) as its code editor

* Minimap next to scrollbar
* Intellisense for HTML, CSS, LESS, & Javascript
* Syntax Highlighting for HTML, CSS, LESS, Javascript, & `{{mustache}}`
* Code Folding
* Commands (press F1)

#### User Security
Saber includes a robust security system so administrators can give users permissions to specific features & web pages.
Create **Security Groups** to manage permissions to features within Saber, then assign users to your Security Groups. 
Assign one or more Security Groups to a web page to make the page secure. All secure pages that are accessed by users 
without proper permissions will be redirected to the `/access-denied` page.

#### Email Clients & Actions
Configure Saber to use an email client when emails need to be sent out to your users.
Saber comes with an SMTP email client manager but you can always utilize a 3rd-party Vendor plugin (or develop your own plugin)
that can send emails another way. For example, you could install the [SendGrid](https://www.github.com/Datasilk/Saber-SendGrid) plugin
if your website uses [SendGrid](https://www.sendgrid.com) as a way to send emails to users.

Saber will send emails to users based on various actions, such as new account signups or when the user requests to reset their password.

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-html-closeup-2.jpg)

Edit page resources for any URL within your domain name

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-file-browser-2.jpg)

Browse server-side files and edit them via the IDE within Saber

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-scrollbar-2.jpg)

Use a minimap to scroll through your code faster than ever

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-photo-gallery-2.jpg)

Upload images & other resources

---

![Saber IDE](http://www.markentingh.com/projects/saber/saber-content-fields-2.jpg)

Generate form fields by writing HTML variables (e.g. `<h2>{{hero-title}}</h2>`) and use them to fill out content for your web pages

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

### Even More Features!
* Manage a list of acceptable meta title prefixes and suffixes to make sure your website has consistentpage titles
    * Select which prefix or suffix to add to individual web page titles & descriptions via the page settings
* Manage a list of website icons to use viewed in a web browser, on Android, and on iOS respectively.
* Create multiple headers & footers for your website and select which ones to use within each individual page's settings
* Include custom Javascript & Stylesheet files on specific pages within your website or on every page within your website

## Under The Hood
Saber uses a few technologies developed by its creator, [Mark Entingh](http://www.github.com/markentingh), including: 

* [Datasilk Core MVC](http://www.github.com/datasilk/core) as the MVC middleware for ASP.NET Core
* [Saber Core](http://www.github.com/datasilk/saber-core) as a business logic layer for the Vendor plugin systen
* [Tapestry](http://www.github.com/datasilk/tapestry) for frontend CSS UI design
* [Datasilk Core JS](http://www.github.com/datasilk/corejs) as a frontend JavaScript framework
* [Selector](http://www.github.com/datasilk/selector) as a replacement for jQuery at only 5kb in size

## Future Development
* Upload a cover photo to use when sharing with Facebook, Twitter, and other social platform
* Page-level meta data management
  * Fields from schema.org (JSON-LD)
  * product related fields (price, saleprice, colors, specs)
