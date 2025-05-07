using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class SharepointApiClient : ISharepointApiClient
    {
        private readonly HttpClient _apiClient;
        public SharepointApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:SharepointApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(10);
        }

        public async Task<IEnumerable<SMAPMission>> GetSMAPMissions()
        {
            var responseMessage = await _apiClient.GetAsync($"api/SMAPMissions/getSMAPMissions");
            var affiliations = JsonConvert.DeserializeObject<IEnumerable<SMAPMission>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<SMAPMission>();
            return affiliations.ToList();
        }

        public async Task<IEnumerable<StaffingPreference>> GetStaffingPreferences()
        {
            var responseMessage = await _apiClient.GetAsync($"api/staffingPreferences/getStaffingPreferences");
            var staffingPreferences = JsonConvert.DeserializeObject<IEnumerable<StaffingPreference>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<StaffingPreference>();
            return staffingPreferences.ToList();
        }
    }
}
