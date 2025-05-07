using Dapper;
using Staffing.API.Contracts.Helpers;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Core.Helpers;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Repository
{
    public class PlanningCardRepository : IPlanningCardRepository
    {
        private readonly IBaseRepository<PlanningCard> _baseRepository;

        public PlanningCardRepository(IBaseRepository<PlanningCard> baseRepository, IDapperContext context)
        {
            _baseRepository = baseRepository;
            _baseRepository.Context = context;
        }
        public async Task<IEnumerable<string>> DeletePlanningCardAndItsAllocations(Guid id, string lastUpdatedBy)
        {

            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
               StoredProcedureMap.DeletePlanningCardAndItsAllocation,
               new
               {
                   id,
                   lastUpdatedBy
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var scheduleMasterIds = result.Read<Guid>().Select(id => id.ToString());

            return scheduleMasterIds;
        }

        public async Task<IList<PlanningCard>> GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags, DateTime? startDate, DateTime? endDate, string bucketIds = null)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
               StoredProcedureMap.GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters,
               new
               {
                   employeeCode,
                   officeCodes,
                   staffingTags,
                   startDate,
                   endDate,
                   bucketIds
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var planningCards = result.Read<PlanningCard>().ToList();
            var allocations = result.Read<ScheduleMasterPlaceholder>().ToList();
            var planningCardSkus = result.Read<SkuDemand>().ToList();
            if (!allocations.Any())
            {
                return planningCards;
            }

            var planningCardAndItsAllocation = MapPlanningcardWithItsAllocations(planningCards, allocations, planningCardSkus);
            return planningCardAndItsAllocation;
        }

        public async Task<IList<PlanningCardAllocation>> GetPlanningCardAllocationsByEmployeeCodesAndDuration(string employeeCodes, DateTime startDate, DateTime endDate)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
               StoredProcedureMap.GetPlanningCardAllocationsByEmployeeCodesAndDuration,
               new
               {
                   employeeCodes,
                   startDate,
                   endDate
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var planningCardAllocations = result.Read<PlanningCardAllocation>().ToList();
            return planningCardAllocations;
        }

        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        public async Task<PlanningCard> InsertPlanningCard(PlanningCard planningCard)
        {
            var insertedPlanningCard = await
                _baseRepository.InsertAsync(
                    new
                    {
                        planningCard.Name,
                        planningCard.StartDate,
                        planningCard.EndDate,
                        planningCard.ProbabilityPercent,
                        planningCard.CreatedBy,
                        planningCard.LastUpdatedBy,
                        planningCard.IsShared,
                        planningCard.SharedOfficeCodes,
                        planningCard.SharedStaffingTags,
                        planningCard.PegOpportunityId
                    },
                    StoredProcedureMap.InsertPlanningCard
                );

            insertedPlanningCard.allocations = new List<ScheduleMasterPlaceholder>();

            return insertedPlanningCard;
        }

        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        public async Task<PlanningCard> UpdatePlanningCard(PlanningCard planningCard)
        {
            var updatedPlanningCard = await
               _baseRepository.UpdateAsync(
                   new
                   {
                       planningCard.Id,
                       planningCard.Name,
                       planningCard.StartDate,
                       planningCard.EndDate,
                       planningCard.MergedCaseCode,
                       planningCard.IsMerged,
                       planningCard.IsSyncedWithPeg,
                       planningCard.ProbabilityPercent,
                       planningCard.LastUpdatedBy,
                       planningCard.SharedOfficeCodes,
                       planningCard.SharedStaffingTags,
                       planningCard.IsShared
                   },
                   StoredProcedureMap.UpdatePlanningCard
               );

            return updatedPlanningCard;
        }

        public async Task<PlanningCard> UpsertPlanningCard(PlanningCard planningCard)
        {
            var updatedPlanningCard = await
               _baseRepository.UpdateAsync(
                   new
                   {
                       planningCard.Id,
                       planningCard.Name,
                       planningCard.StartDate,
                       planningCard.EndDate,
                       planningCard.MergedCaseCode,
                       planningCard.IsMerged,
                       planningCard.IsSyncedWithPeg,
                       planningCard.ProbabilityPercent,
                       planningCard.CreatedBy,
                       planningCard.LastUpdatedBy,
                       planningCard.SharedOfficeCodes,
                       planningCard.SharedStaffingTags,
                       planningCard.IsShared,
                       planningCard.IncludeInCapacityReporting,
                       planningCard.PegOpportunityId,
                       planningCard.Source,
                       planningCard.SourceLastUpdated
                   },
                   StoredProcedureMap.UpsertPlanningCard
               );

            return updatedPlanningCard;
        }


        private IList<PlanningCard> MapPlanningcardWithItsAllocations(IList<PlanningCard> planningCards,
            IList<ScheduleMasterPlaceholder> allocations, IList<SkuDemand> planningCardSkus)
        {
            foreach (var planningCard in planningCards)
            {
                planningCard.allocations = allocations.Where(x => x.PlanningCardId == planningCard.Id)?.ToList();
                planningCard.SkuTerm = planningCardSkus.Where(x => x.PlanningCardId == planningCard.Id);

            }

            return planningCards;
        }


        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        public async Task<PlanningCard> SharePlanningCard(PlanningCard planningCard)
        {
            var sharedPlanningCard = await
               _baseRepository.UpdateAsync(
                   new
                   {
                       planningCard.Id,
                       planningCard.IsShared,
                       planningCard.SharedOfficeCodes,
                       planningCard.SharedStaffingTags,
                       planningCard.IncludeInCapacityReporting,
                       planningCard.LastUpdatedBy
                   },
                   StoredProcedureMap.SharePlanningCard
               );

            return sharedPlanningCard;
        }

        public async Task<IList<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds)
        {
            var planningCards = await _baseRepository.GetAllAsync(
                new { planningCardIds = planningCardIds },
                StoredProcedureMap.GetPlanningCardByPlanningCardIds);

            return planningCards.ToList();
        }

        public async Task<IList<PlanningCard>> GetPlanningCardByPegOpportunityIds(string pegOpportunityIds)
        {
            var planningCardsWithPegOpportunity = await _baseRepository.GetAllAsync(
                new { pegOpportunityIds = pegOpportunityIds },
                StoredProcedureMap.GetPlanningCardByPegOpportunityIds);

            return planningCardsWithPegOpportunity.ToList();
        }

        public async Task<IEnumerable<PlanningCard>> GetPlanningCardsForTypeahead(string searchString)
        {
            var planningCards = await _baseRepository.GetAllAsync(new { searchString }, StoredProcedureMap.GetPlanningCardsForTypeahead);
            return planningCards;
        }
    }
}
