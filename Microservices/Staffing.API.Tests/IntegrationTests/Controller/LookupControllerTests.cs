using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.LookupController")]
    public class LookupControllerTests : IClassFixture<TestServerHost>
    {
        public LookupControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        private readonly TestServerHost _testServer;

        [Fact]
        public async Task GetCaseRoleTypeLookupList_should_return_listOfCaseRoleTypes()
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    "/api/lookup/caseRoleTypes");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var caseRoles = JsonConvert.DeserializeObject<IEnumerable<CaseRoleType>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            caseRoles.Count().Should().BeGreaterOrEqualTo(0);
            caseRoles.ForEach(c => c.CaseRoleCode.Should().NotBeNullOrEmpty());
            caseRoles.ForEach(c => c.CaseRoleName.Should().NotBeNullOrEmpty());
        }

        [Fact]
        public async Task GetInvestmentCategoryLookupList_should_return_listOfInvestmentCategories()
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    "/api/lookup/investmentTypes");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var investments = JsonConvert.DeserializeObject<IEnumerable<InvestmentCategory>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            investments.Count().Should().BeGreaterOrEqualTo(0);
            investments.ForEach(c => c.InvestmentCode.Should().NotBe(0));
            investments.ForEach(c => c.InvestmentName.Should().NotBeNullOrEmpty());
            investments.ForEach(c => c.InvestmentDescription.Should().NotBeNullOrEmpty());
        }
    }
}