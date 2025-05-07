using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class PipelineApiClient : IPipelineApiClient
    {
        private readonly HttpClient _apiClient;

        public PipelineApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("PipelineApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<IList<OpportunityData>> GetOpportunitiesByOfficesActiveInDateRange(string officeCodes,
            DateTime startDate, DateTime endDate, string opportunityStatusTypeCodes, string clientCodes)
        {
            var responseMessage = await _apiClient.GetAsync(
                $"api/opportunity/opportunitiesByOffices?officeCodes={officeCodes}&startDate={startDate}&endDate={endDate}&opportunityStatusTypeCodes={opportunityStatusTypeCodes}&clientCodes={clientCodes}");
            var opportunities =
                JsonConvert.DeserializeObject<IEnumerable<OpportunityData>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new List<OpportunityData>();
            return opportunities.ToList();
        }

        public async Task<OpportunityDetails> GetOpportunityDetailsByPipelineId(Guid pipelineId)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/opportunity/opportunityDetails?pipelineId={pipelineId.ToString()}");
            var opportunity =
                JsonConvert.DeserializeObject<OpportunityDetails>(responseMessage.Content?.ReadAsStringAsync()
                    .Result) ?? new OpportunityDetails();
            return opportunity;
        }

        public async Task<IList<OpportunityData>> GetOpportunitiesByPipelineIds(string pipelineIds, string officeCodes = null,
            string opportunityStatusTypeCodes = null)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return new List<OpportunityData>();
            var payload = new { pipelineIds, officeCodes, opportunityStatusTypeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/opportunity/opportunitiesByPipelineIds", payload);
            var opportunities =
                JsonConvert.DeserializeObject<IEnumerable<OpportunityData>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);
            return opportunities.ToList();
        }

        public async Task<IList<OpportunityData>> GetOpportunitiesWithTaxonomiesByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return new List<OpportunityData>();
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/opportunity/opportunitiesWithTaxonomiesByPipelineIds", pipelineIds);
            var opportunities =
                JsonConvert.DeserializeObject<IEnumerable<OpportunityData>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);
            return opportunities.ToList();
        }

        public async Task<IEnumerable<OpportunityData>> GetOpportunitiesForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString))
                return new List<OpportunityData>();

            var url = $"api/opportunity/typeaheadOpportunities?searchString={searchString}";
            var responseMessage = await _apiClient.GetAsync(url);
            var opportunities =
                JsonConvert.DeserializeObject<IEnumerable<OpportunityData>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);
            return opportunities ?? new List<OpportunityData>();
        }
    }
}