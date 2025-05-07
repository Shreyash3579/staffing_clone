using BvuCD.API.ViewModels;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BvuCD.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "BvuCD.API.TrainingController")]
    public class TrainingControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public TrainingControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("42AUM")]
        public async Task GetFutureTrainingsByEmployee_should_return_trainings_with_futureEndDates(string employeeCode)
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/training?employeeCode={employeeCode}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualTrainings = JsonConvert.DeserializeObject<IEnumerable<TrainingViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            if (actualTrainings.Any())
            {
                actualTrainings.ForEach(c => c.Role.Should().NotBeNullOrEmpty());
                actualTrainings.ForEach(c => c.StartDate.Should().NotBe(DateTime.MinValue));
                actualTrainings.ForEach(c => c.EndDate.Should().NotBe(DateTime.MinValue));
                actualTrainings.ForEach(c => c.EndDate.Should().BeOnOrAfter(DateTime.Today));
                actualTrainings.ForEach(c => c.TrainingName.Should().NotBeNullOrEmpty());
                actualTrainings.ForEach(c => c.Type.Should().Be("Training"));
            }
        }

        [Theory]
        [InlineData("40789,42AUM", "2019-01-01", "2020-01-01")]
        public async Task GetTrainingsWithinDateRangeByEmployeeCodes_should_return_trainings_for_Employees(string employeeCodes, DateTime startDate, DateTime endDate)
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
                await _testServer.Client.PostAsync($"/api/training/trainingsWithinDateRangeByEmployees", stringContent);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualTrainings = JsonConvert.DeserializeObject<IEnumerable<TrainingViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            actualTrainings.ForEach(r => r.EndDate.Should().BeOnOrAfter(startDate));
            actualTrainings.ForEach(r => r.StartDate.Should().BeOnOrBefore(endDate));
            actualTrainings.ForEach(r => r.Role.Should().NotBeNullOrEmpty());
            actualTrainings.ForEach(r => r.TrainingName.Should().NotBeNullOrEmpty());
            actualTrainings.ForEach(r => r.EmployeeCode.Should().ContainAny(employeeCodes.Split(",")));

            var orderedTrainings = actualTrainings.OrderBy(l => l.StartDate);
            orderedTrainings.SequenceEqual(actualTrainings).Should().BeTrue();
        }
    }
}