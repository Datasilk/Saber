# Vendors
Install or build Saber-specific vendor plugins within this folder. 

* Copy a Saber plugin into this folder
* Create a new folder for your own plugin

> Installed vendor plugins can be found by navigating to **File** > **App Settings** within the Saber Editor. Some vendor plugins may not be visible from the App Settings tab and would be considered "pass-through", meaning that the plugin "just works". 

> Vendor plugins cannot be disabled through the Saber Editor and so you must physically remove the vendor files from the project to disable their functionality.

## Currently supported plugins

#### CORS
* [Github repository](https://github.com/Datasilk/Saber-CORS)

Adds CORS-related headers to the controller or service response for trusted cross-origin domains. 

#### Import Export
* [Github repository](https://github.com/Datasilk/Saber-ImportExport)

A vendor plugin for Saber that allows webmasters to backup & restore all web content for their Saber website using a simple zip file. This is useful for creating nightly backups and can also be used to publish pending changes from your local workstation to your live website.

#### Page List
* [Github repository](https://github.com/Datasilk/Saber-PageList)

Display a list of webpages associated with your website, such as blog posts or wiki pages. 

#### Reset Cache
* [Github repository](https://github.com/Datasilk/Saber-ResetCache)

A vendor plugin for Saber that allows webmasters to manually reset the stored cache of objects across all networked servers and executes gulp tasks to copy modified resources to **wwwroot**. This could be useful if your website isn't loading correctly.

#### Replace Template
* [Github repository](https://github.com/Datasilk/Saber-ReplaceTemplate)

A vendor plugin for Saber that allows webmasters to replace the template website that is included with Saber with the currently published website. This was meant to be an internal tool used by Saber developers to update the official template website that is loaded when the user first runs a new copy of Saber in Visual Studio.

## Vendor-Specific Functionality

#### IVendorStartup
Interface used to execute vendor-specific code when the Saber application starts up. All Vendor classes that inherit `IVendorStartup` will be evaluated via
Saber's `ConfigureServices` method and `Configure` method located in the `/App/Startup.cs` class.

#### IVendorViewRenderer
Interface used to execute vendor-specific code when Saber renders a View. Attribute `[ViewPath("/Views/Path/To/myfile.html")]` is required on the class that inherits `IVendorViewRenderer`, which will determine when the `Render` method is being called to load the associated `html` file. Use this interface to add HTML to a View that contains the `{{vendor}}` element.

``` csharp
namespace Saber.Vendor.MyPlugin
{
    [ViewPath("/Views/AppSettings/appsettings.html")]
    public class MyPlugin : IVendorViewRenderer
    {
        public void Render(Core.IRequest request, View view)
        {
            var myview = new View("/Vendor/MyPlugin/settings.html");
            view["vendor"] += myview.Render();
        }
    }
}

```
In the example above, we append the rendered HTML of our `settings.html` view to the `vendor` element whenever Saber renders the `/Views/AppSettings/appsettings.html` View.
> **NOTE:** It is important that you append the rendered HTML to the contents of the `vendor` element instead of replacing the contents because other vendors might have appended content to the same element beforehand.

Saber supports the `IVendorViewRenderer` for all views within the application, and the following views include a `{{vendor}}` HTML variable so that vendors can extend the Editor UI.

* `/Views/AppSettings/appsettings.html`, used to add vendor-speicific Application Settings to Saber
* `/Views/PageSettings/pagesettings.html`, used to add vendor-speicific Page Settings to Saber

#### IVendorController
Interface used to route page requests to vendor-specific controllers. Your class must inherit `Controller` as well as `IVendorController` in order to work properly.
> **NOTE:** Make sure your controller names do not conflict with potential web pages that users will want to create for their website, such as:
>  `About`, `Contact`, `Blog`, `Wiki`, `Projects`, `Team`, `Terms`, `PrivacyPolicy`, `Members`, `Landing`, `Store`, `History`, etc.