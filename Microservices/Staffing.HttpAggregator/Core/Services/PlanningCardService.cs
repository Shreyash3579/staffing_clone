using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class PlanningCardService : IPlanningCardService
    {
        private readonly IResourceAllocationService _resourceAllocationService;
        private readonly IResourcePlaceholderAllocationService _resourcePlaceholderAllocationService;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IAzureServiceBusApiClient _azureServiceBusApiClient;
        public PlanningCardService(IResourceAllocationService resourceAllocationService, 
            IResourcePlaceholderAllocationService resourcePlaceholderAllocationService,
            IStaffingApiClient staffingApiClient, IAzureServiceBusApiClient azureServiceBusApiClient)
        {
            _resourceAllocationService = resourceAllocationService;
            _resourcePlaceholderAllocationService = resourcePlaceholderAllocationService;
            _staffingApiClient = staffingApiClient;
            _azureServiceBusApiClient = azureServiceBusApiClient;
        }
        public async Task<bool> MergePlanningCard(PlanningCard mergedPlanningCard, IEnumerable<ResourceAssignmentViewModel> resourceAllocations, IEnumerable<ResourceAssignmentViewModel> placeholderAllocations)
        {
            var mergeStatus = false;
            IEnumerable<CaseOppChanges> getCaseOppChangesData = null;
            if (mergedPlanningCard.PegOpportunityId != null)
            {
                var firstAllocation = resourceAllocations?.FirstOrDefault() ?? placeholderAllocations?.FirstOrDefault();
                var oldCaseCode = firstAllocation?.OldCaseCode;
                getCaseOppChangesData = await _staffingApiClient.GetCaseChangesByOldCaseCodes(oldCaseCode);  

                if (getCaseOppChangesData == null || !getCaseOppChangesData.Any())
                {
                        // Create a new CaseOppChanges object and populate it
                        var newCaseOppChange = new CaseOppChanges
                        {
                            OldCaseCode = firstAllocation.OldCaseCode,
                            PegOpportunityId = mergedPlanningCard.PegOpportunityId,
                            StartDate = firstAllocation.CaseStartDate,
                            EndDate = firstAllocation.CaseEndDate,
                            LastUpdatedBy = firstAllocation.LastUpdatedBy
                        };

                        getCaseOppChangesData = new List<CaseOppChanges> { newCaseOppChange };
                }
                else
                {
                    getCaseOppChangesData = getCaseOppChangesData.Select(x =>
                    {
                        x.PegOpportunityId = mergedPlanningCard.PegOpportunityId;
                        return x;
                    }).ToList();

                }

            }

            Task updatedCaseOppChangesTask = Task.CompletedTask;
            if (getCaseOppChangesData != null && getCaseOppChangesData.Any())
            {
                updatedCaseOppChangesTask = _staffingApiClient.UpsertCaseChanges(getCaseOppChangesData.FirstOrDefault());
            }

            var resourceAllocationTask = _resourceAllocationService.UpsertResourceAllocations(resourceAllocations);
            var resourcePlaceholderAllocationTask = _resourcePlaceholderAllocationService.UpsertPlaceholderAllocations(placeholderAllocations);
            var updatePlanningCardTask = _staffingApiClient.UpdatePlanningCard(mergedPlanningCard);


            
            await Task.WhenAll(updatedCaseOppChangesTask, resourceAllocationTask, resourcePlaceholderAllocationTask, updatePlanningCardTask);

            if(updatePlanningCardTask.IsCompletedSuccessfully && !string.IsNullOrEmpty(mergedPlanningCard.PegOpportunityId))
            {
                var pegOpportunityMap = new PegOpportunityMap
                {
                    OpportunityId = mergedPlanningCard.PegOpportunityId,
                    OldCaseCode = mergedPlanningCard.MergedCaseCode,
                    LastUpdated = DateTime.Now
                };

                var pegOpportunityMapAsArray = new List<PegOpportunityMap>
                {
                    pegOpportunityMap
                };

                var isSuccessfullyAddedToPegQueue = await _azureServiceBusApiClient.SendToPegQueue(pegOpportunityMapAsArray);
                
                //If success from Service Bus then process
                if (isSuccessfullyAddedToPegQueue)
                {
                    mergedPlanningCard.IsSyncedWithPeg = true;
                    await Task.WhenAll(_staffingApiClient.UpdatePlanningCard(mergedPlanningCard) ,_staffingApiClient.DeletePlanningCardAndItsAllocations((Guid)mergedPlanningCard.Id, mergedPlanningCard.LastUpdatedBy));
                }

                mergeStatus = true;
            }
            else
            {
                await _staffingApiClient.DeletePlanningCardAndItsAllocations((Guid)mergedPlanningCard.Id, mergedPlanningCard.LastUpdatedBy);
            }


            return mergeStatus;
        }

        public async Task<IList<PlanningCard>> GetPlanningCardsWithNotes(IEnumerable<PlanningCard> planningCards, string employeeCode)
        {
            if (planningCards == null) 
            { 
                return new List<PlanningCard>(); 
            }
            var lstPlanningCardIds = string.Join(',', planningCards?.Select(x => x.Id.ToString()));


            var casePlanningViewNotes = Enumerable.Empty<CaseViewNote>();
            if (!string.IsNullOrEmpty(employeeCode))
            {
                casePlanningViewNotes = await _staffingApiClient.GetLatestCaseViewNotes(null,null,lstPlanningCardIds, employeeCode);

            }

            var planningCardsWithNotes = ConvertPlanningCardToViewModelWithNotes(planningCards, casePlanningViewNotes);

            return planningCardsWithNotes;

        }

        private IList<PlanningCard> ConvertPlanningCardToViewModelWithNotes(IEnumerable<PlanningCard> planningCards, IEnumerable<CaseViewNote> caseViewNotes)
        {
            var planningCardWithNotes = planningCards.Select(item => new PlanningCard
            {
                Id = item.Id,
                Name = item.Name,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                IsShared = item.IsShared,
                SharedOfficeCodes = item.SharedOfficeCodes,
                SharedStaffingTags = item.SharedStaffingTags,
                CreatedBy = item.CreatedBy,
                PegOpportunityId = item.PegOpportunityId,
                MergedCaseCode = item.MergedCaseCode,
                IsMerged = item.IsMerged,
                IsSyncedWithPeg = item.IsSyncedWithPeg,
                LastUpdatedBy = item.LastUpdatedBy,
                SkuTerm = item.SkuTerm,
                allocations = item.allocations,
                CasePlanningViewNotes = caseViewNotes == null ? null : caseViewNotes.Where(x => x.PlanningCardId == item.Id)
            }).ToList();

            return planningCardWithNotes;
        }
    }
}
