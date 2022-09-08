# Saber Release
#### A comprehensive guide to creating a release for Saber

### Release for Windows
1. Build Saber project using the *Release* configuration ([more info](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build]))
	* `dotnet build App/Saber.csproj --configuration Release`
2. Build Sql Project using the *Release* configuration (**optional**)
3. Run `gulp publish`, or run 'gulp publish-nosql' if you did not build your Sql project.

