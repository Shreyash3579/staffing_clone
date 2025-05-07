using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.PlanningCardController")]
    public class PlanningCardControllerTests : IClassFixture<TestServerHost>, IDisposable
    {
        private readonly TestServerHost _testServer;
        private Guid insertedPlanningCardId;
        private string lastUpdatedBy = "";

        public PlanningCardControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(insertedPlanningCardId.ToString()) && !string.IsNullOrEmpty(lastUpdatedBy))
            {
                Task.Run(async () => await
                    _testServer.Client.DeleteAsync($"/api/planningCard?id={insertedPlanningCardId}&lastUpdatedBy={lastUpdatedBy}")).Wait();
            }
        }

        [Theory]
        [ClassData(typeof(PlanningCardTestDataGenerator))]
        public async Task InsertPlanningCard_should_insert_planningCard(
            PlanningCard testPlanningCard)
        {
            //ACT
            var response = await _testServer.Client.PostAsync("/api/planningCard",
                new StringContent(JsonConvert.SerializeObject(testPlanningCard), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var insertedData = JsonConvert.DeserializeObject<PlanningCard>(responseString);

            insertedPlanningCardId = (Guid)insertedData.Id;
            lastUpdatedBy = insertedData.LastUpdatedBy;

            //ASSERT
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            insertedData.Id.Should().Be(insertedPlanningCardId);
            insertedData.StartDate.Value.Date.Should().Be(testPlanningCard.StartDate.Value.Date);
            insertedData.EndDate.Value.Date.Should().Be(testPlanningCard.EndDate.Value.Date);
            insertedData.CreatedBy.Should().Be(testPlanningCard.CreatedBy);
            insertedData.LastUpdatedBy.Should().Be(testPlanningCard.LastUpdatedBy);
        }

        [Theory]
        [ClassData(typeof(PlanningCardTestDataGenerator))]
        public async Task UpdatePlanningCard_should_Update_planningCard(
            PlanningCard testPlanningCard)
        {
            //ARRANGE
            var response = await _testServer.Client.PostAsync("/api/planningCard",
                new StringContent(JsonConvert.SerializeObject(testPlanningCard), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var insertedData = JsonConvert.DeserializeObject<PlanningCard>(responseString);

            insertedPlanningCardId = (Guid)insertedData.Id;

            //ACT
            var planningCardUpdate = insertedData.Clone();
            planningCardUpdate.Name = "TestPlanUpdate";
            planningCardUpdate.EndDate = DateTime.Now.AddDays(150);
            planningCardUpdate.LastUpdatedBy = "39209";
            var response2 = await _testServer.Client.PutAsync("/api/planningCard",
                new StringContent(JsonConvert.SerializeObject(planningCardUpdate), Encoding.Default,
                    "application/json"));
            response2.EnsureSuccessStatusCode();

            var response2String = await response2.Content.ReadAsStringAsync();
            var updatedData = JsonConvert.DeserializeObject<PlanningCard>(response2String);

            insertedPlanningCardId = (Guid)updatedData.Id;
            lastUpdatedBy = updatedData.LastUpdatedBy;

            //ASSERT
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            updatedData.Id.Should().Be(insertedPlanningCardId);
            updatedData.StartDate.Value.Date.Should().Be(planningCardUpdate.StartDate.Value.Date);
            updatedData.EndDate.Value.Date.Should().Be(planningCardUpdate.EndDate.Value.Date);
            updatedData.LastUpdatedBy.Should().Be(planningCardUpdate.LastUpdatedBy);
        }

        [Theory]
        [ClassData(typeof(PlanningCardTestDataGenerator))]
        public async Task GetPlanningCardAndItsAllocationsByEmployeeCode_should_return_planningCard(
            PlanningCard testPlanningCard)
        {
            //ARRANGE
            var response = await _testServer.Client.PostAsync("/api/planningCard",
                new StringContent(JsonConvert.SerializeObject(testPlanningCard), Encoding.Default,
                    "application/json"));
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var insertedData = JsonConvert.DeserializeObject<PlanningCard>(responseString);

            insertedPlanningCardId = (Guid)insertedData.Id;
            lastUpdatedBy = insertedData.LastUpdatedBy;
            var employeeCode = insertedData.CreatedBy;

            //ACT            
            var response2 = await _testServer.Client.GetAsync($"/api/planningCard?employeeCode={employeeCode}");
            response2.EnsureSuccessStatusCode();

            var response2String = await response2.Content.ReadAsStringAsync();
            var savedPlanningCards = JsonConvert.DeserializeObject<IEnumerable<PlanningCard>>(response2String);

            //ASSERT
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
            savedPlanningCards.ToList().ForEach(savedPlanningCard =>
            {
                savedPlanningCard.Id.Should().NotBe(Guid.Empty);
                savedPlanningCard.CreatedBy.Should().Be(insertedData.CreatedBy);
                savedPlanningCard.LastUpdatedBy.Should().Be(insertedData.LastUpdatedBy);
                savedPlanningCard.allocations?.Count.Should().BeGreaterOrEqualTo(0);
            });
            
        }

    }

    public class PlanningCardTestDataGenerator : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                new PlanningCard
                {
                    Name = "TestPlan",
                    StartDate = DateTime.Now.AddDays(5),
                    EndDate= DateTime.Now.AddDays(130),
                    CreatedBy = "37995",
                    LastUpdatedBy ="37995"
                }
            };
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
