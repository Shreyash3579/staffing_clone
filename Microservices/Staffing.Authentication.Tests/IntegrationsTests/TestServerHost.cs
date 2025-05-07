using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;

namespace Staffing.Authentication.Tests.IntegrationsTests
{
    /// <summary>
    /// The TestServerHost class allows an in-memory test server to be created and 
    /// HTTP requests issued to the ASP.NET Core app
    /// </summary>
    public class TestServerHost : IDisposable
    {
        private readonly TestServer _testServer;
        public TestServerHost()
        {
            _testServer = new TestServer(new WebHostBuilder()
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build())
                .UseStartup<Startup>());

            Client = _testServer.CreateClient();
        }

        public HttpClient Client { get; set; }

        public void Dispose()
        {
            _testServer?.Dispose();
            Client?.Dispose();
        }
    }
}