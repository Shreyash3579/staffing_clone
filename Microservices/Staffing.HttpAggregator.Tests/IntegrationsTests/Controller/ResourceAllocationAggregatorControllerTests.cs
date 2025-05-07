using FluentAssertions;
using Newtonsoft.Json;
using Staffing.HttpAggregator.Tests.IntegrationTests;
using Staffing.HttpAggregator.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.HttpAggregator.Tests.IntegrationsTests.Controller
{
    public class ResourceAllocationAggregatorControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public ResourceAllocationAggregatorControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("K3NE")]
        public async Task GetCaseRoleAllocationsByOldCaseCodes_should_return_response_with_ok_status(string oldCaseCodes)
        {
            // Arrange
            var payload = new { oldCaseCodes };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByOldCaseCodes", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("")]
        public async Task GetCaseRoleAllocationsByOldCaseCodes_should_return_ok_response_with_zero_allocations(string oldCaseCodes)
        {
            // Arrange
            var payload = new { oldCaseCodes };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByOldCaseCodes", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceAllocations = JsonConvert.DeserializeObject<IEnumerable<CaseRoleAllocationViewModel>>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            resourceAllocations.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("K3NE")]
        public async Task GetCaseRoleAllocationsByOldCaseCodes_should_either_return_no_allocations_or_retrun_allocations_for_the_given_oldCaseCodes(string oldCaseCodes)
        {
            // Arrange
            var payload = new { oldCaseCodes };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByOldCaseCodes", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceAllocations = JsonConvert.DeserializeObject<IEnumerable<CaseRoleAllocationViewModel>>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (resourceAllocations.Count() > 0)
            {
                resourceAllocations.ToList().All(x => x.OldCaseCode == oldCaseCodes);
            }
        }

        [Theory]
        [InlineData("K3NE,A2FJ")]
        public async Task GetCaseRoleAllocationsByOldCaseCodes_should_allocations_for_multiple_oldCaseCodes(string oldCaseCodes)
        {
            // Arrange
            var payload = new { oldCaseCodes };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByOldCaseCodes", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceAllocations = JsonConvert.DeserializeObject<IEnumerable<CaseRoleAllocationViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (resourceAllocations.Count() > 0)
            {
                var groupedAllocations = resourceAllocations.GroupBy(x => x.OldCaseCode).Select(y => new { oldCaseCode = y.Key, allocations = y.ToList() });
                groupedAllocations.Count().Should().BeLessOrEqualTo(oldCaseCodes.Split(',').Count());
            }
        }

        [Theory]
        [InlineData("eaad4f85-1b87-4691-ace2-77cd81379b31")]
        public async Task GetCaseRoleAllocationsByPipelineIds_should_return_response_with_ok_status(string pipelineIds)
        {
            // Arrange
            var payload = new { pipelineIds };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByPipelineIds", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("")]
        public async Task GetCaseRoleAllocationsByPipelineIds_should_return_ok_response_with_zero_allocations(string pipelineIds)
        {
            // Arrange
            var payload = new { pipelineIds };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByPipelineIds", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("eaad4f85-1b87-4691-ace2-77cd81379b31")]
        public async Task GetCaseRoleAllocationsByPipelineIds_should_either_return_no_allocations_or_retrun_allocations_for_the_given_pipelineIds(string pipelineIds)
        {
            // Arrange
            var payload = new { pipelineIds };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByPipelineIds", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceAllocations = JsonConvert.DeserializeObject<IEnumerable<CaseRoleAllocationViewModel>>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (resourceAllocations.Count() > 0)
            {
                resourceAllocations.ToList().All(x => x.PipelineId == pipelineIds);
            }
        }

        [Theory]
        [InlineData("eaad4f85-1b87-4691-ace2-77cd81379b31,768d58d3-03a5-4e5e-90ed-21387d1a243d")]
        public async Task GetCaseRoleAllocationsByPipelineIds_should_allocations_for_multiple_pipelineIds(string pipelineIds)
        {
            // Arrange
            var payload = new { pipelineIds };

            //Act
            var response =
                await _testServer.Client.PostAsJsonAsync(
                    $"/api/resourceAllocationAggregator/getCaseRoleAllocationsByPipelineIds", payload);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var resourceAllocations = JsonConvert.DeserializeObject<IEnumerable<CaseRoleAllocationViewModel>>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            if (resourceAllocations.Count() > 0)
            {
                var groupedAllocations = resourceAllocations.GroupBy(x => x.PipelineId).Select(y => new { pipelineId = y.Key, allocations = y.ToList() });
                groupedAllocations.Count().Should().BeLessOrEqualTo(pipelineIds.Split(',').Count());
            }
        }
    }
}
