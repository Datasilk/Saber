using System;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Datasilk.Core.Extensions;
using Saber.Common.Platform;

namespace Saber
{
    public class Startup
    {
        private static IConfigurationRoot config;

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

            //add gzip compression
            services.AddResponseCompression(options =>
            {
                //options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes =  new[] { "text/css", "image/svg" };
                options.EnableForHttps = true;
            });
            //services.Configure<GzipCompressionProviderOptions>(options =>
            //{
            //    options.Level = CompressionLevel.Optimal;
            //});
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });

            //get list of assemblies for Vendor related functionality
            var assemblies = new List<Assembly> { Assembly.GetCallingAssembly() };
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
            Console.WriteLine("Found " + Common.Vendors.Startups.Count + " Vendor Startup Class" + (Common.Vendors.Startups.Count != 1 ? "es" : ""));

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
            Console.WriteLine("Found " + Common.Vendors.Controllers.Count + " Vendor Controller" + (Common.Vendors.Controllers.Count != 1 ? "s" : ""));

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
            Console.WriteLine("Found " + Common.Vendors.ViewRenderers.Count + " Vendor View Renderer" + (Common.Vendors.ViewRenderers.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //get list of vendor classes that inherit IVendorHtmlComponent interface
            foreach (var assembly in assemblies)
            {
                //get a list of interfaces from the assembly
                var types = assembly.GetTypes()
                    .Where(type => typeof(Vendor.IVendorHtmlComponent).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract).ToList();
                foreach (var type in types)
                {
                    Common.Vendors.GetHtmlComponentsFromType(type);
                }
            }
            //get list of DLLs that contain the IVendorContentField interface
            Common.Vendors.GetHtmlComponentsFromFileSystem();
            Common.Vendors.GetHtmlComponentKeys();
            var totalcomponents = (Common.Vendors.HtmlComponents.Count - Common.Vendors.SpecialVars.Count);
            Console.WriteLine("Found " + (Common.Vendors.HtmlComponents.Count - 1) + " Vendor HTML Component" + ((Common.Vendors.HtmlComponents.Count - 1) != 1 ? "s" : "") +
                " (" + totalcomponents + " component" + (totalcomponents > 1 ? "s" : "") + ", " + 
                Common.Vendors.SpecialVars.Count + " special variable" + (Common.Vendors.SpecialVars.Count > 1 ? "s" : "") + ")");

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
            Console.WriteLine("Found " + Common.Vendors.ContentFields.Count + " Vendor Content Field" + (Common.Vendors.ContentFields.Count != 1 ? "s" : ""));

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
            foreach(var chain in Common.Vendors.Keys)
            {
                totalKeys += chain.Keys.Length;
            }
            Console.WriteLine("Found " + Common.Vendors.Keys.Count + " Vendor" + (Common.Vendors.Keys.Count != 1 ? "s" : "") + " with Security Keys (" + totalKeys + " key" + (totalKeys != 1 ? "s" : "") + ")");

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
            Console.WriteLine("Found " + Common.Vendors.EmailClients.Count + " Vendor Email Client" + (Common.Vendors.EmailClients.Count != 1 ? "s" : ""));

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
            Console.WriteLine("Found " + Common.Vendors.WebsiteSettings.Count + " Vendor Website Setting" + (Common.Vendors.WebsiteSettings.Count != 1 ? "s" : ""));

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Run any services required after initializing all vendor plugins but before configuring vendor startup services
            Core.Email.Handle = Email.Send;

            //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //execute ConfigureServices method for all vendors that use IVendorStartup interface
            foreach (var kv in Common.Vendors.Startups)
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

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            Server.Config = config;

            //configure Server defaults
            Server.hostUri = config.GetSection("hostUri").Value;
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
            Server.BcryptWorkfactor = int.Parse(config.GetSection("Encryption:bcrypt_work_factor").Value);
            Server.Salt = config.GetSection("Encryption:salt").Value;

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
                ContentTypeProvider = provider
            };
            app.UseResponseCompression();
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
            Server.HasAdmin = Query.Users.HasAdmin();
            var resetPass = Query.Users.HasPasswords();
            Server.HasAdmin = Query.Users.HasAdmin();

            //set up Saber language support
            Server.Languages = new Dictionary<string, string>();
            Server.Languages.Add("en", "English"); //english should be the default language
            Query.Languages.GetList().ForEach((lang) => {
                Server.Languages.Add(lang.langId, lang.language);
            });

            //check if default website exists
            

            //set up path pointers for View partials (e.g. {{side-bar "partials/side-bar.html"}} instead of {{side-bar "/Content/partials/side-bar.html"}})
            ViewPartialPointers.Paths.AddRange(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("partials", "Content/partials"),
                new KeyValuePair<string, string>("pages", "Content/pages")
            });

            //execute Configure method for all vendors that use IVendorStartup interface
            foreach (var kv in Common.Vendors.Startups)
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
            app.UseDatasilkMvc(new MvcOptions()
            {
                IgnoreRequestBodySize = true,
                ServicePaths = new string[] { "api", "gmail" },
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
