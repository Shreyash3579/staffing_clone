using Newtonsoft.Json;
using Staffing.AzureServiceBus.Contracts.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Staffing.AzureServiceBus.Core.Helpers;
using Staffing.AzureServiceBus.Models;

namespace Staffing.AzureServiceBus.Core.Services
{
    public class SignalRHubClient : ISignalRHubClient
    {
        private readonly HttpClient _apiClient;

        public SignalRHubClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("SignalRHubBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<string> UpdateSignalRHub(PegOpportunity pegData )
        {
            var responseMessage =
               await _apiClient.PostAsJsonAsync($"api/SignalR/onPegDataReceivedFromServiceBus", pegData);

            var updatedData = responseMessage.Content?.ReadAsStringAsync().Result;

            return updatedData;
        }
    }

}
