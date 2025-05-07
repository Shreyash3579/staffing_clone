using Newtonsoft.Json;
using Staffing.AzureServiceBus.Contracts.Services;
using Staffing.AzureServiceBus.Core.Helpers;
using Staffing.AzureServiceBus.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Core.Services
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

        public async Task<IList<PlanningCard>> GetPlanningCardByPegOpportunityIds(string pegOpportunityIds)
        {
            var responseMessage =
               await _apiClient.GetAsync($"api/planningCard/getPlanningCardByPegOpportunityIds?pegOpportunityIds={pegOpportunityIds}");
            var opportunityThatExistsInBOSS = JsonConvert.DeserializeObject<IList<PlanningCard>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return opportunityThatExistsInBOSS;
        }

        public async Task<PlanningCard> UpsertPlanningCard(PlanningCard convertedPlanningCard)
        {
            //InsertPlanningCard in DB
            var responseMessage =
               await _apiClient.PostAsJsonAsync($"api/planningCard/upsertPlanningCard", convertedPlanningCard);

            var insertedPlanningCard = JsonConvert.DeserializeObject<PlanningCard>(responseMessage.Content
                    ?.ReadAsStringAsync().Result);

            return insertedPlanningCard;
        }

        public async Task DeletePlanningCard(Guid? planningCardId, string lastUpdatedBy)
        {
            var deletedPlanningCards = 
                await _apiClient.DeleteAsync($"api/planningCard?id={planningCardId}&lastUpdatedBy={lastUpdatedBy}");
            return;
        }

        public async Task<PricingSku> UpsertPricingSKU(PricingSku pricingSku)
        {
            //InsertPlanningCard in DB
            var responseMessage =
               await _apiClient.PostAsJsonAsync($"api/CortexSku/upsertPricingSKU", pricingSku);

            var insertedPlanningCard = JsonConvert.DeserializeObject<IEnumerable<PricingSku>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result).FirstOrDefault();

            return insertedPlanningCard;
        }

        public async Task<PricingSkuViewModel> UpsertPricingSkuDataLog(IEnumerable<PricingSkuViewModel> pricingSkuData)
        {
            //InsertPlanningCard in DB
            var responseMessage =
               await _apiClient.PostAsJsonAsync($"api/CortexSku/upsertPricingSkuDataLog", pricingSkuData);

            var upsertedPricingSkuDataLogs = JsonConvert.DeserializeObject<IEnumerable<PricingSkuViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result).FirstOrDefault();

            return upsertedPricingSkuDataLogs;
        }
    }
}