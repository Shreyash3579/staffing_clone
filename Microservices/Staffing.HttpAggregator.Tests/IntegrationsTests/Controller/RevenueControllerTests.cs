using FluentAssertions;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Tests.IntegrationTests;
using Staffing.HttpAggregator.ViewModels;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.HttpAggregator.Tests.IntegrationsTests.Controller
{
    public class RevenueControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public RevenueControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData(18507, 171)]
        public async Task GetRevenueByClientCodeAndCaseCode_should_return_revenues_for_case(int clientCode, int caseCode)
        {
            // Arrange
            var payload = new { clientCode, caseCode };
            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/revenue/getRevenueByClientCodeAndCaseCode", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var revenueData = JsonConvert.DeserializeObject<RevenueViewModel>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData(18507, "8898bab1-50d8-4798-b803-30edaf7e59e9")]
        public async Task GetRevenueByClientCodeAndCaseCode_should_return_revenues_for_opportunity(int clientCode, string pipelineId)
        {
            // Arrange
            var payload = new { clientCode, pipelineId };
            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/revenue/getRevenueByClientCodeAndCaseCode", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var revenueData = JsonConvert.DeserializeObject<RevenueViewModel>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
