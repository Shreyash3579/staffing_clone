using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IPlanningCardService
    {
        Task<PlanningCard> InsertPlanningCard(PlanningCard planningCard);
        Task<PlanningCard> UpdatePlanningCard(PlanningCard planningCard);
        Task<PlanningCard> UpsertPlanningCard(PlanningCard planningCard);
        Task DeletePlanningCardAndItsAllocations(Guid id, string lastUpdatedBy);
        Task<IList<PlanningCard>> GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags, DateTime? startDate, DateTime? endDate, string bucketIds = null);
        Task<IList<PlanningCardAllocation>> GetPlanningCardAllocationsByEmployeeCodesAndDuration(string employeeCodes, DateTime startDate, DateTime endDate);
        Task<PlanningCard> SharePlanningCard(PlanningCard planningCard);
        Task<IList<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds);
        Task<IList<PlanningCard>> GetPlanningCardByPegOpportunityIds(string pegOpportunityIds);

        Task<IEnumerable<PlanningCard>> GetPlanningCardsForTypeahead(string searchString);
    }
}
