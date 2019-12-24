# Vendors
Install or build Saber-specific vendor plugins within this folder. 

* Copy a Saber plugin into this folder
* Create a new folder for your own plugin

## Currently supported plugins

#### CORS
Authorize Web API calls from cross-origin domains. 
* [Github repository](https://github.com/Datasilk/Saber-CORS)

#### Import Export
A vendor plugin for Saber that allows webmasters to backup & restore web content for their Saber website using a simple zip file. 
* [Github repository](https://github.com/Datasilk/Saber-ImportExport)


#### Page List
Display a list of webpages associated with your website, such as blog posts or wiki pages. 
* [Github repository](https://github.com/Datasilk/Saber-PageList)

## Vendor-Specific Functionality

#### IVendorStartup
Interface used to execute vendor-specific code when the Saber application starts up. All Vendor classes that inherit `IVendorStartup` will be evaluated via
Saber's `ConfigureServices` method and `Configure` method located in the `/App/Saber.cs` class.

#### IVendorViewRenderer
Interface used to execute vendor-specific code when Saber renders a View. Attribute `[ViewPath("/Views/Path/To/myfile.html")]` is required on the class that inherits `IVendorViewRenderer`, which will determine when the `Render` method is called based on which view is being rendered. Use this interface to add HTML to a View that contains the `{{vendor}}` element.

```
namespace Saber.Vendor.MyPlugin
{
    [ViewPath("/Views/AppSettings/appsettings.html")]
    public class MyPlugin : IVendorViewRenderer
    {
        public void Render(Request request, View view)
        {
            var myview = new View("/Vendor/MyPlugin/settings.html");
            view["vendor"] += myview.Render();
        }
    }
}

```
In the example above, we append the rendered HTML of our `settings.html` view to the `vendor` element whenever Saber renders the `/Views/AppSettings/appsettings.html` View.
> **NOTE:** It is important that you append the rendered HTML to the contents of the `vendor` element instead of replacing the contents because other vendors might have appended content to the same element beforehand.

Saber supports the `IVendorViewRenderer` for specific views so that vendors can extend the Editor UI.

* `/Views/AppSettings/appsettings.html`, used to add vendor-speicific Application Settings to Saber

#### IVendorController
Interface used to route page requests to vendor-specific controllers. The class must inherit `Controller` as well as `IVendorController` in order to work. 
> **NOTE:** Make sure your controller names do not conflict with potential web pages that users will want to create for their website, such as `About`, `Blog`, `Wiki`, `Projects`, `Team`, `PrivacyPolicy`, `Members`, `Landing`, `Store`, `History`, etc.