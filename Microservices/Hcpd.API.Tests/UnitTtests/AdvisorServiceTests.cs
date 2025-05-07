using FluentAssertions;
using Hcpd.API.Contracts.RepositoryInterfaces;
using Hcpd.API.Core.Services;
using Hcpd.API.Models;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Hcpd.API.Tests.UnitTtests
{
    [Trait("UnitTest", "Hcpd.API.Services")]
    public class AdvisorServiceTests
    {
        [Theory]
        [InlineData("41TBR")]
        public async Task GetAdvisorByEmployeeCode_should_return_employee_with_advisor(string employeeCode)
        {
            //Arrange
            var mockHcpdRepo = new Mock<IHcpdRepository>();
            mockHcpdRepo.Setup(x => x.GetAdvisorByEmployeeCode(employeeCode)).ReturnsAsync(GetFakeAdvisorWithEmploye());
            var sut = new AdvisorService(mockHcpdRepo.Object);

            //Act
            var result = (await sut.GetAdvisorByEmployeeCode(employeeCode));
            result.EmployeeCode.Should().Be(employeeCode);
            result.AdvisorEmployeeCode.Should().NotBe(null);
            result.AdvisorEmployeeCode.Length.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData("")]
        public async Task GetAdvisorByEmployeeCode_should_return_empty_object(string employeeCode)
        {
            //Arrange
            var mockHcpdRepo = new Mock<IHcpdRepository>();
            mockHcpdRepo.Setup(x => x.GetAdvisorByEmployeeCode(employeeCode)).ReturnsAsync(GetEmptyAdvisor());
            var sut = new AdvisorService(mockHcpdRepo.Object);

            //Act
            var result = (await sut.GetAdvisorByEmployeeCode(employeeCode));
            result.Should().Be(null);
        }

        private Advisor GetFakeAdvisorWithEmploye()
        {
            return new Advisor
            {
                EmployeeCode = "41TBR",
                AdvisorEmployeeCode = "38CRR"
            };
        }

        private Advisor GetEmptyAdvisor()
        {
            Advisor advisor = null;
            return advisor;
        }
    }
}
