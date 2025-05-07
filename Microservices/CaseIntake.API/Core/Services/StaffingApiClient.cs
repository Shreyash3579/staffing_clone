using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CaseIntake.API.Models;
using CaseIntake.API.Core.Helpers;


namespace CaseIntake.API.Contracts.Services
{
    public class StaffingApiClient : IStaffingApiClient
    {
        private readonly HttpClient _apiClient;

        public StaffingApiClient(HttpClient httpClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:StaffingApiBaseUrl");
            _apiClient = httpClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
        }

        public async Task<IEnumerable<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return new List<PlanningCard>();

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/planningCard/planningCardsByPlanningCardIds", planningCardIds);

            var planningCards =
                JsonConvert.DeserializeObject<IEnumerable<PlanningCard>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<PlanningCard>();

            return planningCards;
        }

    }
}