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
    public class StaffingAnalyticsApiClient: IStaffingAnalyticsApiClient
    {
        private readonly HttpClient _apiClient;

        public StaffingAnalyticsApiClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:StaffingAnalyticsApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(60);
        }

        public async Task<IEnumerable<ResourceAllocation>> UpdateCostForResourcesAvailableInFullCapacity(string listEmployeeCodes)
        {
            var payload = new { listEmployeeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/analytics/updateCostForResourcesAvailableInFullCapacity", payload);

            var result = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocation>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<ResourceAllocation>();

            return result;
        }

        public async Task<List<ResourceAvailabilityViewModel>> InsertDailyAvailabilityTillNextYearForAll(string employeeCodes)
        {
            var payload = new { employeeCodes };
            var responseMessage = await _apiClient.PutAsJsonAsync("api/analytics/insertDailyAvailabilityTillNextYearForAll", payload);

            var result = JsonConvert.DeserializeObject<IEnumerable<ResourceAvailabilityViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<ResourceAvailabilityViewModel>();

            return result.ToList();
        }

        public async Task UpsertAvailabilityData(string listEmployeeCodes)
        {
            var payload = new { listEmployeeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/analytics/upsertAvailabilityData", payload);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var parsedResult = responseMessage.Content?.ReadAsStringAsync().Result;
                throw new HttpRequestException(parsedResult);

            }
        }
        public async Task UpsertCapacityAnalysisDaily(bool? fullLoad, DateTime? loadAfterLastUpdated)
        {
            var payload = new { fullLoad, loadAfterLastUpdated };
            var responseMessage = await _apiClient.PutAsJsonAsync("api/analytics/UpsertCapacityAnalysisDaily", payload);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var parsedResult = responseMessage.Content?.ReadAsStringAsync().Result;
                throw new HttpRequestException(parsedResult);

            }
        }
        public async Task UpsertCapacityAnalysisMonthly(bool? fullLoad)
        {
            var responseMessage = await _apiClient.PutAsJsonAsync($"api/analytics/UpsertCapacityAnalysisMonthly", fullLoad);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var parsedResult = responseMessage.Content?.ReadAsStringAsync().Result;
                throw new HttpRequestException(parsedResult);

            }
        }
        public async Task UpdateAnlayticsDataForUpsertedCommitment(string commitmentIds)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync("api/analytics/UpdateAnlayticsDataForUpsertedCommitment", commitmentIds);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var parsedResult = responseMessage.Content?.ReadAsStringAsync().Result;
                throw new HttpRequestException(parsedResult);

            }
        }
    }
}
