using Newtonsoft.Json;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class AzureServiceBusApiClient : IAzureServiceBusApiClient
    {
        private readonly HttpClient _apiClient;

        public AzureServiceBusApiClient(HttpClient apiClient)
        {
            _apiClient = apiClient;
            var endpointAddress = ConfigurationUtility.GetValue("AzureServiceBusApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<bool> SendToPegQueue(IEnumerable<PegOpportunityMap> pegOpportunityMap)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync($"api/pegQueue/sendToPegQueue", pegOpportunityMap);

            var isSuccessfullyAddedToPegQueue = JsonConvert.DeserializeObject<bool>(responseMessage.Content?.ReadAsStringAsync().Result);

            return isSuccessfullyAddedToPegQueue;
        }
    }
}
