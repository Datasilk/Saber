# Saber Release
#### A comprehensive guide to creating a release for Saber

### Release for Windows
1. Remove all Vendor plugins located in the `App/Vendors` folder
2. Build Sql Project using the *Release* configuration
3. Run `runtests.bat`
4. If all Cypress tests pass after executing `runtests.bat`, compress the contents of `/App/bin/Release/Saber`
into a zip file named `Saber-{version}.zip`, where `{version}` is replaced with the current version of Saber.
5. Create a new release in GitHub titled *Saber {version}* and include a list of features & bug fixes in the description.