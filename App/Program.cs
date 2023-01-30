using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Saber;
using System.Diagnostics;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Saber.Startup>();
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<WinService>();
    })
    .Build();

host.Start();

var server = host.Services.GetRequiredService<IServer>();
var addressFeature = server.Features.Get<IServerAddressesFeature>();
if(addressFeature != null)
{
    App.Host = addressFeature.Addresses.ToArray();
    foreach (var address in addressFeature.Addresses)
    {
        Console.WriteLine($"{Process.GetCurrentProcess().ProcessName} is listening to {address} in the {Saber.App.Environment} environment");
    }
}
host.WaitForShutdown();
