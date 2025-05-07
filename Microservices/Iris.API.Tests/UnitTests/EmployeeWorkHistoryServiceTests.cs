using Iris.API.Contracts.RepositoryInterfaces;
using Iris.API.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Iris.API.Models;
using Iris.API.Core.Services;
using System.Linq;
using FluentAssertions;

namespace Iris.API.Tests.UnitTests
{
    public class EmployeeWorkHistoryServiceTests
    {
        [Theory]
        [InlineData("01KNI")]
        public async Task GetEmployeeWorkHistory_should_return_employee_with_work_history(string employeeCode)
        {
            //Arrange
            var mockEmployeeWorkHistoryRepo = new Mock<IEmployeeWorkHistoryRepository>();
            mockEmployeeWorkHistoryRepo.Setup(x => x.GetEmployeeWorkHistory(employeeCode)).ReturnsAsync(GetFakeWorkHistory());
            var sut = new EmployeeWorkHistoryService(mockEmployeeWorkHistoryRepo.Object);

            //Act
            var result = (await sut.GetEmployeeWorkHistory(employeeCode)).ToList();
            result.ForEach(wh => wh.EmployeeCode.Should().NotBeNullOrEmpty());
            result.ForEach(wh => wh.Company.Should().NotBeNullOrEmpty());
            result.ForEach(wh => wh.CompanyIndustry.Should().NotBeNullOrEmpty());
            result.ForEach(wh => wh.StartDate.Should().NotBe(DateTime.MinValue));
            result.ForEach(wh => wh.EndDate.Should().NotBe(DateTime.MinValue));

            result.All(wh => wh.EmployeeCode.Should().Equals(employeeCode));
            result.FirstOrDefault(wh => wh.EmployeeCode == employeeCode).Company.Should().Equals("New York University");
            result.FirstOrDefault(wh => wh.EmployeeCode == employeeCode).CompanyIndustry.Should().Equals("Education");
            result.FirstOrDefault(wh => wh.EmployeeCode == employeeCode).EndDate.Should().Equals(new DateTime(2005, 06, 01));
            result.FirstOrDefault(wh => wh.EmployeeCode == employeeCode).StartDate.Should().Equals(new DateTime(2005, 10, 17));
        }

        private IEnumerable<EmployeeWorkHistory> GetFakeWorkHistory()
        {
            var fakeResponse =  new List<EmployeeWorkHistory>
            {
                new EmployeeWorkHistory
                {
                    EmployeeCode = "01KNI",
                    Company = "New York University",
                    CompanyIndustry = "Education",
                    EndDate = new DateTime(2005, 06, 01),
                    StartDate = new DateTime(2005, 10, 17)
                }
            };

            return fakeResponse;
        }
    }
}
