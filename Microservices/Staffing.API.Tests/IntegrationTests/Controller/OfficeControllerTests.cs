using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.OfficeController")]
    public class OfficeControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public OfficeControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Fact]
        public async Task GetOfficeList_should_return_listofActiveOffice()
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/office/offices");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualActiveCases = JsonConvert.DeserializeObject<IEnumerable<Office>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualActiveCases.Count().Should().BeGreaterOrEqualTo(0);
            actualActiveCases.ForEach(c => c.OfficeCode.Should().BeGreaterOrEqualTo(1));
            actualActiveCases.ForEach(c => c.OfficeName.Should().NotBeNullOrEmpty());
            actualActiveCases.ForEach(c => c.OfficeAbbreviation.Should().NotBeNullOrEmpty());
        }
    }
}
