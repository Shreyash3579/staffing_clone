using Newtonsoft.Json;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class PipelineApiClient : IPipelineApiClient
    {
        private readonly HttpClient _apiClient;
        public PipelineApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("PipelineApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(20);

        }

        public async Task<IEnumerable<OpportunityDetailsViewModel>> GetOpportunityDetailsByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return new List<OpportunityDetailsViewModel>();

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/opportunity/opportunitiesDetails", pipelineIds);
            var opportunitiesDetails = JsonConvert.DeserializeObject<IEnumerable<OpportunityDetailsViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<OpportunityDetailsViewModel>()
                ;
            return opportunitiesDetails.ToList();
        }
    }
}
