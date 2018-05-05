using System.Net.Http;
using Xunit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Saber.Tests
{
    public partial class Integration
    {
        private readonly TestServer server;
        private readonly HttpClient client;

        public Integration()
        {
            server = new TestServer(new WebHostBuilder()
            .UseStartup<Startup>()
            .UseEnvironment("Development"));
            client = server.CreateClient();
        }

        [Fact]
        public async Task LogIn()
        {
            //Act
            var auth = new Models.Authenticate("entingh@gmail.com", "blackMustang777");
            var content = new StringContent(JsonConvert.SerializeObject(auth));
            var response = await client.PostAsync("/api/User/Authenticate", content);

            //Assert
            response.EnsureSuccessStatusCode();
        }
    }
}
