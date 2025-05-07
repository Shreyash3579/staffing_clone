using CCM.API.Models;
using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace CCM.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "CCM.API.CaseTypeController")]
    public class CaseTypeControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public CaseTypeControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }

        [Fact]
        public async Task GetCaseTypeList_should_return_caseTypeList()
        {
            //Act
            var response = await _testServer.Client.GetAsync($"/api/caseType/casetypes");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var caseTypes = JsonConvert.DeserializeObject<IEnumerable<CaseType>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            caseTypes.ForEach(c => c.CaseTypeCode.Should().NotBe(0));
            caseTypes.ForEach(c => c.CaseTypeName.Should().NotBeNullOrEmpty());
        }


    }
}
