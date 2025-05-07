using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.CaseRollController")]
    public class CaseRollControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public CaseRollControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("T7HW,F2CB")]
        public async Task GetCasesOnRollByCaseCodes_should_return_listofCasesOnRoll(string oldCaseCodes)
        {
            // Arrange
            var payload = new { oldCaseCodes };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/caseroll/getCasesOnRollByCaseCodes", payload);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var casesOnRoll = JsonConvert.DeserializeObject<IEnumerable<CaseRoll>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            casesOnRoll.Count().Should().BeGreaterOrEqualTo(0);
            casesOnRoll.ForEach(c => c.RolledFromOldCaseCode.Should().NotBeNullOrEmpty());
            casesOnRoll.ForEach(c => c.RolledFromOldCaseCode.Should().BeOneOf(oldCaseCodes.Split(',')));
        }
    }
}
