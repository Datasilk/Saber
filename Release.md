# Saber Release
#### A comprehensive guide to creating a release for Saber

### Kestrel Release (Windows & Linux)
1. Make sure `Program.cs` is using Kestrel instead of IIS Integration
2. Build Saber project using the *Release* configuration ([more info](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build]))
	* `dotnet build App/Saber.csproj --configuration Release`
3. Build Sql Project using the *Release* configuration (**optional**)
4. Run `gulp publish`, or run 'gulp publish-nosql' if you skipped step 3 and did not build your Sql project.

