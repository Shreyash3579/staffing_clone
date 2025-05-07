using FluentAssertions;
using Iris.API.Contracts.RepositoryInterfaces;
using Iris.API.Core.Services;
using Iris.API.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Iris.API.Tests.UnitTests
{
    public class EmployeeSchoolHistoryServiceTests
    {
        [Theory]
        [InlineData("01KNI")]
        public async Task GetEmployeeSchoolHistory_should_return_employee_with_school_history(string employeeCode)
        {
            //Arrange
            var mockEmployeeSchoolHistoryRepo = new Mock<IEmployeeSchoolHistoryRepository>();
            mockEmployeeSchoolHistoryRepo.Setup(x => x.GetEmployeeSchoolHistory(employeeCode)).ReturnsAsync(GetFakeSchoolHistory());
            var sut = new EmployeeSchoolHistoryService(mockEmployeeSchoolHistoryRepo.Object);

            //Act
            var result = (await sut.GetEmployeeSchoolHistory(employeeCode)).ToList();
            result.ForEach(wh => wh.EmployeeCode.Should().NotBeNullOrEmpty());
            result.ForEach(wh => wh.School.Should().NotBeNullOrEmpty());
            result.ForEach(wh => wh.Degree.Should().NotBeNullOrEmpty());
            result.ForEach(wh => wh.Major.Should().NotBeNullOrEmpty());

            result.All(wh => wh.EmployeeCode.Should().Equals(employeeCode));
            result.FirstOrDefault(wh => wh.EmployeeCode == employeeCode).School.Should().Equals("Columbia University");
            result.FirstOrDefault(wh => wh.EmployeeCode == employeeCode).Degree.Should().Equals("Master of Arts");
            result.FirstOrDefault(wh => wh.EmployeeCode == employeeCode).Major.Should().Equals("Administrative and Policy Studies");
        }

        private IEnumerable<EmployeeSchoolHistory> GetFakeSchoolHistory()
        {
            var fakeResponse = new List<EmployeeSchoolHistory>
            {
                new EmployeeSchoolHistory
                {
                    EmployeeCode = "01KNI",
                    School = "Columbia University",
                    Degree = "Master of Arts",
                    Major = "Administrative and Policy Studies"

                },
                new EmployeeSchoolHistory
                {
                    EmployeeCode = "01KNI",
                    School = "New York University (NYU)",
                    Degree = "Bachelor of Arts",
                    Major = "Political Studies and Sociology"
                },
            };

            return fakeResponse;
        }
    }
}
