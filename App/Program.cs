﻿using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Saber
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                .UseContentRoot(Directory.GetCurrentDirectory())

                ////////////////////////////////////////////////////////////////////////////////
                /////Choose Your Platform
                ////////////////////////////////////////////////////////////////////////////////

                //IIS for Window ////////////////////////////////////////////
                //.UseIISIntegration()

                //Kestrel for Linux & MacOSX ////////////////////////////////
                .UseKestrel(
                    options =>
                    {
                        options.Limits.MaxRequestBodySize = null;
                    }
                )

                //Kestrel for Docker ///////////////////////////////////////
                //.UseKestrel(
                //    options =>
                //    {
                //        options.Limits.MaxRequestBodySize = null;
                //        options.ListenAnyIP(80); //for docker
                //    }
                //)


                .UseStartup<Startup>();
            })
            .ConfigureServices(services =>
            {
                services.AddLogging(cfg =>
                    cfg.AddConsole(opts =>
                    {
                        opts.IncludeScopes = false;
                    }));
            });


        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}