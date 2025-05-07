using FluentAssertions;
using Iris.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Iris.API.Tests.IntegrationTests.Controller
{
    public class EmployeeWorkHistoryControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public EmployeeWorkHistoryControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }

        [Theory]
        [InlineData("01KNI")]
        public async Task GetWorkHistoryByEmployeeCodeList_should_return_school_history(string employeeCode)
        {
            //Act
            var response = await _testServer.Client.GetAsync($"/api/employeeWorkHistory/workHistoryByEmployeeCode?employeeCode={employeeCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var workHistory = JsonConvert.DeserializeObject<IEnumerable<EmployeeWorkHistory>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (workHistory.Count() > 0)
            {
                workHistory.ForEach(wh => wh.EmployeeCode.Should().NotBeNullOrEmpty());
                workHistory.ForEach(wh => wh.EmployeeCode.Should().Equals(employeeCode));
                workHistory.ForEach(wh => wh.Company.Should().NotBeNullOrEmpty());
                workHistory.ForEach(wh => wh.CompanyIndustry.Should().NotBeNullOrEmpty());
                workHistory.ForEach(wh => wh.StartDate.Should().NotBe(DateTime.MinValue));
                workHistory.ForEach(wh => wh.EndDate.Should().NotBe(DateTime.MinValue));
            }
        }
    }
}
