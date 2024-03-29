# Saber Release
#### A comprehensive guide to creating a release for Saber

### Release for Windows, Linux, & Docker
1. Remove all Vendor plugins located in the `App/Vendors` folder
2. Build Sql Project using the **Release** configuration
3. If you don't already have a local database named **Saber-Test**, create one by publishing the *Sql* project
	* Using Sql Server Management Studio (SSMS), under **Security > Logins**, create a new user for **docker** (matching credentials found in `App/Content/temp/config.docker.json` **TrustedConnection** string)
	* Update the `App/Content/temp/config.docker.json` **TrustedConnection** connection string so Saber can connect to your local Sql Server
	* In SSMS, select **User Mappings**, then check the **Saber-Test** database, and check the following database membership roles for the Saber-Test database: `db_datareader`, `db_datawriter`, `db_owner`
4. Run `runtests.bat`
5. If all Cypress tests pass after executing `runtests.bat`, compress the contents of `/App/bin/Release/Saber`
into a zip file named `Saber-{version}.zip`, where `{version}` is replaced with the current version of Saber.
6. Create a new release in GitHub titled "*Saber {version}*" and include a list of features & bug fixes in the description along with the zip file you created.
7. Tell the world that you released a new version of Saber!

