# Saber
A simple, straight-forward CMS & website builder

[https://saber.datasilk.io](https://saber.datasilk.io)

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
Please follow the instruction to [Host & deploy](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0) an ASP.NET Core 5.0 application on Linux.

## Publish
Navigate to the Saber project folder and complete the following tasks to create a new **bundle** for your GitHub sponsors to download and install onto their web server, micro-service, and/or workstation.
1. Move `\App\Vendors` folder outside of `\App` folder to make sure no vendor plugins are compiled with Saber
2. Comment out lines of code in `Program.cs` based on which platform you are publishing to  (*IIS, Linux, or Docker*)
3. Publish Saber from within Visual Studio 2019
4. Open folder `\App\bin\Publish\Saber`
   1. Remove `wwwroot\editor\vendors` folder
   2. Remove `wwwroot\editor\js\vendors-editor.js`
   3. Remove all sub-folders inside `\wwwroot` except for `\wwwroot\editor`
   4. Remove all sub-folders inside `\Content` except for `\Content\temp`
   5. Copy `\Content\temp\config.prod.json` to `\App\bin\Publish\Saber` if publishing for **IIS** or **Linux**, or copy `\Content\temp\config.docker.prod.json` instead if you are publishing for **Docker**
5. Open `web.config`
   1. For IIS, use `hostingModel="inprocess"`
   2. For Linux or Docker, use `hostingModel="OutOfProcess"`
    > Your web.config should look similar to the XML document below
    ```xml 
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
      <location path="." inheritInChildApplications="false">
        <system.webServer>
          <handlers>
            <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
          </handlers>
          <aspNetCore processPath="dotnet" arguments=".\Saber.dll" stdoutLogEnabled="false" stdoutLogFile=".\logs\stdout" hostingModel="OutOfProcess">
		    <environmentVariables>
              <environmentVariable name="ASPNETCORE_HTTPS_PORT" value="7070" />
              <environmentVariable name="COMPLUS_ForceENC" value="1" />
              <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
            </environmentVariables>
            <handlerSettings>
              <handlerSetting name="debugFile" value=".\logs\aspnetcore-debug.log" />
              <handlerSetting name="debugLevel" value="ERROR, WARNING, TRACE, CONSOLE, FILE" />
            </handlerSettings>
	      </aspNetCore>
        </system.webServer>
      </location>
    </configuration>
    ```
6. Copy contents of `\App\bin\Publish\Saber` to your **bundle** folder in a folder based on your `Program.cs` changes
    1. for IIS, copy contents to `Saber` folder
    2. for Linux, copy contents to `Saber-Kestrel` folder
    3. for Docker, copy contents to `Saber-Docker` folder
7. Repeat steps 2 through 6 for each platform (IIS, Linux, & Docker)
8. Publish Vendor plugins by executing command `.\publish.bat` from within each plugin folder, then copy the contents of `\Publish\win-x64` to your **bundle** folder inside the `\Vendors` folder
9. Build the `Sql` project within Visual Studio, then copy `\Sql\bin\Debug\Sql_Create.sql` and `\Sql\bin\Debug\Sql.dacpac` to the `\Sql` folder located within your **bundle** folder
9. Include `README.md` in the root of your **bundle** folder
10. Include `Dockerfile` used to build a Docker Image in the root of your bundle folder
    > Dockerfile should contain the text below

    ```
    # syntax=docker/dockerfile:1
    FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
    WORKDIR /app

    EXPOSE 80
    EXPOSE 443

    COPY Saber-Docker .
    COPY Vendors Vendors
    ENTRYPOINT ["dotnet", "Saber.dll"]
    ```
11. Compress the contents of your **bundle** folder into a zip file. The folder structure should look like the following:
    ```
    /Saber
    /Saber-Kestrel
    /Saber-Docker
    /Vendors
    /Sql
    Dockerfile
    README.md
    ```
12. Distribute your zip file to all of your sponsors, friends, and devs who want to use Saber as a platform to build their web projects