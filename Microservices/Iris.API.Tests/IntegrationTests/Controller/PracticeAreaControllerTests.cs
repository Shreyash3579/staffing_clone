using FluentAssertions;
using Iris.API.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Iris.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Iris.API.PracticeAreaController")]
    public class PracticeAreaControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public PracticeAreaControllerTests(TestServerHost testServerHost)
        {
            _testServer = testServerHost;
        }

        [Fact]
        public async Task GetAllIndustryPracticeArea_should_return_lookupData()
        {
            //Act
            var response = await _testServer.Client.GetAsync($"/api/practiceArea/industryPracticeAreaLookup");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var practiceAreas = JsonConvert.DeserializeObject<IEnumerable<PracticeArea>>(responseString).ToList();

            // ASSERT
            practiceAreas.ForEach(x => x.PracticeAreaCode.Should().NotBeNullOrEmpty());
            practiceAreas.ForEach(x => x.PracticeAreaName.Should().NotBeNullOrEmpty());

        }

        [Fact]
        public async Task GetAllCapabilityPracticeArea_should_return_lookupData()
        {
            //Act
            var response = await _testServer.Client.GetAsync($"/api/practiceArea/capabilityPracticeAreaLookup");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var practiceAreas = JsonConvert.DeserializeObject<IEnumerable<PracticeArea>>(responseString).ToList();

            // ASSERT
            practiceAreas.ForEach(x => x.PracticeAreaCode.Should().NotBeNullOrEmpty());
            practiceAreas.ForEach(x => x.PracticeAreaName.Should().NotBeNullOrEmpty());

        }
    }
}
