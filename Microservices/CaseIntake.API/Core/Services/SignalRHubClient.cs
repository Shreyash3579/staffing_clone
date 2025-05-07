using System.Net.Http;
using System.Threading.Tasks;
using System;
using CaseIntake.API.Core.Helpers;


namespace CaseIntake.API.Contracts.Services
{
    public class SignalRHubClient : ISignalRHubClient
    {
        private readonly HttpClient _apiClient;

        public SignalRHubClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:SignalRHubBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<string> GetUpdateOnCaseIntakeChanges(string sharedEmployeeCodes)
        {
            var responseMessage =
               await _apiClient.PostAsJsonAsync($"api/SignalR/getUpdateOnCaseIntakeChanges", sharedEmployeeCodes);

            var responseData = responseMessage.Content?.ReadAsStringAsync().Result;

            return responseData;
        }

    }

}
