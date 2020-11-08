# Saber Release
#### A comprehensive guide to creating a release for Saber

### Kestrel Release (Windows & Linux)
1. Make sure `Program.cs` is using Kestrel instead of IIS Integration
2. Build Saber project using the *Release* configuration
3. Build Sql Project using the *Release* configuration
4. Run `gulp release`

