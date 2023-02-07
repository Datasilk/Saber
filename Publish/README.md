# Saber

A simple, straight forward CMS and website builder. 

Saber gives software engineers the ability to focus on traditional web development by writing HTML, CSS (LESS), and Javascript 
in their respective file formats from an integrated development environment (IDE) within their Saber website.

Saber is lightweight, blazing fast, with a "zero footprint" policy, and has a very robust plugin system. Saber is meant for 
enterprise-level web application development.

## Requirements

* ASP.NET 6.0
* SQL Server 2017 (or greater)
* SQL Server Management Studio (SSMS)

## Installation

1. Get the latest release of Saber at https://www.github.com/Datasilk/Saber/releases
2. Extract the release **7z** zip file
3. Create or Update your MSSQL Database
    * To **Create a new database**, execute the file `Sql/Saber_Create.sql` using **SSMS**. You may want to open the file first and change the following lines to your own database name:
        ``` sql
        :setvar DatabaseName "Saber"
        :setvar DefaultFilePrefix "Saber"
        ```
    * To **Update an existing database**, use the file `Sql/Saber.dacpac`. In **Microsoft SQL Server Management Studio**, right-click your existing Saber database and select **Tasks > Upgrade Data-tier Application** and follow the upgrade wizard.
4. Choose which platform you wish use Saber on below (Docker, IIS, Windows, or Linux)

#### Docker Support
Saber also supports Docker. In order for Saber to work with Docker in Windows, you must first install and run [Docker Desktop](https://docs.docker.com/docker-for-windows/). 
1. Copy `/App/Content/temp/config.docker.json` to `/App` folder and open the file to update the Sql connection string Initial Catalog along with the User ID & Password you've created in Sql Server that has access to your Saber database
2. Click **Play** in Visual Studio after selecting the **Docker** launch command from the drop down

#### IIS Support
Saber now supports IIS natively (in-process & out-of-process)

1. Install the [.NET Core Hosting Bundle](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)
2. Create a new website in Internet Information Services (IIS)
    * Under **Sites**, right-click and select **Add Website..**
    * Under **Application Pools**, right-click the application pool associated with your new website and select **Basic Settings...**, then change **.NET CLR version** to **No Managed Code**
    * Under **Application Pools**, right-click the application pool associated with your new website and select **Advanced Settings...**, then change **Identity** to **NetworkService**
3. Copy all files & folders from the `App` folder to your IIS website folder for Saber.
4. Open `web.config` in your IIS website folder and change `hostingModel="OutOfProcess"` to `hostingModel="InProcess"`
5. Open `config.prod.json` and modify the database connection string
6. If you are running **MS SQL Server** on the same PC as IIS, you can set up your database to use a trusted connection.
    *. In **SSMS**, under **Security > Logins**, create a new user for **NT AUTHORITY\NETWORK SERVICE**, and within the user properties window, select **User Mappings**, then check the Saber database, and check the following database membership roles for the Saber database: `db_datareader`, `db_datawriter`, `db_owner`
    *. Open `config.prod.json` and modify the database connection string, remove `User Id` and `Password` fields, then add `Trusted_Connection=true;`
7. Open a web browser and navigate to your new website. 

    > NOTE: If you receive an In-Process Start Failure, you may need to open `config.prod.json` and change your database connection string to point to the correct database. Make sure to restart your website in IIS after making changes to your config file.


#### Windows Support
Saber can be run from PowerShell with the following command `./Saber.exe`

#### Linux Support
Saber can be run with the following command `./saber`
You can also run Saber within Nginx by following instructions on 
[learn.microsoft.com](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx)