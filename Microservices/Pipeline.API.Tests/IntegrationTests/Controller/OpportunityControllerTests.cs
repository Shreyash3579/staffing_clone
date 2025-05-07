using FluentAssertions;
using Newtonsoft.Json;
using Pipeline.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Pipeline.API.Tests.IntegrationTests.Controller
{
    [Trait("IntegrationTest", "Pipeline.API.OpportunityController")]
    public class OpportunityControllerTests : IClassFixture<TestServerHost>
    {
        private readonly TestServerHost _testServer;

        public OpportunityControllerTests(TestServerHost testServer)
        {
            _testServer = testServer;
        }

        [Theory]
        [InlineData("110,112", 1, 20, "0,1,2,3,4,5")] // 110 -> Boston
        public async Task GetOpportunitiesByOffices_should_return_opportunities(
            string officeCodes, int offsetStartIndex, int pageSize, string opportunityStatusTypeCodes)
        {
            //set input variables here as inline data is not usable after some time
            string startDate = DateTime.Today.ToShortDateString();
            string endDate = DateTime.Today.AddDays(14).ToShortDateString();
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/opportunity/opportunitiesByOffices?startDate={startDate}&endDate={endDate}" +
                    $"&officeCodes={officeCodes}&offsetStartIndex={offsetStartIndex}&pageSize={pageSize}&" +
                    $"opportunityStatusTypeCodes={opportunityStatusTypeCodes}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var actualOpportunities = JsonConvert.DeserializeObject<IEnumerable<OpportunityDetailsViewModel>>(responseString)
                .ToList();
            var expectedOrderedOpportunities = actualOpportunities.OrderByDescending(x => x.StartDate);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            actualOpportunities.Count().Should().BeGreaterOrEqualTo(0);
            actualOpportunities.ForEach(c => c.PipelineId.Should().NotBeEmpty());
            actualOpportunities.ForEach(c => c.OpportunityName.Should().NotBeNullOrEmpty());
            actualOpportunities.ForEach(c => c.ClientName.Should().NotBeNullOrEmpty());
            actualOpportunities.ForEach(c => c.StartDate.Should().NotBeBefore(Convert.ToDateTime(startDate)));
            actualOpportunities.ForEach(c => c.StartDate.Should().NotBeAfter(Convert.ToDateTime(endDate)));
            expectedOrderedOpportunities.SequenceEqual(actualOpportunities).Should().BeTrue(); //test ordering of elements
        }

        [Theory(Skip = "[FromBody] not works")]
        [InlineData("56843017-7236-4699-B953-2231A989C6CD,d6a81109-0183-4aaf-ab01-27d21b361d32")]
        public async Task GetOpportunitiesWithTaxonomiesByPipelineIds_should_return_opportunities(string pipelineIds)
        {
            //Act
            var response =
                await _testServer.Client.PostAsync(
                    $"/api/opportunity/opportunitiesWithTaxonomiesByPipelineIds?pipelineIdList", new StringContent(pipelineIds));

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var opportunitiesList = JsonConvert.DeserializeObject<IEnumerable<OpportunityDetailsViewModel>>(responseString).ToList();

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            foreach (var opportunity in opportunitiesList)
            {
                opportunity.PipelineId.Should().NotBeEmpty();
                opportunity.CoordinatingPartnerCode.Should().NotBeNullOrEmpty();
                opportunity.OpportunityName.Should().NotBeNullOrEmpty();
                opportunity.OpportunityStatus.Should().NotBeNullOrEmpty();
                opportunity.ClientCode.Should().NotBe(0);
                opportunity.ClientName.Should().NotBeNullOrEmpty();
            }
        }

        [Theory]
        [InlineData("godrej")]
        public async Task GetOpportunitiesForTypeahead_should_return_opportunities(string searchString)
        {
            //Act
            var response =
                await _testServer.Client.GetAsync(
                    $"/api/opportunity/typeaheadOpportunities?searchString={searchString}");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            var opportunitiesList = JsonConvert.DeserializeObject<IEnumerable<OpportunityDetailsViewModel>>(responseString).ToList();
            var expectedOrder = opportunitiesList.OrderByDescending(t => t.StartDate);

            //Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            opportunitiesList.Count().Should().BeGreaterOrEqualTo(0);
            opportunitiesList.Count().Should().BeLessOrEqualTo(50);
            opportunitiesList.ForEach(o => o.PipelineId.Should().NotBeEmpty());
            opportunitiesList.ForEach(o => o.OpportunityName.Should().NotBeNullOrEmpty());
            opportunitiesList.ForEach(o => o.ClientCode.Should().NotBe(0));
            opportunitiesList.ForEach(o => o.ClientName.Should().NotBeNullOrEmpty());
            opportunitiesList.ForEach(o => o.CoordinatingPartnerCode.Should().NotBeNullOrEmpty());
            expectedOrder.SequenceEqual(opportunitiesList).Should().BeTrue();

        }
    }
}