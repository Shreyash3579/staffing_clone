using FluentAssertions;
using Hcpd.API.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Xunit;

namespace Hcpd.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Hcpd.API.AdvisorController")]
    public class AdvisorControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public AdvisorControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }

        [Theory]
        [InlineData("41TBR")]
        public async Task GetAdvisorByEmployeeCode_should_return_advisor_employee_code(string employeeCode)
        {
            //var response =
            //    await _testServer.Client.GetAsync($"/api/Advisor/advisorByEmployeeCode?employeeCode={employeeCode}");
            //response.EnsureSuccessStatusCode();
            //var responseString = await response.Content.ReadAsStringAsync();
            //var employeeWithAdvisor = JsonConvert.DeserializeObject<Advisor>(responseString);
            //employeeWithAdvisor.EmployeeCode.Should().Be(employeeCode);
            //employeeWithAdvisor.AdvisorEmployeeCode.Should().NotBe(null);
        }

        [Theory]
        [InlineData("12345")]
        public async Task GetAdvisorByEmployeeCode_should_return_null_for_no_employee_in_hcpd(string employeeCode)
        {
            //var response =
            //    await _testServer.Client.GetAsync($"/api/Advisor/advisorByEmployeeCode?employeeCode={employeeCode}");
            //response.EnsureSuccessStatusCode();
            //var responseString = await response.Content.ReadAsStringAsync();
            //var employeeWithAdvisor = JsonConvert.DeserializeObject<Advisor>(responseString);
            //employeeWithAdvisor.Should().Be(null);
        }
    }
}
