using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Datasilk.Core.Extensions;
using Saber.Common.Platform;

namespace Saber
{
    public class Startup
    {
        private static IConfigurationRoot config;
        private List<Assembly> assemblies = new List<Assembly> { Assembly.GetCallingAssembly() };

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

            //try deleting Vendors that are marked for uninstallation
            Common.Vendors.DeleteVendors();

            //get list of assemblies for Vendor related functionality
            if (!assemblies.Contains(Assembly.GetExecutingAssembly()))
            {
                assemblies.Add(Assembly.GetExecutingAssembly());
            }
            if (!assemblies.Contains(Assembly.GetEntryAssembly()))
            {
                assemblies.Add(Assembly.GetEntryAssembly());
            }

            //get a list of DLLs in the Vendors folder (if any)
            var vendorDLLs = Common.Vendors.LoadDLLs();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorInfo interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorInfo).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetInfoFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorInfo interface
            Common.Vendors.GetInfoFromFileSystem();
            var vendorCount = Core.Vendors.Details.Where(a => a.Version != "").Count();
            Console.WriteLine("Found " + vendorCount + " Vendor" + (vendorCount != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorStartup interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorStartup).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetStartupFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorStartup interface
            Common.Vendors.GetStartupsFromFileSystem();
            Console.WriteLine("Found " + Core.Vendors.Startups.Count + " Vendor Startup Class" + (Core.Vendors.Startups.Count != 1 ? "es" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorController interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorController).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetControllerFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorController interface
            Common.Vendors.GetControllersFromFileSystem();
            Console.WriteLine("Found " + Core.Vendors.Controllers.Count + " Vendor Controller" + (Core.Vendors.Controllers.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorViewRenderer interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorViewRenderer).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetViewRendererFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorViewRenderer interface
            Common.Vendors.GetViewRenderersFromFileSystem();
            Console.WriteLine("Found " + Core.Vendors.ViewRenderers.Count + " Vendor View Renderer" + (Core.Vendors.ViewRenderers.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorHtmlComponent interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorHtmlComponents).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetHtmlComponentsFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorContentField interface
            Common.Vendors.GetHtmlComponentsFromFileSystem();
            Common.Vendors.GetHtmlComponentKeys();
            var totalcomponents = (Core.Vendors.HtmlComponents.Count - Core.Vendors.SpecialVars.Count);
            Console.WriteLine("Found " + (Core.Vendors.HtmlComponents.Count - 1) + " Vendor HTML Component" + ((Core.Vendors.HtmlComponents.Count - 1) != 1 ? "s" : "") +
                " (" + totalcomponents + " component" + (totalcomponents > 1 ? "s" : "") + ", " +
                Core.Vendors.SpecialVars.Count + " special variable" + (Core.Vendors.SpecialVars.Count > 1 ? "s" : "") + ")");

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorContentField interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorContentField).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetContentFieldsFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorContentField interface
            Common.Vendors.GetContentFieldsFromFileSystem();
            Console.WriteLine("Found " + Core.Vendors.ContentFields.Count + " Vendor Content Field" + (Core.Vendors.ContentFields.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorKeys interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorKeys).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetSecurityKeysFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorKeys interface
            Common.Vendors.GetSecurityKeysFromFileSystem();
            var totalKeys = 0;
            foreach(var chain in Core.Vendors.Keys)
            {
                totalKeys += chain.Keys.Length;
            }
            Console.WriteLine("Found " + Core.Vendors.Keys.Count + " Vendor" + (Core.Vendors.Keys.Count != 1 ? "s" : "") + " with Security Keys (" + totalKeys + " key" + (totalKeys != 1 ? "s" : "") + ")");

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorEmailClient interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorEmailClient).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetEmailClientsFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorEmailClient interface
            Common.Vendors.GetEmailClientsFromFileSystem();
            Console.WriteLine("Found " + Core.Vendors.EmailClients.Count + " Vendor Email Client" + (Core.Vendors.EmailClients.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorWebsiteSettings interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorWebsiteSettings).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetWebsiteSettingsFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorWebsiteSettings interface
            Common.Vendors.GetWebsiteSettingsFromFileSystem();
            Core.Vendors.WebsiteSettings = Core.Vendors.WebsiteSettings.OrderBy(a => a.Name).ToList();
            Console.WriteLine("Found " + Core.Vendors.WebsiteSettings.Count + " Vendor Website Setting" + (Core.Vendors.WebsiteSettings.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit SaberEvents abstract class
            foreach (var assembly in assemblies)
            {
                //get a list of abstract classes from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.SaberEvents).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetSaberEventsFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorKeys interface
            Common.Vendors.GetSaberEventsFromFileSystem();
            Console.WriteLine("Found " + Core.Vendors.EventHandlers.Count + " Vendor" + (Core.Vendors.EventHandlers.Count != 1 ? "s" : "") + " That listen to Saber Events");

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //execute ConfigureServices method for all vendors that use IVendorStartup interface
            foreach (var kv in Core.Vendors.Startups)
            {
                var vendor = (Vendor.IVendorStartup)Activator.CreateInstance(kv.Value);
                try
                {
                    vendor.ConfigureServices(services);
                    Console.WriteLine("Configured Services for " + kv.Key);
                }
                catch (Exception) { }
            }
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime)
        {
            App.IsDocker = System.Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            //get environment based on application build
            switch (env.EnvironmentName.ToLower())
            {
                case "production":
                    App.Environment = Environment.production;
                    break;
                case "staging":
                    App.Environment = Environment.staging;
                    break;
                default:
                    App.Environment = Environment.development;
                    break;
            }

            //load application-wide cache
            var configFile = "config" +
                (App.IsDocker ? ".docker" : "") +
                (App.Environment == Environment.production ? ".prod" : "") + ".json";

            if (!File.Exists(App.MapPath(configFile)))
            {
                //create default config.json files
                File.Copy(App.MapPath("/Content/temp/" + configFile), App.MapPath(configFile));
            }

            config = new ConfigurationBuilder()
                .AddJsonFile(App.MapPath(configFile))
                .AddEnvironmentVariables().Build();

            var webconfig = Website.Settings.Load();

            Server.Config = config;

            //configure Server defaults
            App.Host = config.GetSection("hostUri").Value ?? "http://localhost:7000";
            var servicepaths = config.GetSection("servicePaths").Value;
            if (servicepaths != null && servicepaths != "")
            {
                Server.ServicePaths = servicepaths.Replace(" ", "").Split(',');
            }
            if (config.GetSection("version").Value != null)
            {
                Server.Version = config.GetSection("version").Value;
            }

            //configure Server database connection strings
            Query.Sql.ConnectionString = config.GetSection("sql:" + config.GetSection("sql:Active").Value).Value;

            //configure Server security
            Server.BcryptWorkfactor = int.Parse(config.GetSection("encryption:bcrypt_work_factor").Value);
            Server.Salt = config.GetSection("encryption:salt").Value;

            //configure Public API developer key
            Server.DeveloperKeys = config.GetSection("developer-keys").Get<List<Models.ApiKey>>();
            Core.Service.ApiKeys = Server.DeveloperKeys;

            //inject app lifetime
            Server.AppLifetime = appLifetime;

            //configure cookie-based authentication
            var expires = !string.IsNullOrWhiteSpace(config.GetSection("Session:Expires").Value) ? int.Parse(config.GetSection("Session:Expires").Value) : 60;

            //use session
            var sessionOpts = new SessionOptions();
            sessionOpts.Cookie.Name = "Saber";
            sessionOpts.IdleTimeout = TimeSpan.FromMinutes(expires);

            app.UseSession(sessionOpts);

            //handle static files
            var provider = new FileExtensionContentTypeProvider();

            // Add static file mappings
            provider.Mappings[".svg"] = "image/svg+xml";
            var options = new StaticFileOptions
            {
                ContentTypeProvider = provider,
                OnPrepareResponse = (context) =>
                {
                    var headers = context.Context.Response.Headers;
                    var contentType = headers["Content-Type"].ToString();
                    if (context.File.PhysicalPath.Contains("wwwroot\\editor") && context.File.Name.EndsWith(".js"))
                    {
                        contentType = "application/javascript";
                        headers.Add("Content-Encoding", "gzip");
                        headers["Content-Type"] = contentType;
                    }
                }
            };

            app.UseStaticFiles(options);

            //exception handling
            if (App.Environment == Environment.development)
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
            var resetPass = Query.Users.HasPasswords();
            Server.HasAdmin = Query.Users.HasAdmin();

            //set up Saber language support
            App.Languages = new Dictionary<string, string>();
            App.Languages.Add("en", "English"); //english should be the default language

            webconfig.Languages.ForEach((lang) => {
                App.Languages.Add(lang.Id, lang.Name);
            });

            //set up path pointers for View partials (e.g. {{side-bar "partials/side-bar.html"}} instead of {{side-bar "/Content/partials/side-bar.html"}})
            ViewPartialPointers.Paths.AddRange(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("partials", "Content/partials"),
                new KeyValuePair<string, string>("pages", "Content/pages")
            });

            //check vendor versions which may run SQL migration scripts
            Common.Vendors.CheckVersions();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorDataSources interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorDataSources).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetDataSourcesFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorDataSources interface
            Common.Vendors.GetDataSourcesFromFileSystem();
            Console.WriteLine("Found " + Core.Vendors.DataSources.Count + " Vendor Data Source" + (Core.Vendors.DataSources.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Get list of all Public APIs
            var apis = PublicApi.GetList(assemblies);
            Console.WriteLine("Found " + apis.Count + " Public API endpoints");

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Run any services required after initializing all vendor plugins but before configuring vendor startup services
            Core.Delegates.Email.Send = Email.Send;
            Core.Delegates.Website.SaveLessFile = Website.SaveLessFile;
            Core.Delegates.Website.CopyTempWebsite = Website.CopyTempWebsite;
            Core.Delegates.Log.Error = Query.Logs.LogError;
            Core.Delegates.ContentFields.GetFieldType = ContentFields.GetFieldType;
            Core.Delegates.ContentFields.RenderForm = ContentFields.RenderForm;

            //execute Configure method for all vendors that use IVendorStartup interface
            foreach (var kv in Core.Vendors.Startups)
            {
                var vendor = (Vendor.IVendorStartup)Activator.CreateInstance(kv.Value);
                try
                {
                    vendor.Configure(app, env, config);
                    Console.WriteLine("Configured Startup for " + kv.Key);
                }
                catch (Exception ex) {
                    Console.WriteLine("Vendor startup error: " + ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            }

            //copy temporary website (if neccessary)
            Website.CopyTempWebsite();

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


            //run Datasilk Core MVC Middleware
            App.ServicePaths = new string[] { "api", "gmail" };
            app.UseDatasilkMvc(new MvcOptions()
            {
                IgnoreRequestBodySize = true,
                ServicePaths = App.ServicePaths,
                Routes = new Routes()
            });

            //handle missing static files
            app.Use(async (context, next) => {
                if (context.Response.StatusCode == 404 && context.Request.Path.Value.Contains("/content/"))
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
            });

            Console.WriteLine("Running Saber Server in " + App.Environment.ToString() + " environment");
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
