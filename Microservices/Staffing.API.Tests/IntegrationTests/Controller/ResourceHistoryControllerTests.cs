using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.ResourceHistoryController")]
    public class ResourceHistoryControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public ResourceHistoryControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("44ACM")]
        public async Task GetResourceHistoryData_should_return_employeeData_inDescendingOrder_forEndDate(string employeeCode)
        {
            //Act
            var lastYearDate = DateTime.Today.AddYears(-1);
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/resourceHistory?employeeCode={employeeCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceHistoryData = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocationViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            resourceHistoryData.Count().Should().BeGreaterOrEqualTo(0);
            resourceHistoryData.ForEach(c => c.OldCaseCode.Should().NotBeNullOrEmpty());
            resourceHistoryData.ForEach(c => c.CurrentLevelGrade.Should().NotBeNullOrEmpty());
            resourceHistoryData.ForEach(c => c.Allocation.Should().BeGreaterOrEqualTo(0));
            //Asserting order of Historical data should be EndDate Desc
            for (var index = 1; index < resourceHistoryData.Count(); index++)
            {
                resourceHistoryData[index].EndDate.Should().BeOnOrBefore((DateTime)resourceHistoryData[index - 1].EndDate);
            }
        }
    }
}
