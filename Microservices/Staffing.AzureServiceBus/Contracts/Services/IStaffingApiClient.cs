using Staffing.AzureServiceBus.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Contracts.Services
{
    public interface IStaffingApiClient
    {
        Task<IList<PlanningCard>> GetPlanningCardByPegOpportunityIds(string pegOpportunityIds);
        Task<PlanningCard> UpsertPlanningCard(PlanningCard planningCard);
        Task DeletePlanningCard(Guid? planningCardId, string lastUpdatedBy);
        Task<PricingSku> UpsertPricingSKU(PricingSku pricingSku);
        Task<PricingSkuViewModel> UpsertPricingSkuDataLog(IEnumerable<PricingSkuViewModel> pricingSkuData);
    }
}