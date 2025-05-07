using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class AzureSearchApiClient : IAzureSearchApiClient
    {
        private readonly HttpClient _apiClient;

        public AzureSearchApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AzureSearchApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<BossSearchResult> GetResourcesBySearchString(BossSearchCriteria bossSearchCriteria)
        {
            var payload = new { bossSearchCriteria };

            var responseMessage =
                await _apiClient.PostAsJsonAsync($"api/search/resourcesBySearchString", bossSearchCriteria);

            var auditData =
                JsonConvert.DeserializeObject<BossSearchResult>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new BossSearchResult();

            return auditData;
        }
    }
}