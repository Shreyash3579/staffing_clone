using Microservices.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Logger.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Logger.API.ErrorLogsController")]
    public class ErrorLogsControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public ErrorLogsControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [ClassData(typeof(ErrorLogsTestDataGenerator))]
        public async Task LogErrors_should_be_successful(ErrorLogs errorLogs)
        {
            //var content = new StringContent(string.Format("USERNAME={0}", "Permagate"));
            //var content = new FormUrlEncodedContent(errorLogs);
            var jsonObj = JsonConvert.SerializeObject(errorLogs);

            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/ErrorLogs", errorLogs);
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task LogErrors_With_Dynamic_Parameter_should_be_successful()
        {
            var payLoad = new Dictionary<string, string>()
            {
                { "errorMessage", "Test Error Message" },
                { "stackTrace", "Test Stack Trace" }
            };
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/errorlogs/logclienterrors", payLoad);
            response.EnsureSuccessStatusCode();
        }
    }

    public class ErrorLogsTestDataGenerator : IEnumerable<object[]>
    {

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new ErrorLogs
                {
                    Error = new NotImplementedException("Test Data"),
                    ApplicationName = "Test Application",
                    EmployeeCode = "45088"
                }

            };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    }
}


