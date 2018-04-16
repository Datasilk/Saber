using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Diagnostics;
using System.Threading;

public class Startup : Datasilk.Startup {

    public override void Configured(IApplicationBuilder app, IHostingEnvironment env, IConfigurationRoot config)
    {
        base.Configured(app, env, config);
        var query = new Saber.Query.Users(server.sqlConnectionString);
        server.resetPass = query.HasPasswords();
        server.hasAdmin = query.HasAdmin();

        server.languages = new Dictionary<string, string>();
        var languages = new Saber.Query.Languages(server.sqlConnectionString);
        server.languages.Add("en", "English"); //english should be the default language
        languages.GetList().ForEach((lang) => {
            server.languages.Add(lang.langId, lang.language);
        });

        //check if default website exists
        if (!File.Exists(server.MapPath("/Content/pages/home.html")))
        {
            //copy default website since none exists yet
            Directory.CreateDirectory(server.MapPath("/Content/pages/"));
            Directory.CreateDirectory(server.MapPath("/wwwroot/content/"));
            Directory.CreateDirectory(server.MapPath("/wwwroot/content/pages/"));
            Directory.CreateDirectory(server.MapPath("/wwwroot/images/"));
            Utility.FileSystem.CopyDirectoryContents(server.MapPath("/Content/temp/pages/"), server.MapPath("/Content/pages/"));
            Utility.FileSystem.CopyDirectoryContents(server.MapPath("/Content/temp/resources/"), server.MapPath("/wwwroot/content/pages/"));
            Utility.FileSystem.CopyDirectoryContents(server.MapPath("/Content/temp/images/"), server.MapPath("/wwwroot/images/"));
            Utility.FileSystem.CopyDirectoryContents(server.MapPath("/Content/temp/partials/"), server.MapPath("/Partials/"));
            File.Copy(server.MapPath("/Content/temp/css/website.less"), server.MapPath("/CSS/website.less"));

            Thread.Sleep(1000);

            //run default gulp command to copy new website resources to wwwroot folder
            var p = new Process();
            p.StartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = "/c gulp default:website",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                WorkingDirectory = server.MapPath("/").Replace("App\\", ""),
                Verb = "runas"
            };
            p.OutputDataReceived += GulpOutputReceived;
            p.ErrorDataReceived += GulpErrorReceived;
            p.Start();
            p.WaitForExit();
            Thread.Sleep(1000);
        }
    }

    public override void Run(HttpContext context)
    {
        context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = 1_024_000_000; //1 GB
        base.Run(context);
    }

    #region "Gulp Events"
    private void GulpOutputReceived(object sender, DataReceivedEventArgs e)
    {
        Process p = sender as Process;
        if (p == null) { return; }
        Console.WriteLine(e.Data);
    }

    private void GulpErrorReceived(object sender, DataReceivedEventArgs e)
    {
        Process p = sender as Process;
        if (p == null) { return; }
        Console.WriteLine(e.Data);
    }
    #endregion
}
