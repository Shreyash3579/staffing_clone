using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class StaffingAnalyticsApiClient : IStaffingAnalyticsApiClient
    {
        private readonly HttpClient _apiClient;
        public StaffingAnalyticsApiClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("StaffingAnalyticsApiBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress); 
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
        }
        public async Task CreateAnalyticsReport(string scheduleIds)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync("api/analytics/analyticsReport", scheduleIds);
            if(!responseMessage.IsSuccessStatusCode)
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
        public async Task UpdateAnlayticsDataForDeletedCommitment(string commitmentIds)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync("api/analytics/UpdateAnlayticsDataForDeletedCommitment", commitmentIds);
            if (!responseMessage.IsSuccessStatusCode)
            {
                var parsedResult = responseMessage.Content?.ReadAsStringAsync().Result;
                throw new HttpRequestException(parsedResult);

            }
        }

        public void CreatePlaceholderAnalyticsReport(string placeholderScheduleIds)
        {
             _apiClient.PostAsJsonAsync("api/placeholderAnalytics/placeholderAnalyticsReport", placeholderScheduleIds);
            //var upsertedAnalyticsData = JsonConvert.DeserializeObject< IEnumerable<string>>(responseMessage.Content?.ReadAsStringAsync().Result)
            //                    ?? Enumerable.Empty<string>();
            //return upsertedAnalyticsData;
        }

        public async Task<Guid> DeleteAnalyticsDataForDeletedAllocationByScheduleId(Guid deletedAllocationId)
        {
            var responseMessage = await _apiClient.DeleteAsync($"api/analytics/DeleteAnalyticsDataByScheduleId?deletedAllocationId={deletedAllocationId.ToString()}");
            var deletedId = JsonConvert.DeserializeObject<Guid>(responseMessage.Content?.ReadAsStringAsync().Result);
            return deletedId;
        }
        public async Task DeleteAnalyticsDataForDeletedAllocationByScheduleIds(string deletedAllocationIds)
        {
            var responseMessage = await _apiClient.DeleteAsync($"api/analytics/DeleteAnalyticsDataByScheduleIds?deletedAllocationIds={deletedAllocationIds}");
            if (!responseMessage.IsSuccessStatusCode)
            {
                var parsedResult = responseMessage.Content?.ReadAsStringAsync().Result;
                throw new HttpRequestException(parsedResult);

            }
        }
        public void DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string deletedPlaceholderAllocationIds)
        {
            _apiClient.DeleteAsync($"api/placeholderAnalytics/deletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds?scheduleMasterPlaceholderIds={deletedPlaceholderAllocationIds}");
            //if (!responseMessage.IsSuccessStatusCode)
            //{
            //    var parsedResult = responseMessage.Content?.ReadAsStringAsync().Result;
            //    throw new HttpRequestException(parsedResult);

            //}
        }

        public async Task<List<ResourceAvailabilityViewModel>> UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(string employeeCodes)
        {
            var responseMessage = await _apiClient.PutAsJsonAsync("api/analytics/availabilityDataForResourcesWithNoAvailabilityRecords", employeeCodes);
            var upsertedAnalyticsData = JsonConvert.DeserializeObject<IEnumerable<ResourceAvailabilityViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<ResourceAvailabilityViewModel>();
            return upsertedAnalyticsData.ToList();
        }
        public async Task<string> UpdateCostAndAvailabilityDataByScheduleId(string scheduleIds)
        {
            var responseMessage = await _apiClient.PutAsJsonAsync("api/analytics/updateCostAndAvailabilityDataByScheduleId", scheduleIds);
            var updatedIds = JsonConvert.DeserializeObject<string>(responseMessage.Content?.ReadAsStringAsync().Result);
            return updatedIds;
        }
        public async Task<IEnumerable<ResourceAvailability>> UpdateCostForResourcesAvailableInFullCapacity(string employeeCodes)
        {
            var payload = new { listEmployeeCodes = employeeCodes };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/analytics/updateCostForResourcesAvailableInFullCapacity", payload);
            var upsertedAnalyticsData = JsonConvert.DeserializeObject<IEnumerable<ResourceAvailability>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<ResourceAvailability>();
            return upsertedAnalyticsData;
        }
    }
}
