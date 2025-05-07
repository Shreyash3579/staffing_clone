using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class PipelineApiClient : IPipelineApiClient
    {
        private readonly HttpClient _apiClient;

        public PipelineApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:PipelineApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }
        public async Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesByPipelineIds(string pipelineIdList)
        {
            if (string.IsNullOrEmpty(pipelineIdList))
                return Enumerable.Empty<OpportunityViewModel>();
            var payload = new { pipelineIds = pipelineIdList };
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/opportunity/opportunitiesByPipelineIds", payload);
            var opportunities =
                JsonConvert.DeserializeObject<IEnumerable<OpportunityViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);
            return opportunities;
        }

        public async Task<IEnumerable<OpportunityFlatViewModel>> GetOpportunitiesFlatData(DateTime? lastUpdated)
        {
            var responseMessage =
                await _apiClient.GetAsync($"api/opportunity/opportunitiesFlatData?lastUpdated={lastUpdated}");
            
            var oppotuntiesFlatData =
                JsonConvert.DeserializeObject<IEnumerable<OpportunityFlatViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? new List<OpportunityFlatViewModel>();

            return oppotuntiesFlatData;
        }
    }
}
