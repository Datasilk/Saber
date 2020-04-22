using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
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
        private static IConfigurationRoot config;
        private Dictionary<string, Type> vendors = new Dictionary<string, Type>();

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

            //add health checks
            services.AddHealthChecks();

            var assemblies = new List<Assembly> { Assembly.GetCallingAssembly() };
            if (!assemblies.Contains(Assembly.GetExecutingAssembly()))
            {
                assemblies.Add(Assembly.GetExecutingAssembly());
            }
            if (!assemblies.Contains(Assembly.GetEntryAssembly()))
            {
                assemblies.Add(Assembly.GetEntryAssembly());
            }

            //get list of vendor classes that inherit IVendorViewRenderer interface
            var vendorCount = 0;
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorController).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    if (!type.Equals(typeof(Vendor.IVendorController)))
                    {
                        Server.vendorControllers.Add(type.Name.ToLower(), type);
                        vendorCount++;
                    }
                }
            }

            Console.WriteLine("Found " + vendorCount + " Vendor Controller" + (vendorCount != 1 ? "s" : "") + " that inherit IVendorController");
            vendorCount = 0;

            //get list of vendor classes that inherit IVendorViewRenderer interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorViewRenderer).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    if (!type.Equals(typeof(Vendor.IVendorViewRenderer)))
                    {
                        var attributes = type.GetCustomAttributes<Vendor.ViewPathAttribute>();
                        foreach (var attr in attributes)
                        {
                            if (!Server.viewRenderers.ContainsKey(attr.Path))
                            {
                                Server.viewRenderers.Add(attr.Path, new List<Vendor.IVendorViewRenderer>());
                            }
                            Server.viewRenderers[attr.Path].Add((Vendor.IVendorViewRenderer)Activator.CreateInstance(type));
                            vendorCount += 1;
                        }
                    }
                }
            }

            Console.WriteLine("Found " + vendorCount + " Vendor View Renderer" + (vendorCount != 1 ? "s" : "") + " that inherit IVendorViewRenderer");
            vendorCount = 0;

            //get list of vendor classes that inherit IVendorStartup interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorStartup).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    if (!type.Equals(typeof(Vendor.IVendorStartup)))
                    {
                        vendors.Add(type.FullName, type);
                        vendorCount++;
                    }
                }
            }

            Console.WriteLine("Found " + vendorCount + " Vendor" + (vendorCount != 1 ? "s" : "") + " that inherit IVendorStartup");

            //execute ConfigureServices method for all vendors that use IVendorStartup interface
            foreach(var kv in vendors)
            {
                var vendor = (Vendor.IVendorStartup)Activator.CreateInstance(kv.Value);
                try
                {
                    vendor.ConfigureServices(services);
                    Console.WriteLine("Configured Service " + kv.Key);
                }
                catch (Exception) { }
            }
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Server.IsDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";


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
            var configFile = "config" +
                (Server.IsDocker ? ".docker" : "") +
                (Server.environment == Server.Environment.production ? ".prod" : "") + ".json";
            config = new ConfigurationBuilder()
                .AddJsonFile(Server.MapPath(configFile))
                .AddEnvironmentVariables().Build();

            Server.config = config;

            //configure Server defaults
            Server.hostUri = config.GetSection("hostUri").Value;
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
            Query.Sql.ConnectionString = config.GetSection("sql:" + config.GetSection("sql:Active").Value).Value;

            //configure Server security
            Server.bcrypt_workfactor = int.Parse(config.GetSection("Encryption:bcrypt_work_factor").Value);
            Server.salt = config.GetSection("Encryption:salt").Value;

            //configure cookie-based authentication
            var expires = !string.IsNullOrWhiteSpace(config.GetSection("Session:Expires").Value) ? int.Parse(config.GetSection("Session:Expires").Value) : 60;

            //use session
            var sessionOpts = new SessionOptions();
            sessionOpts.Cookie.Name = "Gmaster";
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
            else
            {
                //use HTTPS
                app.UseHsts();
                app.UseHttpsRedirection();

                //use health checks
                app.UseHealthChecks("/health");
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //set up database
            Server.hasAdmin = Query.Users.HasAdmin();
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
                Common.Utility.FileSystem.CopyDirectoryContents(Server.MapPath("/Content/temp/fonts/"), Server.MapPath("/wwwroot/fonts/")); 
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

            //execute Configure method for all vendors that use IVendorStartup interface
            foreach (var kv in vendors)
            {
                var vendor = (Vendor.IVendorStartup)Activator.CreateInstance(kv.Value);
                try
                {
                    vendor.Configure(app, env, config);
                    Console.WriteLine("Configured Startup for " + kv.Key);
                }
                catch (Exception) { }
            }

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////




            //run Datasilk application
            app.UseDatasilkMvc(new MvcOptions()
            {
                IgnoreRequestBodySize = true,
                WriteDebugInfoToConsole = true,
                ServicePaths = new string[] { "api", "gmail" },
                Routes = new Routes()
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

            Console.WriteLine("Running Saber Server in " + Server.environment.ToString() + " environment");
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
