using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;

namespace Staffing.HttpAggregator.Core.Services
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

        public async Task<string> GetUpdateOnSharedNotes(string sharedEmployeeCodes)
        {
            var responseMessage =
               await _apiClient.PostAsJsonAsync($"api/SignalR/getUpdateOnSharedNotes", sharedEmployeeCodes);

            var responseData = responseMessage.Content?.ReadAsStringAsync().Result;

            return responseData;
        }

        public async Task<string> GetUpdateOnRingfenceCommitmentsAlert(string employeeCodes)
        {
            var responseMessage =
               await _apiClient.PostAsJsonAsync($"api/SignalR/getUpdateOnRingfenceCommitment", employeeCodes);

            var responseData = responseMessage.Content?.ReadAsStringAsync().Result;

            return responseData;
        }

    }

}
