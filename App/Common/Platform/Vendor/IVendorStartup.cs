using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Saber.Vendor
{
    /// <summary>
    /// An interface used by vendors to extend the Startup class for Saber
    /// </summary>
    public interface IVendorStartup
    {
        void ConfigureServices(IServiceCollection services) { }
        void Configure(IApplicationBuilder app, IWebHostEnvironment env, IConfigurationRoot config) { }
    }
}
