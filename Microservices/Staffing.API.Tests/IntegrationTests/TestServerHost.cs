using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net.Http;

namespace Staffing.API.Tests.IntegrationTests
{
    /// <summary>
    /// The TestServerHost class allows an in-memory test server to be created and 
    /// HTTP requests issued to the ASP.NET Core app
    /// </summary>
    public class TestServerHost : IDisposable
    {
        private readonly TestServer _testServer;
        private const string jwtToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJFbXBsb3llZUNvZGUiOiIzNzk5NSIsIm5iZiI6MTU2NzA3NDY0NiwiZXhwIjoxODgyNjkzODQ2LCJpYXQiOjE1NjcwNzQ2NDYsImlzcyI6IlN0YWZmaW5nIEF1dGhlbnRpY2F0aW9uIEFQSSIsImF1ZCI6IkFQSXMgYWNjZXNzZWQgYnkgU3RhZmZpbmcgQXBwIn0.56xDe8Mg8DeEIPZuJ4zOONW5Vnp5PhyS8ozdUHCo_Ag";

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
            Client.Timeout = TimeSpan.FromMinutes(30);
        }

        public HttpClient Client { get; set; }

        public void Dispose()
        {
            _testServer?.Dispose();
            Client?.Dispose();
        }
    }
}