# Saber Release
#### A comprehensive guide to creating a release for Saber

### Release for Windows
1. Remove all Vendor plugins located in the `App/Vendors` folder
1. Build Saber project using the *Release* configuration ([more info](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build]))
	* `dotnet build App/Saber.csproj --configuration Release`
2. Build Sql Project using the *Release* configuration (**optional**)
3. Run `gulp publish`, or run 'gulp publish-nosql' if you did not build your Sql project.

This will publish a version of Saber in `/App/bin/Release/Saber` that does not contain any files related to the 
website you currently have installed and it also ignores any Vendor plugin files that you may have installed as well. 