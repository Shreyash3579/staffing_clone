using FluentAssertions;
using Newtonsoft.Json;
using Staffing.API.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Staffing.API.Tests.IntegrationTests.Controller
{
    [Collection("Staffing.API.Integration")]
    [Trait("IntegrationTest", "Staffing.API.PipelineChangesController")]
    public class PipelineChangesControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;
        public PipelineChangesControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [ClassData(typeof(PipelineChangesGenerator))]
        public async Task UpsertPipelineChanges_should_throw_error_if_pipelineId_is_null(CaseOppChanges pipelineChangesRequest)
        {
            // Arrange
            pipelineChangesRequest = GetPipelineChangesRequestWithNullPipelineId();

            //Act
            var response =
                await _testServer.Client.PutAsJsonAsync(
                    $"/api/pipelineChanges/upsertPipelineChanges", pipelineChangesRequest);

            response.IsSuccessStatusCode.Should().Equals(false);
        }

        [Theory]
        [ClassData(typeof(PipelineChangesGenerator))]
        public async Task UpsertPipelineChanges_should_save_and_return_the_same_pipeline_data(CaseOppChanges pipelineChangesRequest)
        {
            // Arrange
            pipelineChangesRequest = GetPipelineChangesRequest();

            //Act
            var response =
                await _testServer.Client.PutAsJsonAsync(
                    $"/api/pipelineChanges/upsertPipelineChanges", pipelineChangesRequest);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var pipelineChangesResponse = JsonConvert.DeserializeObject<CaseOppChanges>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.Equal(pipelineChangesRequest.StartDate, pipelineChangesResponse.StartDate);
            Assert.Equal(pipelineChangesRequest.EndDate, pipelineChangesResponse.EndDate);
            Assert.Equal(pipelineChangesRequest.ProbabilityPercent, pipelineChangesResponse.ProbabilityPercent);
            Assert.Equal(pipelineChangesRequest.PipelineId, pipelineChangesResponse.PipelineId);
            Assert.Equal(pipelineChangesRequest.LastUpdatedBy, pipelineChangesResponse.LastUpdatedBy);
        }

        [Theory]
        [ClassData(typeof(PipelineChangesGenerator))]
        public async Task UpsertPipelineChanges_should_not_update_probability_if_probability_is_null(CaseOppChanges pipelineChangesRequest)
        {
            // Arrange
            pipelineChangesRequest = GetPipelineChangesRequestWithNullProbability();

            //Act
            var response =
                await _testServer.Client.PutAsJsonAsync(
                    $"/api/pipelineChanges/upsertPipelineChanges", pipelineChangesRequest);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var pipelineChangesResponse = JsonConvert.DeserializeObject<CaseOppChanges>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Assert.Equal(pipelineChangesRequest.StartDate, pipelineChangesResponse.StartDate);
            Assert.Equal(pipelineChangesRequest.EndDate, pipelineChangesResponse.EndDate);

            if (pipelineChangesResponse.ProbabilityPercent != null)
            {
                pipelineChangesResponse.ProbabilityPercent.Should().BeGreaterOrEqualTo(0);
            }

            Assert.Equal(pipelineChangesRequest.PipelineId, pipelineChangesResponse.PipelineId);
            Assert.Equal(pipelineChangesRequest.LastUpdatedBy, pipelineChangesResponse.LastUpdatedBy);
        }

        [Theory]
        [ClassData(typeof(PipelineChangesGenerator))]
        public async Task UpsertPipelineChanges_should_not_update_start_end_date_if_start_end_date_is_null(CaseOppChanges pipelineChangesRequest)
        {
            // Arrange
            pipelineChangesRequest = GetPipelineChangesRequestWithNullStartEndDate();

            //Act
            var response =
                await _testServer.Client.PutAsJsonAsync(
                    $"/api/pipelineChanges/upsertPipelineChanges", pipelineChangesRequest);

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var pipelineChangesResponse = JsonConvert.DeserializeObject<CaseOppChanges>(responseString);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            if (pipelineChangesResponse.StartDate == null)
            {
                pipelineChangesResponse.EndDate.Should().BeNull();
            }

            if (pipelineChangesResponse.EndDate == null)
            {
                pipelineChangesResponse.StartDate.Should().BeNull();
            }

            if (pipelineChangesResponse.StartDate != null)
            {
                pipelineChangesResponse.EndDate.Should().NotBeNull();
            }

            if (pipelineChangesResponse.EndDate != null)
            {
                pipelineChangesResponse.StartDate.Should().NotBeNull();
            }

            Assert.Equal(pipelineChangesRequest.ProbabilityPercent, pipelineChangesResponse.ProbabilityPercent);
        }

        public CaseOppChanges GetPipelineChangesRequest()
        {
            return new CaseOppChanges
            {
                EndDate = Convert.ToDateTime("06-Oct-2020"),
                LastUpdatedBy = "51030",
                PipelineId = new Guid("85c48a81-c86a-4b13-a1cc-6dc788fd2476"),
                ProbabilityPercent = 80,
                StartDate = Convert.ToDateTime("07-Oct-2020"),
            };
        }

        public CaseOppChanges GetPipelineChangesRequestWithNullProbability()
        {
            return new CaseOppChanges
            {
                EndDate = Convert.ToDateTime("06-Oct-2020"),
                LastUpdatedBy = "51030",
                PipelineId = new Guid("85c48a81-c86a-4b13-a1cc-6dc788fd2476"),
                ProbabilityPercent = null,
                StartDate = Convert.ToDateTime("07-Oct-2020"),
            };
        }

        public CaseOppChanges GetPipelineChangesRequestWithNullPipelineId()
        {
            return new CaseOppChanges
            {
                EndDate = null,
                LastUpdatedBy = "51030",
                ProbabilityPercent = 70,
                StartDate = null,
            };
        }

        public CaseOppChanges GetPipelineChangesRequestWithNullStartEndDate()
        {
            return new CaseOppChanges
            {
                EndDate = null,
                LastUpdatedBy = "51030",
                PipelineId = new Guid("85c48a81-c86a-4b13-a1cc-6dc788fd2476"),
                ProbabilityPercent = 70,
                StartDate = null,
            };
        }

        public class PipelineChangesGenerator : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[]
                {
                    new CaseOppChanges
                    {
                        EndDate = Convert.ToDateTime("06-Oct-2020"),
                        LastUpdatedBy = "51030",
                        PipelineId = new Guid("85c48a81-c86a-4b13-a1cc-6dc788fd2476"),
                        ProbabilityPercent = 80,
                        StartDate = Convert.ToDateTime("07-Oct-2020")
                    }
                };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
