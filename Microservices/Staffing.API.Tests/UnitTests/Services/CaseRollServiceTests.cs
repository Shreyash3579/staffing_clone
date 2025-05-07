using FluentAssertions;
using Moq;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.UnitTests.Services
{
    [Trait("UnitTest", "Staffing.API.Services")]
    public class CaseRollServiceTests
    {
        [Theory]
        [InlineData("", "oldCaseCodes cannot be null or empty")]
        public async Task GetOfficeList_should_return_filterdOffices(string oldCaseCodes, string errorMessage)
        {
            //Arrange
            var mockCaseRollRepo = new Mock<ICaseRollRepository>();
            var sut = new CaseRollService(mockCaseRollRepo.Object);

            //Act
            var exception = await Record.ExceptionAsync(() => sut.GetCasesOnRollByCaseCodes(oldCaseCodes));

            //Assert
            exception?.Message.Should().Be(errorMessage);
            exception.Should().BeOfType<ArgumentException>();
        }
    }
}
