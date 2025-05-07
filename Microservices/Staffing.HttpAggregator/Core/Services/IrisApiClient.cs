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
    public class IrisApiClient : IIrisApiClient
    {
        private readonly HttpClient _apiClient;

        public IrisApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("IrisApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<IEnumerable<PracticeArea>> GetAllCapabilityPracticeArea()
        {
            var responseMessage = await _apiClient.GetAsync($"api/practiceArea/capabilityPracticeAreaLookup");
            var practiceAreas = JsonConvert.DeserializeObject<IEnumerable<PracticeArea>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<PracticeArea>();
            return practiceAreas;
        }
        public async Task<IEnumerable<PracticeArea>> GetAllIndustryPracticeArea()
        {
            var responseMessage = await _apiClient.GetAsync($"api/practiceArea/industryPracticeAreaLookup");
            var practiceAreas = JsonConvert.DeserializeObject<IEnumerable<PracticeArea>>(responseMessage.Content?.ReadAsStringAsync().Result)
                ?? Enumerable.Empty<PracticeArea>();
            return practiceAreas;
        }
    }
}
