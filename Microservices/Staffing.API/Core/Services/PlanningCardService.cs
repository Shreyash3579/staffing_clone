using Hangfire;
using Staffing.API.Contracts.RepositoryInterfaces;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.API.Core.Services
{
    public class PlanningCardService : IPlanningCardService
    {
        private readonly IPlanningCardRepository _planningCardRepository;
        private readonly IScheduleMasterPlaceholderService _placeholderService;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public PlanningCardService(IPlanningCardRepository planningCardRepository, IScheduleMasterPlaceholderService placeholderService, IBackgroundJobClient backgroundJobClient)
        {
            _planningCardRepository = planningCardRepository;
            _placeholderService = placeholderService;
            _backgroundJobClient = backgroundJobClient;
        }
        public async Task DeletePlanningCardAndItsAllocations(Guid id, string lastUpdatedBy)
        {
            var allocationsIds = await _planningCardRepository.DeletePlanningCardAndItsAllocations(id, lastUpdatedBy);
            var placeholderScheduleIds = string.Join(",", allocationsIds.Distinct());
            _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(placeholderScheduleIds));

        }
        public async Task<IList<PlanningCard>> GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags, DateTime? startDate, DateTime? endDate, string bucketIds = null)
        {
            return await _planningCardRepository.GetPlanningCardAndItsAllocationsByEmployeeCodeAndFilters(employeeCode, officeCodes, staffingTags, startDate, endDate, bucketIds);
        }
        public async Task<IList<PlanningCardAllocation>> GetPlanningCardAllocationsByEmployeeCodesAndDuration(string employeeCodes, DateTime startDate, DateTime endDate)
        {
            return await _planningCardRepository.GetPlanningCardAllocationsByEmployeeCodesAndDuration(employeeCodes, startDate, endDate);
        }

        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        public async Task<PlanningCard> InsertPlanningCard(PlanningCard planningCard)
        {
            return await _planningCardRepository.InsertPlanningCard(planningCard);
        }

        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        public async Task<PlanningCard> UpdatePlanningCard(PlanningCard planningCard)
        {
            var planningCardDataBeforeUpdation = await GetPlanningCardByPlanningCardIds(planningCard.Id.ToString());
            var updatedPlanningCard = await _planningCardRepository.UpdatePlanningCard(planningCard);

            IEnumerable<ScheduleMasterPlaceholder> allocations = Enumerable.Empty<ScheduleMasterPlaceholder>();

            if (updatedPlanningCard.StartDate != null && updatedPlanningCard.EndDate != null && isPlanningCardDateChanged(planningCardDataBeforeUpdation[0], updatedPlanningCard))
            {
                try
                {
                    allocations = await _placeholderService.GetAllocationsByPlanningCardIds(planningCard.Id.ToString());


                    if (allocations.Any())
                    {
                        foreach (var allocation in allocations)
                        {
                            allocation.StartDate = DateTime.Parse(updatedPlanningCard.StartDate);
                            allocation.EndDate = DateTime.Parse(updatedPlanningCard.EndDate);
                        }
                        await _placeholderService.UpsertPlaceholderAllocation(allocations);
                    }
                }
                catch (Exception e)
                {
                    await _planningCardRepository.UpdatePlanningCard(planningCardDataBeforeUpdation[0]);
                    throw;
                }
            }
            return updatedPlanningCard;
        }

        public async Task<PlanningCard> UpsertPlanningCard(PlanningCard planningCard)
        {
            string planningCardId = planningCard.Id.ToString();
            _ = CreateAndDeletePlaceholderAnalytic(planningCard, planningCardId);

            var existingPlanningCard = await GetExistingPlanningCard(planningCard.Id);
            var upsertedPlanningCard = await _planningCardRepository.UpsertPlanningCard(planningCard);
            var allocations = await UpdateAllocationsOnPlanningCardUpdates(existingPlanningCard, upsertedPlanningCard);

            return ConvertToPlanningCardAndAllocationModel(upsertedPlanningCard, allocations);
        }

        private async Task<PlanningCard> GetExistingPlanningCard(Guid? planningCardId)
        {
            if (planningCardId == null)
            {
                return null;
            }

            var planningCards = await GetPlanningCardByPlanningCardIds(planningCardId.ToString());
            return planningCards.FirstOrDefault();
        }

        private async Task<IEnumerable<ScheduleMasterPlaceholder>> UpdateAllocationsOnPlanningCardUpdates(PlanningCard existingPlanningCard, PlanningCard updatedPlanningCard)
        {
            var allocations = await _placeholderService.GetAllocationsByPlanningCardIds(updatedPlanningCard.Id.ToString());
            if (allocations.Any() && existingPlanningCard != null && isPlanningCardDateChanged(existingPlanningCard, updatedPlanningCard))
            {

                try
                {
                    allocations = await UpdateAllocationDates(allocations, updatedPlanningCard);
                }
                catch (Exception)
                {
                    await RollbackToPreviousState(updatedPlanningCard);

                }

            }
            return allocations;
        }



        private async Task<IEnumerable<ScheduleMasterPlaceholder>> UpdateAllocationDates(IEnumerable<ScheduleMasterPlaceholder> allocations, PlanningCard updatedPlanningCard)
        {
            foreach (var allocation in allocations)
            {
                allocation.StartDate = DateTime.Parse(updatedPlanningCard.StartDate);
                allocation.EndDate = DateTime.Parse(updatedPlanningCard.EndDate);
            }
            return await _placeholderService.UpsertPlaceholderAllocation(allocations);

        }

        private async Task RollbackToPreviousState(PlanningCard previousPlanningCard)
        {
            await _planningCardRepository.UpsertPlanningCard(previousPlanningCard);
        }


        private PlanningCard ConvertToPlanningCardAndAllocationModel(PlanningCard planningCard,
            IEnumerable<ScheduleMasterPlaceholder> allocations)
        {

            var allocationsOnPlanningCard = allocations.Where(x => x.PlanningCardId == planningCard.Id)?.ToList();
            foreach (var allocation in allocationsOnPlanningCard)
            {
                allocation.IncludeInCapacityReporting = planningCard.IncludeInCapacityReporting;
                allocation.IsPlanningCardShared = planningCard.IsShared;
            }

            planningCard.allocations = allocationsOnPlanningCard;

            return planningCard;
        }
        bool isPlanningCardDateChanged(PlanningCard original, PlanningCard updated)
        {
            if ((original.StartDate == null && original.EndDate == null) && (updated.StartDate != null || updated.EndDate != null))
            {
                return true;
            }
            DateTime originalStartDate = DateTime.Parse(original.StartDate);
            DateTime originalEndDate = DateTime.Parse(original.EndDate);

            DateTime updatedStartDate = DateTime.Parse(updated.StartDate);
            DateTime updatedEndDate = DateTime.Parse(updated.EndDate);

            return originalStartDate.Date != updatedStartDate.Date || originalEndDate.Date != updatedEndDate.Date;
        }


        //ToDo: Remove this endpoint as moved to UpsertPlanningCard By : 26-March-2024
        public async Task<PlanningCard> SharePlanningCard(PlanningCard planningCard)
        {
            string planningCardId = planningCard.Id.ToString();

            _ = CreateAndDeletePlaceholderAnalytic(planningCard, planningCardId);

            var sharePlanningCard = await _planningCardRepository.SharePlanningCard(planningCard);

            return sharePlanningCard;

        }

        private async Task CreateAndDeletePlaceholderAnalytic(PlanningCard planningCard, string planningCardId)
        {
            var allocations = await _placeholderService.GetAllocationsByPlanningCardIds(planningCardId);

            var placeholderAllocationsToIncludeForPlanningCards = allocations.Where(x => string.IsNullOrEmpty(x.OldCaseCode) && !string.IsNullOrEmpty(x.EmployeeCode) && !(bool)x.IsPlaceholderAllocation);

            var placeholderScheduleIds = string.Join(",", placeholderAllocationsToIncludeForPlanningCards.Select(x => x.Id).Distinct());

            if (planningCard.IncludeInCapacityReporting != null && (bool)planningCard.IncludeInCapacityReporting)
            {
                _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.CreatePlaceholderAnalyticsReport(placeholderScheduleIds));
            }
            else
            {
                _backgroundJobClient.Enqueue<IStaffingAnalyticsApiClient>(x => x.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(placeholderScheduleIds));
            }
        }

        public async Task<IList<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<PlanningCard>().ToList();
            return await _planningCardRepository.GetPlanningCardByPlanningCardIds(planningCardIds);
        }

        public async Task<IList<PlanningCard>> GetPlanningCardByPegOpportunityIds(string pegOpportunityIds)
        {
            if (string.IsNullOrEmpty(pegOpportunityIds))
                throw new ArgumentNullException(pegOpportunityIds, "At least one pegOpportunityId is required.");

            var planningCardData = await _planningCardRepository.GetPlanningCardByPegOpportunityIds(pegOpportunityIds);

            return planningCardData;

        }

        public async Task<IEnumerable<PlanningCard>> GetPlanningCardsForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
            {
                return Enumerable.Empty<PlanningCard>();
            }

            var planningCards = await
                _planningCardRepository.GetPlanningCardsForTypeahead(searchString);

            return planningCards;
        }
    }
}
