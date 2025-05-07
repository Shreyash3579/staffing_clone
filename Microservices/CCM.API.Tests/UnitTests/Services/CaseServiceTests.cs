using CCM.API.Contracts.RepositoryInterfaces;
using CCM.API.Core.Services;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CCM.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "CCM.API.Services")]
    public class CaseServiceTests
    {

        [Theory]
        [InlineData("2019-01-10", "2019-01-01", "110,112", "1,2,3",
            "endDate should be greater than or equal to startDate")]
        [InlineData("2019-01-01", "2019-01-10", null, "1,2,3",
            "Office Code can not be null")]
        [InlineData("2019-01-01", "2019-01-10", "110,112", null,
            "Case Type Code can not be null")]// 110 -> Boston, 112 -> NewYork
        public async Task GetNewDemandCasesByOffices_should_return_ArgumentException(DateTime startDate, DateTime endDate,
            string officeCodes, string caseTypeCodes, string errorMessage)
        {
            //Arrange
            var mockCaseRepo = new Mock<ICaseRepository>();

            var sut = new CaseService(mockCaseRepo.Object);
            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetNewDemandCasesByOffices(
                Convert.ToDateTime(startDate),
                Convert.ToDateTime(endDate),
                officeCodes,
                caseTypeCodes));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("2019-01-10", "2019-01-01", "110,112", "1,2,3",
            "endDate should be greater than or equal to startDate")]
        [InlineData("2019-01-01", "2019-01-10", null, "1,2,3",
            "Office Code can not be null")]
        [InlineData("2019-01-01", "2019-01-10", "110,112", null,
            "Case Type Code can not be null")]// 110 -> Boston, 112 -> NewYork
        public async Task GetActiveCasesExceptNewDemandsByOffices_should_return_ArgumentException(DateTime startDate, DateTime endDate,
            string officeCodes, string caseTypeCodes, string errorMessage)
        {
            //Arrange
            var mockCaseRepo = new Mock<ICaseRepository>();

            var sut = new CaseService(mockCaseRepo.Object);
            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetActiveCasesExceptNewDemandsByOffices(
                Convert.ToDateTime(startDate),
                Convert.ToDateTime(endDate),
                officeCodes,
                caseTypeCodes));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

        [Theory]
        [InlineData("Case Code cannot be empty")]
        public async Task GetCaseDetailsByCaseCodes_should_return_ArgumentException(string errorMessage)
        {
            //Arrange
            var mockCaseRepo = new Mock<ICaseRepository>();

            var sut = new CaseService(mockCaseRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetCaseDataByCaseCodes(null));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }

    }
}
