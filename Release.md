# Saber Release
#### A comprehensive guide to creating a release for Saber

1. Make sure `Program.cs` is using Kestrel instead of IIS Integration
2. Build Saber project using the *Release* configuration
3. Build Sql Project
4. Open `Sql/bin/Release/Saber_Create.sql` and replace the following lines

	``` sql
	:setvar DatabaseName "Sql"
	:setvar DefaultFilePrefix "Sql"
	```
	with

	``` sql
	:setvar DatabaseName "Saber"
	:setvar DefaultFilePrefix "Saber"
	```
5. Copy `Sql/bin/Release/Saber_Create.sql` to `App/bin/Release/netcoreapp3.1/Sql/`
6. Create file `App/bin/Release/netcoreapp3.1/config.json` with the following contents:
	``` json
	{
	  "sql": {
		"active": "SqlServerTrusted",
		"SqlServerTrusted": "server=.\\SQL2017; database=Saber; Trusted_Connection=true"
	  },
	  "encryption": {
		"salt": "?",
		"bcrypt_work_factor": "10"
	  }
	}
	```
7. Copy `App/wwwroot/` to `App/bin/Release/netcoreapp3.1/`
8. Delete all files from the `App/bin/Release/netcoreapp3.1/Vendors` folder
9. Delete the following folders
   * `App/bin/Release/netcoreapp3.1/wwwroot/{all folders except editor folder}`
   * `App/bin/Release/netcoreapp3.1/wwwroot/editor/js/vendors/`
   * `App/bin/Release/netcoreapp3.1/wwwroot/editor/css/vendors/`
   * `App/bin/Release/netcoreapp3.1/Content/backups/`
   * `App/bin/Release/netcoreapp3.1/Content/pages/`
   * `App/bin/Release/netcoreapp3.1/Content/partials/`

