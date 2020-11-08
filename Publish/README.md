# Saber
#### Installation Instructions for Windows & Linux

## Windows (IIS)
1. Create/Update your MSSQL Database
    * To **Create a new database**, execute the file `Sql/Saber_Create.sql` using **Microsoft SQL Server Management Studio**. You may want to open the file first and change the following lines to your own database name:
    ``` sql
    :setvar DatabaseName "Saber"
    :setvar DefaultFilePrefix "Saber"
    ```
    * Under **Security > Logins**, create a new user for **NT AUTHORITY\NETWORK SERVICE**, and within the user properties window, select **User Mappings**, then check the Saber database, and check the following database membership roles for the Saber database: `db_datareader`, `db_datawriter`, `db_owner`

* To **Update an existing database**, use the file `Sql/Saber.dacpac`. In **Microsoft SQL Server Management Studio**, right-click your existing Saber database and select **Tasks > Upgrade Data-tier Application** and follow the upgrade wizard.
2. Install the [.NET Core Hosting Bundle](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/?view=aspnetcore-3.1#install-the-net-core-hosting-bundle)
3. Create a new website in Internet Information Services (IIS)
    * Under **Sites**, right-click and select **Add Website..**
    * Under **Application Pools**, right-click the application pool associated with your new website and select **Basic Settings...**, then change **.NET CLR version** to **No Managed Code**
    * Under **Application Pools**, right-click the application pool associated with your new website and select **Advanced Settings...**, then change **Identity** to **NetworkService**
4. Copy all files & folders from the `App` folder to your IIS website folder for Saber.
5. Open `web.config` in your IIS website folder and change `hostingModel="OutOfProcess"` to `hostingModel="InProcess"`
6. Open a web browser and navigate to your new website. If this is your first time running Saber, a `config.prod.json` file will be generated in your IIS website folder.
    > NOTE: If you receive an In-Process Start Failure, you may need to open `config.prod.json` and change your database connection string to point to the correct database. Make sure to restart your website in IIS after making changes to your config file.

## Linux (Nginx)
Instructions coming soon. You Linux users could probably figure it out without instructions anyway lol.