using Basis.API.ViewModels;
using FluentAssertions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Basis.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Basis.API.PracticeAreaController")]
    public class PracticeAreaControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public PracticeAreaControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }

        [Fact]
        public async Task GetAllPracticeArea_should_return_all_practice_areas()
        {
            //Act
            var response =
                await _testServer.Client.GetAsync("/api/practiceArea/getAllPracticeArea");
            
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var practiceAreas = JsonConvert.DeserializeObject<IEnumerable<PracticeAreaViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            practiceAreas.Count().Should().BeGreaterOrEqualTo(0);
            practiceAreas.ForEach(c => c.PracticeAreaCode.Should().NotBeNullOrEmpty());
            practiceAreas.ForEach(c => c.PracticeAreaName.Should().NotBeNullOrEmpty());
        }

        [Theory]
        [InlineData("40789,42AUM,05TMO,02LEV")]
        [InlineData("51030,50286")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_employees_with_practice_areas_if_any(string employeeCodes)
        {
            var payload = new
            {
                listEmployeeCodes = employeeCodes
            };
            var json = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            //Act
            var response = await _testServer.Client.PostAsync("/api/practiceArea/getAffiliationsByEmployeeCodesAndPracticeAreaCodes", stringContent);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var practiceAreasWithEmployeeCodes = JsonConvert.DeserializeObject<IEnumerable<EmployeePracticeAreaViewModel>>(responseString).ToList();

            if (practiceAreasWithEmployeeCodes.Count() > 0)
            {
                Assert.Contains(practiceAreasWithEmployeeCodes, emp => employeeCodes.Split(',').Contains(emp.EmployeeCode));
            }
        }

        [Theory]
        [InlineData("")]
        [InlineData("abcd,33333")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_empty_response(string employeeCodes)
        {
            var payload = new
            {
                listEmployeeCodes = employeeCodes
            };
            var json = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            //Act
            var response = await _testServer.Client.PostAsync("/api/practiceArea/getAffiliationsByEmployeeCodesAndPracticeAreaCodes", stringContent);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var practiceAreasWithEmployeeCodes = JsonConvert.DeserializeObject<IEnumerable<EmployeePracticeAreaViewModel>>(responseString).ToList();

            practiceAreasWithEmployeeCodes.Count().Should().Be(0);
        }

        [Theory]
        [InlineData("02LEV,41192,39209,37995,45088", "6,11")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_employees_with_given_practice_area_codes(string employeeCodes, string practiceAreaCodes)
        {
            var payload = new
            {
                listEmployeeCodes = employeeCodes,
                practiceAreaCodes
            };
            var json = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            //Act
            var response = await _testServer.Client.PostAsync("/api/practiceArea/getAffiliationsByEmployeeCodesAndPracticeAreaCodes", stringContent);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var practiceAreasWithEmployeeCodes = JsonConvert.DeserializeObject<IEnumerable<EmployeePracticeAreaViewModel>>(responseString).ToList();

            if (practiceAreasWithEmployeeCodes.Count() > 0)
            {
                Assert.Contains(practiceAreasWithEmployeeCodes, 
                    emp => employeeCodes.Split(',').Contains(emp.EmployeeCode)
                    && practiceAreaCodes.Split(',').Contains(emp.PracticeAreaCode)
                    );
            }
        }

        [Theory]
        [InlineData("02LEV,41192,39209,37995,45088", "123,456")]
        public async Task GetPracticeAreaAffiliationsByEmployeeCodes_should_return_employees_empty_response_for_invalid_practice_area_codes(string employeeCodes, string practiceAreaCodes)
        {
            var payload = new
            {
                listEmployeeCodes = employeeCodes,
                practiceAreaCodes
            };
            var json = JsonConvert.SerializeObject(payload);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            //Act
            var response = await _testServer.Client.PostAsync("/api/practiceArea/getAffiliationsByEmployeeCodesAndPracticeAreaCodes", stringContent);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var practiceAreasWithEmployeeCodes = JsonConvert.DeserializeObject<IEnumerable<EmployeePracticeAreaViewModel>>(responseString).ToList();

            practiceAreasWithEmployeeCodes.Count().Should().Be(0);
        }
    }
}
