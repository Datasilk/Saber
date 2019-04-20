using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

public class Startup : Datasilk.Startup {

    public override void Configured(IApplicationBuilder app, IHostingEnvironment env, IConfigurationRoot config)
    {
        base.Configured(app, env, config);
        Query.Sql.connectionString = Server.sqlConnectionString;
        var resetPass = Query.Users.HasPasswords();
        Server.hasAdmin = Query.Users.HasAdmin();

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
            Saber.Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/resources/"), Server.MapPath("/wwwroot/content/pages/"));
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
    }
}
