using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;

namespace Vacation.API.Tests.IntegrationTests
{
    /// <summary>
    /// The TestServerHost class allows an in-memory test server to be created and 
    /// HTTP requests issued to the ASP.NET Core app
    /// </summary>
    public class TestServerHost : IDisposable
    {
        private readonly TestServer _testServer;
        private const string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6IjM3OTk1IiwibmJmIjoxNTU5ODg1MjM4LCJleHAiOjE4NzU1MDQ0MzgsImlhdCI6MTU1OTg4NTIzOCwiaXNzIjoiU3RhZmZpbmcgQXV0aGVudGljYXRpb24gQVBJIiwiYXVkIjoiQVBJcyBhY2Nlc3NlZCBieSBTdGFmZmluZyBBcHAifQ.nCQ72obTKROxl6Y6V7AP4W9bZoQYjySa6eXz78KpMAw";

        public TestServerHost()
        {
            _testServer = new TestServer(new WebHostBuilder()
                .UseConfiguration(new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build())
                .UseStartup<Startup>());

            Client = _testServer.CreateClient();
            Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {jwtToken}");
        }

        public HttpClient Client { get; set; }

        public void Dispose()
        {
            _testServer?.Dispose();
            Client?.Dispose();
        }
    }
}