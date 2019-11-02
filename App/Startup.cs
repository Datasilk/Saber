using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Datasilk.Core.Extensions;
using Saber.Common.Platform;

namespace Saber
{
    public class Startup
    {
        protected static IConfigurationRoot config;

        public virtual void ConfigureServices(IServiceCollection services)
        {
            //set up Server-side memory cache
            services.AddDistributedMemoryCache();
            services.AddMemoryCache();

            //configure request form options
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue;
                x.MultipartHeadersLengthLimit = int.MaxValue;
            });

            //add session
            services.AddSession();

            //add hsts
            //services.AddHsts(options => { });
            services.AddHttpsRedirection(options => { });
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //set root Server path
            var path = env.ContentRootPath + "\\";

            Server.RootPath = path;

            //get environment based on application build
            switch (env.EnvironmentName.ToLower())
            {
                case "production":
                    Server.environment = Server.Environment.production;
                    break;
                case "staging":
                    Server.environment = Server.Environment.staging;
                    break;
                default:
                    Server.environment = Server.Environment.development;
                    break;
            }

            //load application-wide cache
            var configFile = "config" + (Server.environment == Server.Environment.production ? ".prod" : "") + ".json";
            config = new ConfigurationBuilder()
                .AddJsonFile(Server.MapPath(configFile))
                .AddEnvironmentVariables().Build();

            Server.config = config;

            //configure Server defaults
            Server.nameSpace = config.GetSection("assembly").Value;
            Server.defaultController = config.GetSection("defaultController").Value;
            Server.defaultServiceMethod = config.GetSection("defaultServiceMethod").Value;
            Server.hostUrl = config.GetSection("hostUrl").Value;
            var servicepaths = config.GetSection("servicePaths").Value;
            if (servicepaths != null && servicepaths != "")
            {
                Server.servicePaths = servicepaths.Replace(" ", "").Split(',');
            }
            if (config.GetSection("version").Value != null)
            {
                Server.Version = config.GetSection("version").Value;
            }

            //configure Server database connection strings
            Server.sqlActive = config.GetSection("sql:Active").Value;
            Server.sqlConnectionString = config.GetSection("sql:" + Server.sqlActive).Value;

            //configure Server security
            Server.bcrypt_workfactor = int.Parse(config.GetSection("Encryption:bcrypt_work_factor").Value);
            Server.salt = config.GetSection("Encryption:salt").Value;

            //configure cookie-based authentication
            var expires = !string.IsNullOrEmpty(config.GetSection("Session:Expires").Value) ? int.Parse(config.GetSection("Session:Expires").Value) : 60;

            //use session
            var sessionOpts = new SessionOptions();
            sessionOpts.Cookie.Name = Server.nameSpace;
            sessionOpts.IdleTimeout = TimeSpan.FromMinutes(expires);

            app.UseSession(sessionOpts);

            //handle static files
            var provider = new FileExtensionContentTypeProvider();

            // Add static file mappings
            provider.Mappings[".svg"] = "image/svg";
            var options = new StaticFileOptions
            {
                ContentTypeProvider = provider
            };
            app.UseStaticFiles(options);

            //exception handling
            if (Server.environment == Server.Environment.development)
            {
                app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions
                {
                    SourceCodeLineCount = 10
                });
            }

            //use HTTPS
            app.UseHsts();
            app.UseHttpsRedirection();

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
                Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/images/"), Server.MapPath("/wwwroot/images/"));
                Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/scripts/"), Server.MapPath("/wwwroot/js/"));
                Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/scripts/"), Server.MapPath("/Scripts/"));
                Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/partials/"), Server.MapPath("/Content/partials/"));
                Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/pages/"), Server.MapPath("/Content/pages/"));
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
                p.OutputDataReceived += Common.ProcessInfo.Gulp.OutputReceived;
                p.ErrorDataReceived += Common.ProcessInfo.Gulp.ErrorReceived;
                p.Start();
                p.WaitForExit();
                Thread.Sleep(1000);
            }

            //initialize platform-specific html variables for scaffolding
            ViewDataBinder.Initialize();

            //run Datasilk application
            app.UseDatasilkMvc(new MvcOptions()
            {
                IgnoreRequestBodySize = true,
                Routes = new Routes(),
                WriteDebugInfoToConsole = true
            });

            //handle missing static files
            app.Use(async (context, next) => {
                if (context.Response.StatusCode == 404 && context.Request.Path.Value.Contains("/content/pages/"))
                {
                    //missing static files that belong to Saber webpages that haven't been saved yet, 
                    //or the user saved the html file using the Editor UI, but haven't saved the less or js files
                    var extension = GetFileExtension(context.Request.Path.Value).ToLower();
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
                //await next.Invoke();
            });
        }

        private string GetFileExtension(string filename)
        {
            for (int x = filename.Length - 1; x >= 0; x += -1)
            {
                if (filename.Substring(x, 1) == ".")
                {
                    return filename.Substring(x + 1);
                }
            }

            return "";
        }
    }
}
