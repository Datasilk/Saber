using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Utility.Strings;
using Saber.Common.Platform;

public partial class Startup : Datasilk.Startup {

    //vendor-specific startup methods
    partial void ConfigureVendorServices(IServiceCollection services);
    partial void ConfigureVendors(IApplicationBuilder app);

    public override void ConfiguringServices(IServiceCollection services)
    {
        base.ConfiguringServices(services);
        ConfigureVendorServices(services);
    }


    public override void Configured(IApplicationBuilder app, IWebHostEnvironment env, IConfigurationRoot config)
    {
        base.Configured(app, env, config);

        //set up database connection
        Query.Sql.connectionString = Server.sqlConnectionString;
        var resetPass = Query.Users.HasPasswords();
        Server.hasAdmin = Query.Users.HasAdmin();

        //set up Saber language support
        Server.languages = new Dictionary<string, string>();
        Server.languages.Add("en", "English"); //english should be the default language
        Query.Languages.GetList().ForEach((lang) => {
            Server.languages.Add(lang.langId, lang.language);
        });

        //check if default website exists
        if (!File.Exists(Server.MapPath("/Content/pages/home.html")))
        {
            //copy default website since none exists yet
            Directory.CreateDirectory(Server.MapPath("/wwwroot/content/"));
            Directory.CreateDirectory(Server.MapPath("/wwwroot/content/pages/"));
            Directory.CreateDirectory(Server.MapPath("/wwwroot/images/"));
            Directory.CreateDirectory(Server.MapPath("/wwwroot/js/"));
            Directory.CreateDirectory(Server.MapPath("/Content/pages/"));
            Directory.CreateDirectory(Server.MapPath("/Content/partials/"));
            Saber.Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/images/"), Server.MapPath("/wwwroot/images/"));
            Saber.Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/scripts/"), Server.MapPath("/wwwroot/js/"));
            Saber.Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/scripts/"), Server.MapPath("/Scripts/"));
            Saber.Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/partials/"), Server.MapPath("/Content/partials/"));
            Saber.Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/pages/"), Server.MapPath("/Content/pages/"));
            File.Copy(Server.MapPath("/Content/temp/css/website.less"), Server.MapPath("/CSS/website.less"), true);

            Thread.Sleep(1000);

            //run default gulp command to copy new website resources to wwwroot folder
            var p = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = "/c gulp default:website",
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    WorkingDirectory = Server.MapPath("/").Replace("App\\", ""),
                    Verb = "runas"
                }
            };
            p.OutputDataReceived += Saber.Common.ProcessInfo.Gulp.OutputReceived;
            p.ErrorDataReceived += Saber.Common.ProcessInfo.Gulp.ErrorReceived;
            p.Start();
            p.WaitForExit();
            Thread.Sleep(1000);
        }

        //initialize platform-specific html variables for scaffolding
        ScaffoldDataBinder.Initialize();

        //configure vendor startup
        ConfigureVendors(app);

        //handle missing static files
        app.Use(async (context, next) => {
            await next.Invoke();
            if (context.Response.StatusCode == 404 && context.Request.Path.Value.Contains("/content/pages/"))
            {
                //missing static files that belong to Saber webpages that haven't been saved yet, 
                //or the user saved the html file using the Editor UI, but haven't saved the less or js files
                var extension = context.Request.Path.Value.GetFileExtension().ToLower();
                switch (extension)
                {
                    case "js":
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync("(function(){\n\n})();");
                        break;
                    case "css":
                        context.Response.StatusCode = 200;
                        await context.Response.WriteAsync("");
                        break;
                }
            }
        });
    }
}
