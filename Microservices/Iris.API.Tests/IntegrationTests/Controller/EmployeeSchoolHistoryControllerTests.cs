using FluentAssertions;
using Iris.API.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Iris.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Iris.API.EmployeeSchoolHistoryController")]
    public class EmployeeSchoolHistoryControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public EmployeeSchoolHistoryControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }

        [Theory]
        [InlineData("01KNI")]
        public async Task GetSchoolHistoryByEmployeeCodeList_should_return_school_history(string employeeCode)
        {
            //Act
            var response = await _testServer.Client.GetAsync($"/api/employeeSchoolHistory/schoolHistoryByEmployeeCode?employeeCode={employeeCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var schoolHistory = JsonConvert.DeserializeObject<IEnumerable<EmployeeSchoolHistory>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (schoolHistory.Count() > 0)
            {
                schoolHistory.ForEach(sh => sh.EmployeeCode.Should().NotBeNullOrEmpty());
                schoolHistory.ForEach(sh => sh.EmployeeCode.Should().Equals(employeeCode));
                schoolHistory.ForEach(sh => sh.Degree.Should().NotBeNullOrEmpty());
                schoolHistory.ForEach(sh => sh.School.Should().NotBeNullOrEmpty());
                schoolHistory.ForEach(sh => sh.Major.Should().NotBeNullOrEmpty());
            }
        }
    }
}
