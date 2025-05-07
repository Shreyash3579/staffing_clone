using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Vacation.API.Models;
using Vacation.API.Core.Helpers;
using Vacation.API.Contracts.Services;

namespace Vacation.API.Core.Services
{
    public class StaffingApiClient : IStaffingApiClient
    {
        private readonly HttpClient _apiClient;

        public StaffingApiClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("StaffingApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task InsertAzureSearchQueryLog(AzureSearchQueryLog searchQueryLog)
        {
            //Update Planning Card in DB
            var responseMessage =
               await _apiClient.PutAsJsonAsync($"api/azureSearchQueryLog", searchQueryLog);

            var upsertQueryLogStatus = JsonConvert.DeserializeObject<AzureSearchQueryLog>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return;
        }

    }
}