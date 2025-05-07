using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Vacation.API.Tests.IntegrationTests;
using Vacation.API.ViewModels;
using Xunit;

namespace Vacation.API.Tests.IntegrationsTests.Controller
{
    [Trait("IntegrationTest", "Vacation.API.VacationRequestController")]
    public class VacationRequestControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public VacationRequestControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("39209")]
        public async Task GetFutureVacationRequestsByEmployee_should_return_vacations_for_Employee(string employeeCode)
        {
            var today = DateTime.Today;
            var minDate = DateTime.MinValue;
            //Act
            var response =
                await _testServer.Client.GetAsync($"/api/vacationrequest?employeeCode={employeeCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualVacationRequests = JsonConvert.DeserializeObject<IEnumerable<VacationRequestViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            if (actualVacationRequests.Count() > 0)
            {
                actualVacationRequests.ForEach(x => x.StartDate.Should().NotBe(minDate));
                actualVacationRequests.ForEach(x => x.EndDate.Should().NotBe(minDate));
                actualVacationRequests.ForEach(x => x.EndDate.Should().NotBeBefore(today));
                actualVacationRequests.ForEach(x => x.Status.Should().NotBeNullOrEmpty());
                actualVacationRequests.ForEach(x => x.Description.Should().NotBeNullOrEmpty());
                actualVacationRequests.ForEach(x => x.Type.Should().Be("Vacation"));
            }
        }

        [Theory]
        [InlineData("40789,31JWE", "2019-01-01", "2020-01-01")]
        public async Task GetVacationsWithinDateRangeByEmployeeCodes_should_return_vacations_for_Employees(string employeeCodes, DateTime startDate, DateTime endDate)
        {
            // Arrange

            var payload = new
            {
                employeeCodes = employeeCodes,
                startDate = startDate,
                endDate = endDate
            };
            var json = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            //Act
            var response =
                await _testServer.Client.PostAsync($"/api/vacationrequest/vacationsWithinDateRangeByEmployees", stringContent);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualVacationRequests = JsonConvert.DeserializeObject<IEnumerable<VacationRequestViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            actualVacationRequests.ForEach(r => r.EndDate.Should().BeOnOrAfter(startDate));
            actualVacationRequests.ForEach(r => r.StartDate.Should().BeOnOrBefore(endDate));
            actualVacationRequests.ForEach(r => r.EmployeeCode.Should().ContainAny(employeeCodes.Split(",")));
            actualVacationRequests.ForEach(r => r.Status.Should().ContainAny("Pending", "Approved"));

            var orderedVacationRequests = actualVacationRequests.OrderBy(l => l.StartDate);
            orderedVacationRequests.SequenceEqual(actualVacationRequests).Should().BeTrue();
        }
    }
}