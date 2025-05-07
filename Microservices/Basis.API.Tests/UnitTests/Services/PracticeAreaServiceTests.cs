using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Core.Services;
using Basis.API.Models;
using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Basis.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Basis.API.PracticeAreaService")]
    public class PracticeAreaServiceTests
    {
        [Fact]
        public async Task GetAllPracticeArea_should_return_all_practice_areas_from_basis_database()
        {
            //Arrange
            var mockPracticeAreaRepo = new Mock<IPracticeAreaRepository>();

            mockPracticeAreaRepo.Setup(x => x.GetAllPracticeArea())
                .ReturnsAsync(GetFakePracticeAreas());
            var sut = new PracticeAreaService(mockPracticeAreaRepo.Object);

            //Act
            var result = await sut.GetAllPracticeArea();
            var results = result.ToList();

            results.ForEach(pa => pa.PracticeAreaCode.Should().NotBeNullOrEmpty());
            results.ForEach(pa => pa.PracticeAreaName.Should().NotBeNullOrEmpty());
        }

        [Theory]
        [InlineData("05TMO,02LEV", "")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_list_of_practice_areas_for_the_given_employee_codes(string employeeCodes, string practiceAreaCodes)
        {
            //Arrange
            var mockPracticeAreaRepo = new Mock<IPracticeAreaRepository>();
            mockPracticeAreaRepo.Setup(x => x.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes))
                .ReturnsAsync(GetFakePracticeAreasWithEmployeeCodes());
            var sut = new PracticeAreaService(mockPracticeAreaRepo.Object);

            //Act
            var result = await sut.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes);

            if (result.Count() > 0)
            {
                Assert.Contains(result, emp => employeeCodes.Split(',').Contains(emp.EmployeeCode));
            }
        }

        [Theory]
        [InlineData("05TMO,02LEV", "11,6")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_list_of_employee_codes_with_given_practice_area_codes(string employeeCodes, string practiceAreaCodes)
        {
            //Arrange
            var mockPracticeAreaRepo = new Mock<IPracticeAreaRepository>();
            mockPracticeAreaRepo.Setup(x => x.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes))
                .ReturnsAsync(GetFakePracticeAreasWithEmployeeCodes());
            var sut = new PracticeAreaService(mockPracticeAreaRepo.Object);

            //Act
            var result = await sut.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes);

            if (result.Count() > 0)
            {
                Assert.Contains(result, emp => employeeCodes.Split(',').Contains(emp.EmployeeCode) && practiceAreaCodes.Split(',').Contains(emp.PracticeAreaCode));
            }
        }

        [Theory]
        [InlineData("", "")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_empty_object(string employeeCodes, string practiceAreaCodes)
        {
            //Arrange
            var mockPracticeAreaRepo = new Mock<IPracticeAreaRepository>();
            mockPracticeAreaRepo.Setup(x => x.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes))
                .ReturnsAsync(GetFakePracticeAreasWithEmployeeCodes());
            var sut = new PracticeAreaService(mockPracticeAreaRepo.Object);

            //Act
            var result = await sut.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes);
            result.Count().Should().Be(0);
        }

        [Theory]
        [InlineData("ABCD,1234", "")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_empty_object_for_invalid_employees(string employeeCodes, string practiceAreaCodes)
        {
            //Arrange
            var mockPracticeAreaRepo = new Mock<IPracticeAreaRepository>();
            mockPracticeAreaRepo.Setup(x => x.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes))
                .ReturnsAsync(GetFakeInvalidData());
            var sut = new PracticeAreaService(mockPracticeAreaRepo.Object);

            //Act
            var result = await sut.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCodes, practiceAreaCodes);
            result.Count().Should().Be(0);
        }

        private IEnumerable<PracticeAreaAffiliation> GetFakeInvalidData()
        {
            return Enumerable.Empty<PracticeAreaAffiliation>();
        }

        private IEnumerable<PracticeAreaAffiliation> GetFakePracticeAreasWithEmployeeCodes()
        {
            var practiceAreasWithEmployeeCodes = new List<PracticeAreaAffiliation>
            {
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "02LEV",
                    PracticeAreaCode = "6",
                    PracticeAreaName = "Technology & Cloud Services",
                    RoleCode = "10",
                    RoleName = "Connected"
                },
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "02LEV",
                    PracticeAreaCode = "11",
                    PracticeAreaName = "Performance Improvement",
                    RoleCode = "8",
                    RoleName = "Active Contributor - L1"
                },
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "02LEV",
                    PracticeAreaCode = "11",
                    PracticeAreaName = "Performance Improvement",
                    RoleCode = "15",
                    RoleName = "Practice Certified Expert"
                },
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "05TMO",
                    PracticeAreaCode = "2",
                    PracticeAreaName = "Consumer Products",
                    RoleCode = "11",
                    RoleName = "Reporting"
                },
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "05TMO",
                    PracticeAreaCode = "3",
                    PracticeAreaName = "Advanced Manufacturing & Services",
                    RoleCode = "11",
                    RoleName = "Reporting"
                },
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "05TMO",
                    PracticeAreaCode = "4",
                    PracticeAreaName = "Financial Services",
                    RoleCode = "11",
                    RoleName = "Reporting"
                },
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "05TMO",
                    PracticeAreaCode = "5",
                    PracticeAreaName = "Healthcare",
                    RoleCode = "11",
                    RoleName = "Reporting"
                },
                new PracticeAreaAffiliation
                {
                    EmployeeCode = "05TMO",
                    PracticeAreaCode = "6",
                    PracticeAreaName = "Technology & Cloud Services",
                    RoleCode = "11",
                    RoleName = "Reporting"
                },
            };

            return practiceAreasWithEmployeeCodes;
        }

        private IEnumerable<PracticeAreaAffiliation> GetFakePracticeAreas()
        {
            var practiceAreas = new List<PracticeAreaAffiliation>
            {
                new PracticeAreaAffiliation
                { 
                    PracticeAreaCode = "2",
                    PracticeAreaName = "Consumer Products"
                },
                new PracticeAreaAffiliation
                {
                    PracticeAreaCode = "3",
                    PracticeAreaName = "Advanced Manufacturing & Services"
                },
                new PracticeAreaAffiliation
                {
                    PracticeAreaCode = "4",
                    PracticeAreaName = "Financial Services"
                },
                new PracticeAreaAffiliation
                {
                    PracticeAreaCode = "5",
                    PracticeAreaName = "Healthcare"
                },
                new PracticeAreaAffiliation
                {
                    PracticeAreaCode = "6",
                    PracticeAreaName = "Technology & Cloud Services"
                },
                new PracticeAreaAffiliation
                {
                    PracticeAreaCode = "7",
                    PracticeAreaName = "Telecom / Media / Tech"
                },
                new PracticeAreaAffiliation
                {
                    PracticeAreaCode = "8",
                    PracticeAreaName = "Other Industries"
                },
            };

            return practiceAreas;
        }
    }
}
