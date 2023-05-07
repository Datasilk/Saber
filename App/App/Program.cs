using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Saber;
using System.Collections;
using System.Diagnostics;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<WinService>();
    })
    .Build();

host.Start();

Console.WriteLine("");
Console.WriteLine("Working path: " + App.RootPath);
var envars = System.Environment.GetEnvironmentVariables();
if(envars != null && envars.Keys.Count > 0 && !envars.Contains("VisualStudioDir"))
{
    Console.WriteLine("");
    Console.WriteLine("Environment Variables:");
    foreach (DictionaryEntry de in envars)
    {
        Console.WriteLine("{0} = {1}", de.Key, de.Value);
    }
    Console.WriteLine("");
}

var server = host.Services.GetRequiredService<IServer>();
var addressFeature = server.Features.Get<IServerAddressesFeature>();
if(addressFeature != null)
{
    App.Host = addressFeature.Addresses.ToArray();
    foreach (var address in App.Host)
    {
        Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} is listening to {address} in the {App.Environment} environment");
    }
}
host.WaitForShutdown();
