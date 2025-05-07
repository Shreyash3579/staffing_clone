using Microservices.Common.Core.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Graph.Models;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OpportunityService(IPipelineApiClient pipelineApiClient, IStaffingApiClient staffingApiClient,
            IResourceApiClient resourceApiClient, IHttpContextAccessor httpContextAccessor,
            ICCMApiClient ccmApiClient)
        {
            _pipelineApiClient = pipelineApiClient;
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _ccmApiClient = ccmApiClient;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<OpportunityData>> GetOpportunities(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated,
                    IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories,
                    IList<Revenue> revenueByServiceLinesOpps, IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, string lstPipelineIdsOfOppsUpdatedInBOSSInDateRange,
                    IEnumerable<Guid?> planningBoardPipelineIds, string loggedInUser = null)
        {
            if (!IsOpportuntiyFilterSet(filterObj))
                return Enumerable.Empty<OpportunityData>();

            Task<IList<OpportunityData>> opportunitiesTask;
            #region Pipeline API Calls

            if (filterObj.DemandTypes.Contains(Constants.DemandType.Opportunity))
            {
                opportunitiesTask = _pipelineApiClient.GetOpportunitiesByOfficesActiveInDateRange(filterObj.OfficeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.OpportunityStatusTypeCodes, filterObj.ClientCodes);
            }
            else
            {
                opportunitiesTask = Task.FromResult<IList<OpportunityData>>(new List<OpportunityData>());
            }

            var lstPipelineIdsFromSupply = (allocationsStaffedBySupply != null) ? string.Join(",", allocationsStaffedBySupply?.Select(x => x.PipelineId).Distinct()) : "";
            var lstPipelineidsFromSupplyAndPinned = (filterObj.OpportunityExceptionShowList + "," + lstPipelineIdsFromSupply).Trim(',');
            var disntinctPipelineidsFromSupplyAndPinned = string.Join(",", lstPipelineidsFromSupplyAndPinned.Split(",").Distinct());
            var pinnedOppsAndOppsStaffedBySupplyTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(disntinctPipelineidsFromSupplyAndPinned);
            var OppsUpdatedInBOSSInDateRangeTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(
                lstPipelineIdsOfOppsUpdatedInBOSSInDateRange.Trim(','), filterObj.OfficeCodes, filterObj.OpportunityStatusTypeCodes);

            await Task.WhenAll(opportunitiesTask, pinnedOppsAndOppsStaffedBySupplyTask, OppsUpdatedInBOSSInDateRangeTask);
            var unfilteredOpportunities = opportunitiesTask.Result;
            var pinnedOppsAndOppsStaffedBySupply = pinnedOppsAndOppsStaffedBySupplyTask.Result.ToList();
            var OppsUpdatedInBOSSInDateRange = OppsUpdatedInBOSSInDateRangeTask.Result.ToList();
            pinnedOppsAndOppsStaffedBySupply.AddRange(OppsUpdatedInBOSSInDateRange);
            var pinnedOppsAndOppsStaffedBySupplyAndOppsUpdatedInBOSSInDateRange = pinnedOppsAndOppsStaffedBySupply;

            #endregion

            var opportunities = GetOpportunitiesIncludingStaffedBySupplyAndUserPreferences(filterObj, unfilteredOpportunities, pinnedOppsAndOppsStaffedBySupplyAndOppsUpdatedInBOSSInDateRange,
                lstPipelineIdsFromSupply, lstPipelineIdsOfOppsUpdatedInBOSSInDateRange);

            #region Staffing API calls

            var listPipelineId = string.Join(",", opportunities.Select(x => x.PipelineId.ToString()).ToList());

            var opportunitiesSkuTermsDataTask =
                _staffingApiClient.GetSKUTermForProjects(null, listPipelineId, null);
            var allocatedResourcesDataTask = _staffingApiClient.GetResourceAllocationsByPipelineIds(listPipelineId);
            var placeholderAllocationDataTask = _staffingApiClient.GetPlaceholderAllocationsByPipelineIds(listPipelineId);
            //get pricing team size here
            var oppDataWithPricingTeamSizeTask = _staffingApiClient.GetOppCortexPlaceholderInfoByPipelineIds(listPipelineId.ToString());
            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(listPipelineId);
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            Task<IEnumerable<CaseViewNote>> casePlanningViewNotesTask;
            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotesTask = _staffingApiClient.GetCaseViewNotesByPipelineIds(listPipelineId, loggedInUser);
            }
            else
            {
                casePlanningViewNotesTask = Task.FromResult<IEnumerable<CaseViewNote>>(new List<CaseViewNote>());
            }

            await
               Task.WhenAll(opportunitiesSkuTermsDataTask, allocatedResourcesDataTask, caseAttributeLookupListTask, pipelineChangesTask,
               placeholderAllocationDataTask, commitmentTypeListTask, casePlanningViewNotesTask, oppDataWithPricingTeamSizeTask);

            var opportunitiesSkuTerms = opportunitiesSkuTermsDataTask.Result;
            var allocatedResources = allocatedResourcesDataTask.Result;
            var caseAttributeLookupList = caseAttributeLookupListTask.Result;
            var pipelineChanges = pipelineChangesTask.Result;
            var placeholderAllocations = placeholderAllocationDataTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;
            var casePlanningViewNotes = casePlanningViewNotesTask.Result;
            var oppDataWithPricingTeamSize = oppDataWithPricingTeamSizeTask.Result;
            #endregion

            pipelineChanges.Join(opportunities, (pipelineChange) => pipelineChange.PipelineId, (opportunity) => opportunity.PipelineId, (pipelineChange, opportunity) =>
            {
                opportunity.isStartDateUpdatedInBOSS = pipelineChange.StartDate.HasValue && pipelineChange.StartDate != DateTime.MinValue && opportunity.StartDate != pipelineChange.StartDate;
                opportunity.isEndDateUpdatedInBOSS = pipelineChange.EndDate.HasValue && pipelineChange.EndDate != DateTime.MinValue && opportunity.EndDate != pipelineChange.EndDate;
                opportunity.OriginalStartDate = opportunity.StartDate;
                opportunity.OverrideStartDate = pipelineChange.StartDate;
                opportunity.StartDate = pipelineChange.StartDate ?? opportunity.StartDate;
                opportunity.OriginalEndDate = opportunity.EndDate;
                opportunity.OverrideEndDate = pipelineChange.EndDate;
                opportunity.EndDate = pipelineChange.EndDate ?? opportunity.EndDate;
                opportunity.OriginalProbabilityPercent = opportunity.ProbabilityPercent;
                opportunity.OverrideProbabilityPercent = pipelineChange.ProbabilityPercent;
                opportunity.ProbabilityPercent = pipelineChange.ProbabilityPercent ?? opportunity.ProbabilityPercent;
                opportunity.Notes = pipelineChange.Notes;
                opportunity.CaseServedByRingfence = pipelineChange.CaseServedByRingfence;
                opportunity.StaffingOfficeCode = pipelineChange.StaffingOfficeCode;

                return opportunity;
            }).ToList();


            var employees = employeesIncludingTerminated.ToList();

            opportunities = ConvertToOpportunityDataModel(opportunities, allocatedResources, placeholderAllocations, null,
                skuTermLookup, caseAttributeLookupList, employees, offices, investmentCategories, caseRoleTypeList, commitmentTypeList, casePlanningViewNotes, opportunitiesSkuTerms, oppDataWithPricingTeamSize);

            var pinnedOpportunities = opportunities.Where(c => (bool)filterObj.OpportunityExceptionShowList?.Contains(c.PipelineId.ToString())).ToList();
            var opportunitiesStaffedBySupply = opportunities.Where(c => (bool)lstPipelineIdsFromSupply?.Contains(c.PipelineId.ToString())).ToList();

            // Show opp that are pinned or staffedBySupply irrespective of demand filters
            opportunities = opportunities.Except(pinnedOpportunities).ToList();
            opportunities = opportunities.Except(opportunitiesStaffedBySupply).ToList();

            // Remove the opps that were updated in BOSS and now the start date of the opp is not in the date range
            opportunities = opportunities
                .Where(x => x.StartDate >= filterObj.StartDate && x.StartDate <= filterObj.EndDate).ToList();

            opportunities = FilterOpportunitiesByPracticeArea(filterObj, opportunities).ToList();
            opportunities = FilterOpportunitiesByProbabilityPercent(filterObj, opportunities);

            opportunities = (List<OpportunityData>)await FilterOpportunitiesByCaseAttributes(filterObj, opportunities, revenueByServiceLinesOpps);


            opportunities = opportunities.Concat(opportunitiesStaffedBySupply).ToList();

            opportunities = FilterOpportunitiesByStaffFromSupplyFilter(filterObj, planningBoardPipelineIds, opportunities).ToList();

            opportunities = opportunities.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();
            opportunities.InsertRange(0, pinnedOpportunities);

            return opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First());
        }

        public async Task<IEnumerable<OpportunityData>> GetOpportunitiesForNewStaffingTab(DemandFilterCriteria filterObj,IEnumerable<ResourceAssignmentViewModel> allocationsStaffedBySupply, 
            IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories,
            IList<Revenue> revenueByServiceLinesOpps, IEnumerable<Guid?> planningBoardPipelineIds, IList<CaseAttribute> caseAttributeLookupList, 
            IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList, string loggedInUser = null)
        {
              if (!IsFilterCriteriaValidForOpportunities(filterObj))
                  return Enumerable.Empty<OpportunityData>();

            Task<IList<OpportunityData>> opportunitiesTask;

            var lstPipelineIdsBySupply = (allocationsStaffedBySupply != null) ? string.Join(",", allocationsStaffedBySupply?.Select(x => x.PipelineId).Distinct()) : "";

            var opportunitiesStaffedBySuppyTask =  _pipelineApiClient.GetOpportunitiesByPipelineIds(lstPipelineIdsBySupply);

            if (filterObj.DemandTypes.Contains(Constants.DemandType.Opportunity))
            {
                opportunitiesTask = _pipelineApiClient.GetOpportunitiesByOfficesActiveInDateRange(filterObj.OfficeCodes, filterObj.StartDate, filterObj.EndDate, filterObj.OpportunityStatusTypeCodes, filterObj.ClientCodes);
            }
            else
            {
                opportunitiesTask = Task.FromResult<IList<OpportunityData>>(new List<OpportunityData>());
            }

            await Task.WhenAll(opportunitiesStaffedBySuppyTask, opportunitiesTask);

            var opportunitiesStaffedBySuppy = opportunitiesStaffedBySuppyTask.Result;
            var opportunities = opportunitiesTask.Result;

            opportunities = opportunities.Concat(opportunitiesStaffedBySuppy.Where(x => !opportunities.Select(y => y.PipelineId).Contains(x.PipelineId))).ToList();


            opportunities = await GetOpportunityRelatedDetails(filterObj, employeesIncludingTerminated, offices, skuTermLookup, investmentCategories, loggedInUser, opportunities, 
                caseRoleTypeList, caseAttributeLookupList, commitmentTypeList);

            var opportunitiesStaffedBySupply = opportunities.Where(c => (bool)lstPipelineIdsBySupply?.Contains(c.PipelineId.ToString())).ToList();

            opportunities = opportunities.Except(opportunitiesStaffedBySupply).ToList();

            opportunities = await FilterOppsByFilterCriteria(filterObj, revenueByServiceLinesOpps, planningBoardPipelineIds, opportunities);
            opportunities = opportunities.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            opportunities = opportunities.Concat(opportunitiesStaffedBySupply).ToList();

            return opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First());
        }


        private async Task<IList<OpportunityData>> GetOpportunityRelatedDetails(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated, 
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories, string loggedInUser, 
            IList<OpportunityData> opportunities, IEnumerable<CaseRoleType> caseRoleTypeList, IList<CaseAttribute> caseAttributeLookupList, IEnumerable<CommitmentType> commitmentTypeList)
        {
            var listPipelineId = string.Join(",", opportunities.Select(x => x.PipelineId.ToString()).ToList());

            // TODO: Check with the team if Sku Terms needs to be fetched between date range for pinned opp
            var opportunitiesSkuTermsDataTask = Task.FromResult(Enumerable.Empty<SKUCaseTerms>());

            if (filterObj != null)
            {
                opportunitiesSkuTermsDataTask =
                    _staffingApiClient.GetSKUTermsForCaseOrOpportunityForDuration(null, listPipelineId, filterObj.StartDate,
                        filterObj.EndDate);
            }
            var allocatedResourcesDataTask = _staffingApiClient.GetResourceAllocationsByPipelineIds(listPipelineId);
            var placeholderAllocationDataTask = _staffingApiClient.GetPlaceholderAllocationsByPipelineIds(listPipelineId);
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(listPipelineId);
            var staCommitmentDetailsTask = _staffingApiClient.GetProjectSTACommitmentDetails(null, listPipelineId, null);

            Task<IEnumerable<CaseViewNote>> casePlanningViewNotesTask = Task.FromResult<IEnumerable<CaseViewNote>>(new List<CaseViewNote>());
            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotesTask = _staffingApiClient.GetCaseViewNotesByPipelineIds(listPipelineId, loggedInUser);
            }

            await
               Task.WhenAll(opportunitiesSkuTermsDataTask, allocatedResourcesDataTask, pipelineChangesTask,
               placeholderAllocationDataTask, casePlanningViewNotesTask, staCommitmentDetailsTask);

            var opportunitiesSkuTerms = opportunitiesSkuTermsDataTask.Result;
            var allocatedResources = allocatedResourcesDataTask.Result;
            var pipelineChanges = pipelineChangesTask.Result;
            var placeholderAllocations = placeholderAllocationDataTask.Result;
            var casePlanningViewNotes = casePlanningViewNotesTask.Result;
            var staCommitmentDetails = staCommitmentDetailsTask.Result;

            OverrideOppChangesSavedInBOSS(opportunities, pipelineChanges);

            var employees = employeesIncludingTerminated.ToList();

            opportunities = ConvertToOpportunityDataModel(opportunities, allocatedResources, placeholderAllocations, opportunitiesSkuTerms,
                skuTermLookup, caseAttributeLookupList, employees, offices, investmentCategories, caseRoleTypeList, commitmentTypeList, casePlanningViewNotes,null,null, staCommitmentDetails);
            return opportunities;
        }

        private async Task<IList<OpportunityData>> FilterOppsByFilterCriteria(DemandFilterCriteria filterObj, IList<Revenue> revenueByServiceLinesOpps, IEnumerable<Guid?> planningBoardPipelineIds, IList<OpportunityData> opportunities)
        {
            // Remove the opps that were updated in BOSS and now the start date of the opp is not in the date range
            opportunities = opportunities
                .Where(x => x.StartDate >= filterObj.StartDate && x.StartDate <= filterObj.EndDate).ToList();

            opportunities = FilterOpportunitiesByPracticeArea(filterObj, opportunities).ToList();
            opportunities = FilterOpportunitiesByProbabilityPercent(filterObj, opportunities);

            opportunities = (List<OpportunityData>)await FilterOpportunitiesByCaseAttributes(filterObj, opportunities, revenueByServiceLinesOpps);

            opportunities = FilterOpportunitiesByStaffFromSupplyFilter(filterObj, planningBoardPipelineIds, opportunities).ToList();
            return opportunities;
        }

        private static void OverrideOppChangesSavedInBOSS(IList<OpportunityData> opportunities, IEnumerable<CaseOppChanges> pipelineChanges)
        {
            pipelineChanges.Join(opportunities, (pipelineChange) => pipelineChange.PipelineId, (opportunity) => opportunity.PipelineId, (pipelineChange, opportunity) =>
            {
                opportunity.isStartDateUpdatedInBOSS = pipelineChange.StartDate.HasValue && pipelineChange.StartDate != DateTime.MinValue && opportunity.StartDate != pipelineChange.StartDate;
                opportunity.isEndDateUpdatedInBOSS = pipelineChange.EndDate.HasValue && pipelineChange.EndDate != DateTime.MinValue && opportunity.EndDate != pipelineChange.EndDate;
                opportunity.StartDate = pipelineChange.StartDate ?? opportunity.StartDate;
                opportunity.EndDate = pipelineChange.EndDate ?? opportunity.EndDate;
                opportunity.ProbabilityPercent = pipelineChange.ProbabilityPercent ?? opportunity.ProbabilityPercent;
                opportunity.Notes = pipelineChange.Notes;
                opportunity.CaseServedByRingfence = pipelineChange.CaseServedByRingfence;
                opportunity.StaffingOfficeCode = pipelineChange.StaffingOfficeCode;

                return opportunity;
            }).ToList();
        }

        public async Task<IEnumerable<OpportunityData>> GetPinnedOpportunitiesDetails(DemandFilterCriteria filterObj, string opportunityExceptionShowList, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories,
            IList<Revenue> revenueByServiceLinesOpps, IEnumerable<Guid?> planningBoardPipelineIds, IList<CaseAttribute> caseAttributeLookupList,
            IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList, string loggedInUser = null)
        {
            if(string.IsNullOrEmpty(opportunityExceptionShowList))
                return Enumerable.Empty<OpportunityData>();

            var opportunities = await _pipelineApiClient.GetOpportunitiesByPipelineIds(opportunityExceptionShowList);

            opportunities = await GetOpportunityRelatedDetails(filterObj, employeesIncludingTerminated, offices, skuTermLookup, investmentCategories, loggedInUser, opportunities,
                caseRoleTypeList, caseAttributeLookupList, commitmentTypeList);

            // TODO: Check with team if filter is required for the pinned opp and uncomment accordingly or delelte it
            //opportunities = await FilterOppsByFilterCriteria(filterObj, revenueByServiceLinesOpps, planningBoardPipelineIds, opportunities);
            
            opportunities = opportunities.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            return opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First());
        }

        public async Task<IEnumerable<OpportunityData>> GetHiddenOpportunitiesDetails(DemandFilterCriteria filterObj, IEnumerable<Resource> employeesIncludingTerminated,
            IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup, IEnumerable<InvestmentCategory> investmentCategories,
            IList<Revenue> revenueByServiceLinesOpps, IEnumerable<Guid?> planningBoardPipelineIds, IList<CaseAttribute> caseAttributeLookupList,
            IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterObj.OpportunityExceptionHideList))
                return Enumerable.Empty<OpportunityData>();

            var opportunities = await _pipelineApiClient.GetOpportunitiesByPipelineIds(filterObj.OpportunityExceptionHideList);

            opportunities = await GetOpportunityRelatedDetails(filterObj, employeesIncludingTerminated, offices, skuTermLookup, investmentCategories, loggedInUser, opportunities,
                caseRoleTypeList, caseAttributeLookupList, commitmentTypeList);

            opportunities = await FilterOppsByFilterCriteria(filterObj, revenueByServiceLinesOpps, planningBoardPipelineIds, opportunities);
            opportunities = opportunities.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();

            return opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First());
        }


        public async Task<IEnumerable<OpportunityData>> GetOpportunitiesByPipelineIdsAndFilterValues(string pipelineIds, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup,
            IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(pipelineIds) && !IsOpportuntiyFilterSet(filterObj))
                return Enumerable.Empty<OpportunityData>();

            Task<IList<OpportunityData>> opportunitiesByFilterValuesTask;

            #region Pipeline API Calls

            if (filterObj.DemandTypes.Contains(Constants.DemandType.Opportunity))
            {
                opportunitiesByFilterValuesTask = _pipelineApiClient.GetOpportunitiesByOfficesActiveInDateRange(filterObj.OfficeCodes, filterObj.StartDate,
                    filterObj.EndDate, filterObj.OpportunityStatusTypeCodes, filterObj.ClientCodes);
            }
            else
            {
                opportunitiesByFilterValuesTask = Task.FromResult<IList<OpportunityData>>(new List<OpportunityData>());
            }

            var opportunitiesByPipelineIdTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(pipelineIds, null, filterObj.OpportunityStatusTypeCodes);

            #endregion

            await Task.WhenAll(opportunitiesByFilterValuesTask, opportunitiesByPipelineIdTask);
            var opportunitiesByFilterValues = opportunitiesByFilterValuesTask.Result;
            var opportunitiesByPipelineId = opportunitiesByPipelineIdTask.Result;

            var opportunities = opportunitiesByFilterValues.Concat(opportunitiesByPipelineId).ToList();

            opportunities = opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First()).ToList(); //get distinct opps

            opportunities = FilterOpportunitiesByPracticeArea(filterObj, opportunities).ToList();

            var lstPipelineIds = string.Join(',', opportunities.Select(x => x.PipelineId.ToString()).Distinct());
            //get notes
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(lstPipelineIds);
            var opportunitiesSkuTermsDataTask =
                _staffingApiClient.GetSKUTermForProjects(null, lstPipelineIds, null);
            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            Task<IEnumerable<CaseViewNote>> casePlanningViewNotesTask;
            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotesTask = _staffingApiClient.GetLatestCaseViewNotes(null,lstPipelineIds,null, loggedInUser);
            }
            else
            {
                casePlanningViewNotesTask = Task.FromResult<IEnumerable<CaseViewNote>>(new List<CaseViewNote>());
            }
            var oppDataWithCortexSkuPlaceholdersTask = _staffingApiClient.GetOppCortexPlaceholderInfoByPipelineIds(lstPipelineIds);

            await Task.WhenAll(pipelineChangesTask, opportunitiesSkuTermsDataTask, caseAttributeLookupListTask, caseRoleTypeListTask, commitmentTypeListTask, casePlanningViewNotesTask, oppDataWithCortexSkuPlaceholdersTask);

            var pipelineChanges = pipelineChangesTask.Result;
            var opportunitySkuDemand = opportunitiesSkuTermsDataTask.Result;
            var caseAttributeLookupList = caseAttributeLookupListTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;
            var casePlanningViewNotes = casePlanningViewNotesTask.Result;
            var oppDataWithCortexSkuPlaceholders = oppDataWithCortexSkuPlaceholdersTask.Result;


            pipelineChanges.Join(opportunities, (pipelineChange) => pipelineChange.PipelineId, (opportunity) => opportunity.PipelineId, (pipelineChange, opportunity) =>
            {
                opportunity.isStartDateUpdatedInBOSS = pipelineChange.StartDate.HasValue && pipelineChange.StartDate != DateTime.MinValue && opportunity.StartDate != pipelineChange.StartDate;
                opportunity.isEndDateUpdatedInBOSS = pipelineChange.EndDate.HasValue && pipelineChange.EndDate != DateTime.MinValue && opportunity.EndDate != pipelineChange.EndDate;
                opportunity.OriginalStartDate = opportunity.StartDate;
                opportunity.OverrideStartDate = pipelineChange.StartDate;
                opportunity.StartDate = pipelineChange.StartDate ?? opportunity.StartDate;
                opportunity.OriginalEndDate = opportunity.EndDate;
                opportunity.OverrideEndDate = pipelineChange.EndDate;
                opportunity.EndDate = pipelineChange.EndDate ?? opportunity.EndDate;
                opportunity.OriginalProbabilityPercent = opportunity.ProbabilityPercent;
                opportunity.OverrideProbabilityPercent = pipelineChange.ProbabilityPercent;
                opportunity.ProbabilityPercent = pipelineChange.ProbabilityPercent ?? opportunity.ProbabilityPercent;
                opportunity.Notes = pipelineChange.Notes;
                opportunity.CaseServedByRingfence = pipelineChange.CaseServedByRingfence;
                opportunity.StaffingOfficeCode = pipelineChange.StaffingOfficeCode;

                return opportunity;
            }).ToList();

            opportunities = FilterBySelectedDateRange(filterObj, opportunities).ToList();

            var employees = employeesIncludingTerminated.ToList();

            opportunities = ConvertToOpportunityDataModel(opportunities, null, null, null,
                skuTermLookup, caseAttributeLookupList, employees, offices, investmentCategories, caseRoleTypeList, commitmentTypeList, casePlanningViewNotes,
                opportunitySkuDemand, oppDataWithCortexSkuPlaceholders);

            //TODO: remove the following line once new SKU logic is implemented
            //the line is added to prevent filtering of cases by SKU
            opportunities.ForEach(opportunity => opportunity.SKUCaseTerms = null);

            opportunities = opportunities
                .Where(x => (!x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.ManagingOfficeCode.ToString()))
                             || x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.StaffingOfficeCode.ToString())).ToList();

            opportunities = (List<OpportunityData>)await FilterOpportunitiesByCaseAttributes(filterObj, opportunities, revenueByServiceLinesOpps);
            return opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First());
        }

        public async Task<IEnumerable<OpportunityData>> GetFilteredOpportunitiesByPipelineIds(string pipelineIds, DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup,
            IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps, string loggedInUser=null)
        {
            if (string.IsNullOrEmpty(pipelineIds) && !IsOpportuntiyFilterSet(filterObj))
                return Enumerable.Empty<OpportunityData>();

            var opportunitiesByPipelineId = await _pipelineApiClient.GetOpportunitiesByPipelineIds(pipelineIds);

            var opportunities = opportunitiesByPipelineId.ToList();

            opportunities = FilterOpportunitiesByPracticeArea(filterObj, opportunities).ToList();

            var lstPipelineIds = string.Join(',', opportunities.Select(x => x.PipelineId.ToString()).Distinct());

            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(lstPipelineIds);
            var opportunitiesSkuTermsDataTask =
                _staffingApiClient.GetSKUTermForProjects(null, lstPipelineIds, null);

            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();
            //get Notes here
            Task<IEnumerable<CaseViewNote>> casePlanningViewNotesTask;
            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotesTask = _staffingApiClient.GetLatestCaseViewNotes(null,lstPipelineIds,null, loggedInUser);
            }
            else
            {
                casePlanningViewNotesTask = Task.FromResult<IEnumerable<CaseViewNote>>(new List<CaseViewNote>());
            }
            var oppDataWithCortexSkuPlaceholdersTask = _staffingApiClient.GetOppCortexPlaceholderInfoByPipelineIds(lstPipelineIds);

            await Task.WhenAll(pipelineChangesTask, opportunitiesSkuTermsDataTask, caseAttributeLookupListTask, caseRoleTypeListTask, commitmentTypeListTask, casePlanningViewNotesTask, oppDataWithCortexSkuPlaceholdersTask);

            var pipelineChanges = pipelineChangesTask.Result;
            var opportunitySkuDemand = opportunitiesSkuTermsDataTask.Result;
            var caseAttributeLookupList = caseAttributeLookupListTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;
            var casePlanningViewNotes = casePlanningViewNotesTask.Result;
            var oppDataWithCortexSkuPlaceholders = oppDataWithCortexSkuPlaceholdersTask.Result;


            pipelineChanges.Join(opportunities, (pipelineChange) => pipelineChange.PipelineId, (opportunity) => opportunity.PipelineId, (pipelineChange, opportunity) =>
            {
                opportunity.isStartDateUpdatedInBOSS = pipelineChange.StartDate.HasValue && pipelineChange.StartDate != DateTime.MinValue && opportunity.StartDate != pipelineChange.StartDate;
                opportunity.isEndDateUpdatedInBOSS = pipelineChange.EndDate.HasValue && pipelineChange.EndDate != DateTime.MinValue && opportunity.EndDate != pipelineChange.EndDate;
                opportunity.StartDate = pipelineChange.StartDate ?? opportunity.StartDate;
                opportunity.EndDate = pipelineChange.EndDate ?? opportunity.EndDate;
                opportunity.ProbabilityPercent = pipelineChange.ProbabilityPercent ?? opportunity.ProbabilityPercent;
                opportunity.Notes = pipelineChange.Notes;
                opportunity.CaseServedByRingfence = pipelineChange.CaseServedByRingfence;
                opportunity.StaffingOfficeCode = pipelineChange.StaffingOfficeCode;

                return opportunity;
            }).ToList();

            var employees = employeesIncludingTerminated.ToList();

            opportunities = ConvertToOpportunityDataModel(opportunities, null, null, null,
                skuTermLookup, caseAttributeLookupList, employees, offices, investmentCategories, caseRoleTypeList, commitmentTypeList, casePlanningViewNotes, opportunitySkuDemand, oppDataWithCortexSkuPlaceholders);

            //TODO: remove the following line once new SKU logic is implemented
            //the line is added to prevent filtering of cases by SKU
            opportunities.ForEach(opportunity => opportunity.SKUCaseTerms = null);

            opportunities = opportunities
                .Where(x => (!x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.ManagingOfficeCode.ToString()))
                             || x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.StaffingOfficeCode.ToString())).ToList();

            opportunities = (List<OpportunityData>)await FilterOpportunitiesByCaseAttributes(filterObj, opportunities, revenueByServiceLinesOpps);
            return opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First());
        }

        public async Task<IEnumerable<OpportunityData>> GetOpportunitiesByFilterValues(DemandFilterCriteria filterObj,
            IEnumerable<Resource> employeesIncludingTerminated, IEnumerable<Office> offices, IEnumerable<SKUTerm> skuTermLookup,
            IEnumerable<InvestmentCategory> investmentCategories, IList<Revenue> revenueByServiceLinesOpps)
        {
            if (!IsOpportuntiyFilterSet(filterObj))
                return Enumerable.Empty<OpportunityData>();

            IList<OpportunityData> opportunitiesByFilterValues = new List<OpportunityData>();

            #region Pipeline API Calls

            if (filterObj.DemandTypes.Contains(Constants.DemandType.Opportunity))
            {
                opportunitiesByFilterValues = await _pipelineApiClient.GetOpportunitiesByOfficesActiveInDateRange(filterObj.OfficeCodes, filterObj.StartDate,
                    filterObj.EndDate, filterObj.OpportunityStatusTypeCodes, filterObj.ClientCodes);
            }

            #endregion

            var opportunities = opportunitiesByFilterValues.ToList();

            if (opportunities.Count() == 0)
                return Enumerable.Empty<OpportunityData>();

            opportunities = FilterOpportunitiesByPracticeArea(filterObj, opportunities).ToList();

            var lstPipelineIds = string.Join(',', opportunities.Select(x => x.PipelineId.ToString()).Distinct());

            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(lstPipelineIds);
            var opportunitiesSkuTermsDataTask =
                _staffingApiClient.GetSKUTermForProjects(null, lstPipelineIds, null);
            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            await Task.WhenAll(pipelineChangesTask, opportunitiesSkuTermsDataTask, caseAttributeLookupListTask, caseRoleTypeListTask, commitmentTypeListTask);

            var pipelineChanges = pipelineChangesTask.Result;
            var opportunitySkuDemand = opportunitiesSkuTermsDataTask.Result;
            var caseAttributeLookupList = caseAttributeLookupListTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            opportunities.ForEach(x =>
            {
                x.OriginalStartDate = x.StartDate;
                x.OriginalEndDate = x.EndDate;
                x.OriginalProbabilityPercent = x.ProbabilityPercent;
            });

            pipelineChanges.Join(opportunities, (pipelineChange) => pipelineChange.PipelineId, (opportunity) => opportunity.PipelineId, (pipelineChange, opportunity) =>
            {
                opportunity.isStartDateUpdatedInBOSS = pipelineChange.StartDate.HasValue && pipelineChange.StartDate != DateTime.MinValue && opportunity.StartDate != pipelineChange.StartDate;
                opportunity.isEndDateUpdatedInBOSS = pipelineChange.EndDate.HasValue && pipelineChange.EndDate != DateTime.MinValue && opportunity.EndDate != pipelineChange.EndDate;
                opportunity.OverrideStartDate = pipelineChange.StartDate;
                opportunity.StartDate = pipelineChange.StartDate ?? opportunity.StartDate;
                opportunity.OverrideEndDate = pipelineChange.EndDate;
                opportunity.EndDate = pipelineChange.EndDate ?? opportunity.EndDate;
                opportunity.OverrideProbabilityPercent = pipelineChange.ProbabilityPercent;
                opportunity.ProbabilityPercent = pipelineChange.ProbabilityPercent ?? opportunity.ProbabilityPercent;
                opportunity.Notes = pipelineChange.Notes;
                opportunity.CaseServedByRingfence = pipelineChange.CaseServedByRingfence;
                opportunity.StaffingOfficeCode = pipelineChange.StaffingOfficeCode;

                return opportunity;
            }).ToList();

            var employees = employeesIncludingTerminated.ToList();

            opportunities = ConvertToOpportunityDataModel(opportunities, null, null, null,
                skuTermLookup, caseAttributeLookupList, employees, offices, investmentCategories, caseRoleTypeList, commitmentTypeList, null, opportunitySkuDemand);

            //TODO: remove the following line once new SKU logic is implemented
            //the line is added to prevent filtering of cases by SKU
            opportunities.ForEach(opportunity => opportunity.SKUCaseTerms = null);

            opportunities = opportunities
                .Where(x => (!x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.ManagingOfficeCode.ToString()))
                             || x.StaffingOfficeCode.HasValue && filterObj.OfficeCodes.Contains(x.StaffingOfficeCode.ToString())).ToList();

            opportunities = (List<OpportunityData>)await FilterOpportunitiesByCaseAttributes(filterObj, opportunities, revenueByServiceLinesOpps);
            return opportunities.GroupBy(x => x.PipelineId).Select(grp => grp.First());
        }

        public async Task<OpportunityDetails> GetOpportunity(Guid pipelineId)
        {
            var opportunityTask = _pipelineApiClient.GetOpportunityDetailsByPipelineId(pipelineId);
            //get pricing team size here
            var oppDataWithPricingTeamSizeTask = _staffingApiClient.GetOppCortexPlaceholderInfoByPipelineIds(pipelineId.ToString());
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineId(pipelineId);
            var employeesTask = _resourceApiClient.GetEmployees();
            var staCommitmentDetailsTask = _staffingApiClient.GetProjectSTACommitmentDetails(null, pipelineId.ToString(), null);

            await Task.WhenAll(opportunityTask, oppDataWithPricingTeamSizeTask, pipelineChangesTask, employeesTask, staCommitmentDetailsTask);

            var opportunityData = opportunityTask.Result;
            var oppDataWithPricingTeamSize = oppDataWithPricingTeamSizeTask.Result.FirstOrDefault();
            var pipelineChangesData = pipelineChangesTask.Result;
            var staCommitmentDetails = staCommitmentDetailsTask.Result;

            if (pipelineChangesData != null && pipelineChangesData.PipelineId.Equals(pipelineId))
            {
                opportunityData.StartDate = pipelineChangesData.StartDate ?? opportunityData.StartDate;
                opportunityData.EndDate = pipelineChangesData.EndDate ?? opportunityData.EndDate;
                opportunityData.Notes = pipelineChangesData.Notes;
                opportunityData.ProbabilityPercent = pipelineChangesData.ProbabilityPercent ?? opportunityData.ProbabilityPercent;
                opportunityData.CaseServedByRingfence = pipelineChangesData?.CaseServedByRingfence ?? pipelineChangesData?.CaseServedByRingfence;
            }

            var coordinatingPartner =
                employeesTask.Result.FirstOrDefault(r => r.EmployeeCode == opportunityData.CoordinatingPartnerCode);
            var billingPartner =
                employeesTask.Result.FirstOrDefault(r => r.EmployeeCode == opportunityData.BillingPartnerCode);
            var otherPartners = opportunityData.OtherPartnersCodes != null
                ? employeesTask.Result.Where(
                    r => opportunityData.OtherPartnersCodes.Split(',').Contains(r.EmployeeCode))
                : new List<Resource>();

            var otherPartnersNamesWithOfficeAbbreviation = otherPartners.Aggregate(string.Empty,
                (current, otherPartner) => current + " " + otherPartner.FullName + " - " +
                                           otherPartner.Office.OfficeAbbreviation + ",");

            return ConvertToOpportunityDetailsModel(opportunityData, coordinatingPartner, billingPartner,
                otherPartnersNamesWithOfficeAbbreviation, null, oppDataWithPricingTeamSize, staCommitmentDetails);
        }

        public async Task<OpportunityDetails> GetOpportunityAndAllocationsByPipelineId(Guid pipelineId)
        {
            var opportunityData = await _pipelineApiClient.GetOpportunityDetailsByPipelineId(pipelineId);

            var allocatedResourcesDataTask =
                _staffingApiClient.GetResourceAllocationsByPipelineId(opportunityData.PipelineId.ToString());

            var employeesTask = _resourceApiClient.GetEmployees();
            var officesTask = _ccmApiClient.GetOfficeList();
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineId(pipelineId);
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();

            await Task.WhenAll(allocatedResourcesDataTask, employeesTask, pipelineChangesTask, investmentCategoriesTask);

            var allocatedResourcesData = allocatedResourcesDataTask.Result;
            var employees = employeesTask.Result;
            var offices = officesTask.Result;
            var pipelineChanges = pipelineChangesTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;

            //start date, end date and probability percent are editable and saved in staffing db. Hence it overrides one coming from pipeline
            if (pipelineChanges != null)
            {
                opportunityData.StartDate = pipelineChanges.StartDate ?? opportunityData.StartDate;
                opportunityData.EndDate = pipelineChanges.EndDate ?? opportunityData.EndDate;
                opportunityData.Notes = pipelineChanges.Notes;
                opportunityData.ProbabilityPercent = pipelineChanges.ProbabilityPercent ?? opportunityData.ProbabilityPercent;
                opportunityData.CaseServedByRingfence = pipelineChanges.CaseServedByRingfence;
                //opportunityData.EndDate = CalculateLikelyEndDate(opportunityData.Duration, pipelineChanges.StartDate);
            }

            var allocatedResources = ConvertToResourceAssignmentViewModel(allocatedResourcesData, opportunityData, employees, offices, investmentCategories);

            var coordinatingPartner =
                employees.FirstOrDefault(r => r.EmployeeCode == opportunityData.CoordinatingPartnerCode);
            var billingPartner =
                employees.FirstOrDefault(r => r.EmployeeCode == opportunityData.BillingPartnerCode);

            var otherPartnersNamesWithOfficeAbbreviation = string.Empty;
            if (string.IsNullOrEmpty(opportunityData.OtherPartnersCodes))
                return ConvertToOpportunityDetailsModel(opportunityData, coordinatingPartner, billingPartner,
                    otherPartnersNamesWithOfficeAbbreviation, allocatedResources);

            var otherPartners = employeesTask.Result.Where(r =>
                opportunityData.OtherPartnersCodes.Split(',').Contains(r.EmployeeCode));
            otherPartnersNamesWithOfficeAbbreviation = otherPartners.Aggregate(string.Empty,
                (current, otherPartner) => current + " " + otherPartner.FullName + " - " +
                                           otherPartner.Office.OfficeAbbreviation + ",");
            return ConvertToOpportunityDetailsModel(opportunityData, coordinatingPartner, billingPartner,
                otherPartnersNamesWithOfficeAbbreviation, allocatedResources);
        }


        public async Task<IEnumerable<OpportunityData>> GetOpportunitiesForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
                return Enumerable.Empty<OpportunityData>();

            var accessibleOffices = JWTHelper.GetAccessibleOffices(_httpContextAccessor.HttpContext);

            var opportunitiesData = await _pipelineApiClient.GetOpportunitiesForTypeahead(searchString);

            var pipelineIds = string.Join(',', opportunitiesData.Select(x => x.PipelineId.ToString()).ToList());

            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(pipelineIds);
            var employeesTask = _resourceApiClient.GetEmployees();

            await Task.WhenAll(pipelineChangesTask, employeesTask);

            var pipelineChanges = pipelineChangesTask.Result;
            var employees = employeesTask.Result;

            var offices = accessibleOffices?.Select(i => (int?)i).ToList();

            opportunitiesData = GetAccessibleOpportunities(opportunitiesData, employees, offices);

            var opportunities = ConvertToOpportunityDataModel(opportunitiesData.ToList(), employees, pipelineChanges.ToList());

            return opportunities;
        }

        #region Private methods

        private IEnumerable<OpportunityData> GetAccessibleOpportunities(IEnumerable<OpportunityData> opportunitiesData,
            IEnumerable<Resource> resources, List<int?> accessibleOffices)
        {
            if (accessibleOffices != null && !accessibleOffices.Any())
            {
                return opportunitiesData.GroupBy(x => x.PipelineId).Select(grp => grp.First()).ToList();
            }
            // Splitting opportunities into multiple rows for Opportunity having multiple other partners
            var splittedOpportunityData = new List<OpportunityData>();
            splittedOpportunityData.AddRange(opportunitiesData.Where(x => string.IsNullOrEmpty(x.OtherPartnersCodes)));
            splittedOpportunityData.AddRange(opportunitiesData.Where(x => !string.IsNullOrEmpty(x.OtherPartnersCodes))
                .SelectMany(o => o.OtherPartnersCodes?.Split(',')?
                .Select(x => new OpportunityData
                {
                    PipelineId = o.PipelineId,
                    CortexId = o?.CortexId,
                    EstimatedTeamSize = o?.EstimatedTeamSize,
                    CoordinatingPartnerCode = o.CoordinatingPartnerCode,
                    BillingPartnerCode = o.BillingPartnerCode,
                    OtherPartnersCodes = x?.Trim(),
                    OpportunityName = o.OpportunityName,
                    ClientCode = o.ClientCode,
                    ClientName = o.ClientName,
                    ManagingOfficeAbbreviation = o.ManagingOfficeAbbreviation,
                    ManagingOfficeCode = o.ManagingOfficeCode,
                    StartDate = o.StartDate,
                    EndDate = o.EndDate,
                    Duration = o.Duration,
                    ProbabilityPercent = o.ProbabilityPercent,
                    PrimaryIndustry = o.PrimaryIndustry,
                    PrimaryCapability = o.PrimaryCapability,
                    Type = o.Type,
                    CaseAttributes = o.CaseAttributes,
                    Notes = o.Notes,
                    CaseServedByRingfence = o.CaseServedByRingfence,
                    AllocatedResources = o.AllocatedResources,
                    PlaceholderAllocations = o.PlaceholderAllocations,
                    SKUCaseTerms = o.SKUCaseTerms
                })).ToList());

            var opportunitesAccessibleByUser =
                (
                    from opp in splittedOpportunityData
                    join res in resources on opp.BillingPartnerCode equals res.EmployeeCode into billingPartnerGrp
                    from billingPartner in billingPartnerGrp.DefaultIfEmpty()
                    join res in resources on opp.CoordinatingPartnerCode equals res.EmployeeCode into coordinatingPartnerGrp
                    from coordinatingPartner in coordinatingPartnerGrp.DefaultIfEmpty()
                    join res in resources on opp.OtherPartnersCodes equals res.EmployeeCode into otherPartnerGrp
                    from otherpartner in otherPartnerGrp.DefaultIfEmpty()
                    select new OpportunityData()
                    {
                        PipelineId = opp.PipelineId,
                        CortexId = opp?.CortexId,
                        EstimatedTeamSize = opp?.EstimatedTeamSize,
                        CoordinatingPartnerCode = opp.CoordinatingPartnerCode,
                        OpportunityName = opp.OpportunityName,
                        ClientCode = opp.ClientCode,
                        ClientName = opp.ClientName,
                        ManagingOfficeAbbreviation = coordinatingPartner?.Office.OfficeAbbreviation,
                        ManagingOfficeCode = coordinatingPartner?.Office.OfficeCode,
                        BillingOfficeAbbreviation = billingPartner?.Office.OfficeAbbreviation,
                        BillingOfficeCode = billingPartner?.Office.OfficeCode,
                        OtherOfficeAbbreviation = otherpartner?.Office.OfficeAbbreviation,
                        OtherOfficeCode = otherpartner?.Office.OfficeCode,
                        StartDate = opp.StartDate,
                        EndDate = opp.EndDate,
                        ProbabilityPercent = opp.ProbabilityPercent,
                        PrimaryIndustry = opp.PrimaryIndustry,
                        PrimaryCapability = opp.PrimaryCapability,
                        Notes = opp.Notes,
                        CaseServedByRingfence = opp.CaseServedByRingfence,
                        Type = "Opportunity"
                    }).Where(opp => accessibleOffices == null || accessibleOffices.Contains(opp.ManagingOfficeCode)
                        || accessibleOffices.Contains(opp.BillingOfficeCode)
                        || accessibleOffices.Contains(opp.OtherOfficeCode)).ToList();

            var filteredOppData = from opp in opportunitiesData
                                  join accessibleOpp in opportunitesAccessibleByUser on opp.PipelineId equals accessibleOpp.PipelineId into matches
                                  where matches.Any()
                                  select opp;

            return filteredOppData.GroupBy(x => x.PipelineId).Select(grp => grp.First()).ToList();
        }

        private static bool IsOpportuntiyFilterSet(DemandFilterCriteria filterObj)
        {
            //All opportunities are billable onLy. If search contains billable , then search for OPPORTUNITIES else return empty array.
            return !((string.IsNullOrEmpty(filterObj.OfficeCodes)
                        || string.IsNullOrEmpty(filterObj.OpportunityStatusTypeCodes)
                            || (!filterObj.DemandTypes.Contains(Constants.DemandType.Opportunity) && !filterObj.DemandTypes.Contains(Constants.DemandType.CasesStaffedBySupply) && string.IsNullOrEmpty(filterObj.OpportunityExceptionShowList))
                                || !filterObj.CaseTypeCodes.Split(',').Contains(((int)Constants.CaseType.Billable).ToString())));
        }

        private static bool IsFilterCriteriaValidForOpportunities(DemandFilterCriteria filterObj)
        {
            //All opportunities are billable onLy. If search contains billable , then search for OPPORTUNITIES else return empty array.
            return !((string.IsNullOrEmpty(filterObj.OpportunityStatusTypeCodes)
                            || (!filterObj.DemandTypes.Contains(Constants.DemandType.Opportunity) && !filterObj.DemandTypes.Contains(Constants.DemandType.CasesStaffedBySupply)))
                                || !filterObj.CaseTypeCodes.Split(',').Contains(((int)Constants.CaseType.Billable).ToString()));
        }

        private static OpportunityDetails ConvertToOpportunityDetailsModel(OpportunityDetails opportunityData, 
            Resource coordinatingPartner, Resource billingPartner, string otherPartnersNamesWithOfficeAbbreviation, IEnumerable<ResourceAssignmentViewModel> allocatedResourcesData = null, CaseOppCortexTeamSize oppDataWithPricingTeamSize = null, IEnumerable<CaseOppCommitmentViewModel> staCommitmentDetails = null)
        {
            var opportunity = new OpportunityDetails
            {
                PipelineId = opportunityData.PipelineId,
                CortexId = opportunityData?.CortexId,
                EstimatedTeamSize = opportunityData?.EstimatedTeamSize,
                PricingTeamSize = oppDataWithPricingTeamSize?.PricingTeamSize,
                CoordinatingPartnerCode = opportunityData.CoordinatingPartnerCode,
                CoordinatingPartnerName = coordinatingPartner?.FullName,
                BillingPartnerCode = opportunityData.BillingPartnerCode,
                BillingPartnerName = billingPartner?.FullName,
                OtherPartnersCodes = opportunityData.OtherPartnersCodes,
                OtherPartnersNamesWithOfficeAbbreviations =
                    otherPartnersNamesWithOfficeAbbreviation?.TrimStart(' ').TrimEnd(','),
                OpportunityName = opportunityData.OpportunityName,
                OpportunityStatus = opportunityData.OpportunityStatus,
                ClientCode = opportunityData.ClientCode,
                ClientName = opportunityData.ClientName,
                PrimaryIndustry = opportunityData.PrimaryIndustry,
                IndustryPracticeArea = opportunityData.IndustryPracticeArea,
                PrimaryCapability = opportunityData.PrimaryCapability,
                CapabilityPracticeArea = opportunityData.CapabilityPracticeArea,
                ManagingOfficeName = coordinatingPartner?.Office.OfficeName,
                ManagingOfficeAbbreviation = coordinatingPartner?.Office.OfficeAbbreviation,
                BillingOfficeName = billingPartner?.Office.OfficeName,
                BillingOfficeAbbreviation = billingPartner?.Office.OfficeAbbreviation,
                StartDate = opportunityData.StartDate,
                EndDate = opportunityData.EndDate,
                ProbabilityPercent = opportunityData.ProbabilityPercent,
                Type = "Opportunity",
                CaseAttributes = opportunityData.CaseAttributes,
                AllocatedResources = allocatedResourcesData,
                Notes = opportunityData.Notes,
                CaseServedByRingfence = opportunityData.CaseServedByRingfence,
                isSTACommitmentCreated = staCommitmentDetails?.Any(x=> x.OpportunityId == opportunityData.PipelineId) ?? false
            };

            return opportunity;
        }

        private static IList<ResourceAssignmentViewModel> AttachEmployeeInfo(IEnumerable<ResourceAssignmentViewModel> resourceAllocations, IEnumerable<Resource> resources,
            IEnumerable<Office> offices, IEnumerable<InvestmentCategory> investmentCategories, IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList)
        {
            var resourceAllocationsWithResourceInfo = (from resAlloc in resourceAllocations
                                                       join res in resources on resAlloc.EmployeeCode equals res.EmployeeCode into resAllocGroups
                                                       from resource in resAllocGroups.DefaultIfEmpty()
                                                       join o in offices on resAlloc.OperatingOfficeCode equals o.OfficeCode into resAllocOffices
                                                       from office in resAllocOffices.DefaultIfEmpty()
                                                       join ic in investmentCategories on resAlloc.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                                                       from invesmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                                                       join crt in caseRoleTypeList on resAlloc.CaseRoleCode equals crt.CaseRoleCode into resAllocCaseRoleType
                                                       from caseRoleType in resAllocCaseRoleType.DefaultIfEmpty()
                                                       join ct in commitmentTypeList on resAlloc.CommitmentTypeCode equals ct.CommitmentTypeCode into resCommitmentTypeList
                                                       from commitmentType in resCommitmentTypeList.DefaultIfEmpty()
                                                       select new ResourceAssignmentViewModel()
                                                       {
                                                           Id = resAlloc.Id,
                                                           PipelineId = resAlloc.PipelineId,
                                                           CortexId = resAlloc?.CortexId,
                                                           EstimatedTeamSize = resAlloc?.EstimatedTeamSize,
                                                           EmployeeCode = resAlloc.EmployeeCode,
                                                           EmployeeName = resource?.FullName,
                                                           InternetAddress = resource?.InternetAddress,
                                                           OperatingOfficeCode = resAlloc.OperatingOfficeCode,
                                                           OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                                           CurrentLevelGrade = resAlloc.CurrentLevelGrade,
                                                           Allocation = resAlloc.Allocation,
                                                           StartDate = resAlloc.StartDate,
                                                           EndDate = resAlloc.EndDate,
                                                           InvestmentCode = resAlloc.InvestmentCode,
                                                           InvestmentName = invesmentCategory?.InvestmentName,
                                                           CaseRoleCode = resAlloc.CaseRoleCode,
                                                           CaseRoleName = caseRoleType?.CaseRoleName,
                                                           LastUpdatedBy = resAlloc.LastUpdatedBy,
                                                           Notes = resAlloc.Notes,
                                                           TerminationDate = resource?.TerminationDate,
                                                           ServiceLineCode = resAlloc.ServiceLineCode,
                                                           ServiceLineName = resAlloc.ServiceLineName,
                                                           CommitmentTypeCode = resAlloc.CommitmentTypeCode,
                                                           CommitmentTypeName = commitmentType?.CommitmentTypeName,
                                                           IsPlaceholderAllocation = resAlloc.IsPlaceholderAllocation,
                                                           PositionGroupCode = resAlloc?.PositionGroupCode
                                                       }).ToList();

            return resourceAllocationsWithResourceInfo;
        }

        private static List<OpportunityData> ConvertToOpportunityDataModel(IList<OpportunityData> opportunities,
            IList<ResourceAssignmentViewModel> allocatedResources, IList<ResourceAssignmentViewModel> placeholderAllocations, IEnumerable<SKUCaseTerms> opportunitiesSkuTerms,
            IEnumerable<SKUTerm> skuTermLookup, IList<CaseAttribute> caseAttributeLookupList, IList<Resource> employees, IEnumerable<Office> offices,
            IEnumerable<InvestmentCategory> investmentCategories, IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<CommitmentType> commitmentTypeList,
            IEnumerable<CaseViewNote> caseViewNotes = null, IEnumerable<SKUDemand> opportunitySkuDemand = null, IEnumerable<CaseOppCortexTeamSize> caseOppCortexTeamSizes = null, IEnumerable<CaseOppCommitmentViewModel> staCommitmentDetails = null)
        {
            var resourceAllocationsWithResourceInfo = allocatedResources == null
                ? null
                : AttachEmployeeInfo(allocatedResources, employees, offices, investmentCategories, caseRoleTypeList, commitmentTypeList);
            var placeholderAllocationsWithResourceInfo = placeholderAllocations == null
                ? null
                : AttachEmployeeInfo(placeholderAllocations, employees, offices, investmentCategories, caseRoleTypeList, commitmentTypeList);

            var data = opportunities.Select(item => new OpportunityData()
            {
                PipelineId = item.PipelineId,
                CortexId = item?.CortexId,
                EstimatedTeamSize = item?.EstimatedTeamSize,
                OpportunityName = item.OpportunityName,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                CoordinatingPartnerName = employees.FirstOrDefault(x => x.EmployeeCode.Equals(item.CoordinatingPartnerCode, StringComparison.InvariantCultureIgnoreCase))?.FullName,
                BillingPartnerCode = item.BillingPartnerCode,
                BillingPartnerName = employees.FirstOrDefault(x => x.EmployeeCode.Equals(item.BillingPartnerCode, StringComparison.InvariantCultureIgnoreCase))?.FullName,
                BillingOfficeCode = employees.FirstOrDefault(x => x.EmployeeCode.Equals(item.BillingPartnerCode, StringComparison.InvariantCultureIgnoreCase))?.Office.OfficeCode,
                BillingOfficeAbbreviation = employees.FirstOrDefault(x => x.EmployeeCode.Equals(item.BillingPartnerCode, StringComparison.InvariantCultureIgnoreCase))?.Office.OfficeAbbreviation,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                ManagingOfficeCode = item.ManagingOfficeCode ?? employees
                                                         .FirstOrDefault(e => e.EmployeeCode.Equals(item.CoordinatingPartnerCode, StringComparison.InvariantCultureIgnoreCase))
                                                         ?.Office.OfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation ?? employees
                                                         .FirstOrDefault(e => e.EmployeeCode.Equals(item.CoordinatingPartnerCode, StringComparison.InvariantCultureIgnoreCase))
                                                         ?.Office.OfficeAbbreviation,
                OriginalStartDate = item.OriginalStartDate != null ? item.OriginalStartDate : item.StartDate,
                OverrideStartDate = item.OverrideStartDate,
                StartDate = item.StartDate,
                //EndDate = item.EndDate ?? CalculateLikelyEndDate(item.Duration, pipelineChange?.StartDate),
                OriginalEndDate = item.OriginalEndDate != null ? item.OriginalEndDate : item.EndDate,
                OverrideEndDate = item.OverrideEndDate,
                EndDate = item.EndDate,
                OriginalProbabilityPercent = item.OriginalProbabilityPercent != null ? item.OriginalProbabilityPercent : item.ProbabilityPercent,
                OverrideProbabilityPercent = item.OverrideProbabilityPercent,
                ProbabilityPercent = item.ProbabilityPercent,
                Notes = item.Notes,
                CaseServedByRingfence = item.CaseServedByRingfence,
                Type = "Opportunity",
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                AllocatedResources = ConvertToResourceAssignmentViewModel(
                                resourceAllocationsWithResourceInfo?.Where(x => x.PipelineId == item.PipelineId), item
                            ),
                PlaceholderAllocations = ConvertToResourceAssignmentViewModel(
                                placeholderAllocationsWithResourceInfo?.Where(x => x.PipelineId == item.PipelineId), item
                            ),
                SKUTerm = opportunitySkuDemand?.Where(x => x.PipelineId == item.PipelineId),
                // SKUTerm = opportunitiesSkuTerms.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.SkuTerm,
                SKUCaseTerms = opportunitiesSkuTerms?.Where(x => x.PipelineId == item.PipelineId)?.Select(x =>
                    new SKUCaseTermsViewModel
                    {
                        Id = x.Id,
                        PipelineId = x.PipelineId,
                        OldCaseCode = x.OldCaseCode,
                        EffectiveDate = x.EffectiveDate,
                        LastUpdatedBy = x.LastUpdatedBy,
                        SKUTerms = x.SKUTermsCodes?.Split(',').Select(int.Parse).ToList().Join(skuTermLookup,
                            c => c,
                            s => s.Code,
                            (caseSku, skuTerm) => new SKUTerm
                            {
                                Code = skuTerm.Code,
                                Name = skuTerm.Name
                            }).ToList() // TODO: remove this once new SKU logic is implemented everywhere
                    }).FirstOrDefault(),
                CaseAttributes = string.IsNullOrEmpty(item.CaseAttributes)
                                ? null
                                : string.Join(",", item.CaseAttributes?.Split(',').Select(int.Parse).ToList()
                                    .Join(caseAttributeLookupList,
                                        o => o,
                                        c => c.CaseAttributeCode,
                                        (opportunityAttribute, caseAttribute) => caseAttribute.CaseAttributeName).ToList()),
                isStartDateUpdatedInBOSS = item.isStartDateUpdatedInBOSS,
                isEndDateUpdatedInBOSS = item.isEndDateUpdatedInBOSS,
                StaffingOfficeCode = item.StaffingOfficeCode,
                StaffingOfficeAbbreviation = item.StaffingOfficeAbbreviation ?? offices
                                                         .FirstOrDefault(e => e.OfficeCode == item.StaffingOfficeCode)
                                                         ?.OfficeAbbreviation,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                CasePlanningViewNotes = caseViewNotes == null ? null : caseViewNotes.Where(x => x.PipelineId == item.PipelineId),
                IsPlaceholderCreatedFromCortex = caseOppCortexTeamSizes?.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.IsPlaceholderCreatedFromCortex,
                isSTACommitmentCreated = staCommitmentDetails?.Any(x => x.OpportunityId == item.PipelineId) ?? false,
                PricingTeamSize = caseOppCortexTeamSizes?.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.PricingTeamSize,
                
                
                //LatestCaseViewNote = caseViewNotes == null ? null : caseViewNotes.Where(x=> x.PipelineId == item.PipelineId).FirstOrDefault(),
            }).ToList();

            return data;
        }

        private static IList<OpportunityData> ConvertToOpportunityDataModel(IList<OpportunityData> opportunities,
            IList<Resource> employees, IList<CaseOppChanges> pipelineChanges)
        {
            var opportunityList = (from opp in opportunities
                                   let pipelineChange = pipelineChanges?.FirstOrDefault(x => x.PipelineId == opp.PipelineId)
                                   let resource = employees.FirstOrDefault(y => string.Equals(y.EmployeeCode,
                                       opp.CoordinatingPartnerCode, StringComparison.CurrentCultureIgnoreCase))
                                   select new OpportunityData()
                                   {
                                       PipelineId = opp.PipelineId,
                                       CortexId = opp?.CortexId,
                                       EstimatedTeamSize = opp?.EstimatedTeamSize,
                                       CoordinatingPartnerCode = opp.CoordinatingPartnerCode,
                                       CoordinatingPartnerName = resource?.FullName,
                                       OpportunityName = opp.OpportunityName,
                                       ClientCode = opp.ClientCode,
                                       ClientName = opp.ClientName,
                                       ManagingOfficeAbbreviation = resource?.Office.OfficeAbbreviation,
                                       ManagingOfficeCode = resource?.Office.OfficeCode,
                                       StartDate = pipelineChange?.StartDate ?? opp.StartDate,
                                       EndDate = pipelineChange?.EndDate ?? opp.EndDate,
                                       ProbabilityPercent = pipelineChange?.ProbabilityPercent ?? opp.ProbabilityPercent,
                                       PrimaryIndustry = opp.PrimaryIndustry,
                                       PrimaryCapability = opp.PrimaryCapability,
                                       Notes = pipelineChange?.Notes,
                                       CaseServedByRingfence = pipelineChange?.CaseServedByRingfence,
                                       Type = "Opportunity"
                                   }).ToList();

            return opportunityList;
        }

        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations, OpportunityData opportunityItem)
        {
            var allocations = resourceAllocations?.Select(resAlloc => new ResourceAssignmentViewModel()
            {
                Id = resAlloc.Id,
                ClientName = opportunityItem.ClientName,
                PipelineId = resAlloc.PipelineId,
                OpportunityName = opportunityItem.OpportunityName,
                EmployeeCode = resAlloc.EmployeeCode,
                EmployeeName = resAlloc.EmployeeName,
                InternetAddress = resAlloc.InternetAddress,
                OperatingOfficeCode = resAlloc.OperatingOfficeCode,
                OperatingOfficeAbbreviation = resAlloc.OperatingOfficeAbbreviation,
                CurrentLevelGrade = resAlloc.CurrentLevelGrade,
                Allocation = resAlloc.Allocation,
                StartDate = resAlloc.StartDate,
                EndDate = resAlloc.EndDate,
                InvestmentCode = resAlloc.InvestmentCode,
                InvestmentName = resAlloc.InvestmentName,
                CaseRoleCode = resAlloc.CaseRoleCode,
                CaseRoleName = resAlloc?.CaseRoleName,
                LastUpdatedBy = resAlloc.LastUpdatedBy,
                Notes = resAlloc.Notes,
                OpportunityStartDate = opportunityItem.StartDate,
                OpportunityEndDate = opportunityItem.EndDate,
                TerminationDate = resAlloc.TerminationDate,
                ServiceLineCode = resAlloc.ServiceLineCode,
                ServiceLineName = resAlloc.ServiceLineName,
                CommitmentTypeName = resAlloc.CommitmentTypeName,
                CommitmentTypeCode = resAlloc.CommitmentTypeCode,
                IsPlaceholderAllocation = resAlloc.IsPlaceholderAllocation,
                PositionGroupCode = resAlloc?.PositionGroupCode
            }).ToList();



            return allocations;
        }

        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations,
            OpportunityDetails opportunityItem, IEnumerable<Resource> resources, IEnumerable<Office> offices, IEnumerable<InvestmentCategory> investmentCategories)
        {
            var allocations = (from resAlloc in resourceAllocations
                               join res in resources on resAlloc.EmployeeCode equals res.EmployeeCode into resAllocGroups
                               from resource in resAllocGroups.DefaultIfEmpty()
                               join o in offices on resAlloc.OperatingOfficeCode equals o.OfficeCode into resAllocOffices
                               from office in resAllocOffices.DefaultIfEmpty()
                               join ic in investmentCategories on resAlloc.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                               from investmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                               select new ResourceAssignmentViewModel()
                               {
                                   Id = resAlloc.Id,
                                   ClientName = opportunityItem.ClientName,
                                   PipelineId = resAlloc.PipelineId,
                                   OpportunityName = opportunityItem.OpportunityName,
                                   EmployeeCode = resAlloc.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   InternetAddress = resource?.InternetAddress,
                                   OperatingOfficeCode = resAlloc.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   CurrentLevelGrade = resAlloc.CurrentLevelGrade,
                                   Allocation = resAlloc.Allocation,
                                   StartDate = resAlloc.StartDate,
                                   EndDate = resAlloc.EndDate,
                                   InvestmentCode = resAlloc.InvestmentCode,
                                   InvestmentName = investmentCategory?.InvestmentName,
                                   CaseRoleCode = resAlloc.CaseRoleCode,
                                   LastUpdatedBy = resAlloc.LastUpdatedBy,
                                   Notes = resAlloc.Notes,
                                   OpportunityStartDate = opportunityItem?.StartDate,
                                   OpportunityEndDate = opportunityItem?.EndDate
                               }).ToList();

            return allocations;

        }

        private static List<OpportunityData> GetOpportunitiesIncludingStaffedBySupplyAndUserPreferences(DemandFilterCriteria filterObj,
            IList<OpportunityData> unfilteredOpportunitiesData, IList<OpportunityData> pinnedOppsAndOppsStaffedBySupplyAndOppsUpdatedInBOSSInDateRange,
            string lstStaffedBySupplyPipelineIds, string lstPipelineIdsOfOppsUpdatedInBOSSInDateRange)
        {
            var unfilteredOpportunites = unfilteredOpportunitiesData.ToList();

            // adding updated opps in BOSS within the selected date range to the unfiltered opps
            unfilteredOpportunites.AddRange(pinnedOppsAndOppsStaffedBySupplyAndOppsUpdatedInBOSSInDateRange
                .Where(c => lstPipelineIdsOfOppsUpdatedInBOSSInDateRange.Contains(c.PipelineId.ToString())));

            //remove Opps that were marked as hidden by user
            var filteredOpportunites = unfilteredOpportunites
                .Where(x => !(bool)filterObj.OpportunityExceptionHideList?.Contains(x.PipelineId.ToString())).ToList();

            //add Opps that were marked as pinned by user
            filteredOpportunites.InsertRange(0, pinnedOppsAndOppsStaffedBySupplyAndOppsUpdatedInBOSSInDateRange
                .Where(c => filterObj.OpportunityExceptionShowList.Contains(c.PipelineId.ToString())));
            filteredOpportunites.AddRange(pinnedOppsAndOppsStaffedBySupplyAndOppsUpdatedInBOSSInDateRange
                .Where(c => lstStaffedBySupplyPipelineIds.Contains(c.PipelineId.ToString())));


            filteredOpportunites = filteredOpportunites.GroupBy(x => x.PipelineId).Select(g => g.FirstOrDefault()).ToList();

            return filteredOpportunites;
        }

        private static List<OpportunityData> FilterOpportunitiesByProbabilityPercent(DemandFilterCriteria filterObj,
            IList<OpportunityData> opportunities)
        {
            var filteredOpportunities = new List<OpportunityData>();
            foreach (var opp in opportunities)
            {
                var probabilityPercent = opp.ProbabilityPercent.HasValue ? opp.ProbabilityPercent : 0;

                if (probabilityPercent >= filterObj.MinOpportunityProbability)
                {
                    filteredOpportunities.Add(opp);
                }
            }

            return filteredOpportunities;
        }

        private IList<OpportunityData> FilterOpportunitiesByPracticeArea(DemandFilterCriteria filterObj,
            IList<OpportunityData> opportunities)
        {
            var filteredOpportunities = opportunities;

            if (!string.IsNullOrEmpty(filterObj.IndustryPracticeAreaCodes))
            {
                filteredOpportunities = filteredOpportunities?.Where(x => filterObj.IndustryPracticeAreaCodes.Split(",")
                                                                .Contains(x.IndustryPracticeAreaCode?.ToString())).ToList();
            }
            if (!string.IsNullOrEmpty(filterObj.CapabilityPracticeAreaCodes))
            {
                filteredOpportunities = filteredOpportunities?.Where(x => filterObj.CapabilityPracticeAreaCodes.Split(",")
                                                                .Contains(x.CapabilityPracticeAreaCode?.ToString())).ToList();
            }

            return filteredOpportunities;
        }

        private IList<OpportunityData> FilterBySelectedDateRange(DemandFilterCriteria filterObj,
            IList<OpportunityData> opportunities)
        {
            var filteredOpportunities = opportunities;
            filteredOpportunities = filteredOpportunities?.Where(x => x.StartDate >= filterObj.StartDate && x.StartDate <= filterObj.EndDate).ToList();
            return filteredOpportunities;
        }

        private async Task<IList<OpportunityData>> FilterOpportunitiesByCaseAttributes(DemandFilterCriteria filterObj,
            IList<OpportunityData> opportunities, IList<Revenue> revenueByServiceLinesOpps)
        {
            if (string.IsNullOrEmpty(filterObj.CaseAttributeNames))
                return opportunities.OrderBy(x => x.StartDate).ToList();

            var opportunitiesFilteredByCaseAttributes = new List<OpportunityData>();

            bool isSelectedServicecontainsPEG = false;

            foreach (var selectedServiceLineCode in filterObj.CaseAttributeNames.Split(','))
            {
                if (selectedServiceLineCode == Constants.ServiceLineCodes.PEG || selectedServiceLineCode == Constants.ServiceLineCodes.PEG_SURGE)
                {
                    isSelectedServicecontainsPEG = true;
                    opportunitiesFilteredByCaseAttributes
                        .AddRange(opportunities.Where(x => x.CaseServedByRingfence ?? false));
                }
                if (selectedServiceLineCode == Constants.ServiceLineCodes.AAG)
                {
                    opportunitiesFilteredByCaseAttributes.AddRange(opportunities.Where(x =>
                            x.CaseAttributes?.Split(",").ToList().Any(a =>
                                a.Contains(Constants.CaseAttribute.AAG, StringComparison.OrdinalIgnoreCase)) ?? false)
                        .ToList());
                }

                var oppsWithSelectedServiceLine = revenueByServiceLinesOpps?.Where(x => x.ServiceLineCode == selectedServiceLineCode).Select(y => y.OpportunityId).ToList();

                if (oppsWithSelectedServiceLine != null)
                {
                    opportunitiesFilteredByCaseAttributes.AddRange(opportunities.Where(
                        x => oppsWithSelectedServiceLine.Any(opId => opId == x.PipelineId.ToString())
                        ).ToList());
                }
            }

            if (!isSelectedServicecontainsPEG)
            {
                opportunitiesFilteredByCaseAttributes.RemoveAll(x => x.CaseServedByRingfence ?? false);
            }

            var caseAttributeNameList = CommonUtils.GetServiceLineCodeNames(filterObj.CaseAttributeNames);

            foreach (var opportunity in opportunities)
            {
                var oppPercent = opportunity.ProbabilityPercent.HasValue ? opportunity.ProbabilityPercent : 0;

                if (oppPercent < filterObj.MinOpportunityProbability)
                    continue;

                //TODO: update sku based staffing tag filter logic once new sku logic is implemneted 
                var opportunityAssignedSkuTerms = opportunity.SKUCaseTerms?.SKUTerms?.Select(s => s.Name).ToList();
                var opportunityAssignedAttributes = opportunity.CaseAttributes?.Split(",").ToList();
                if (opportunityAssignedAttributes != null && opportunityAssignedAttributes.Any(c =>
                        caseAttributeNameList.Any(f => c.Contains(f, StringComparison.OrdinalIgnoreCase)) ||
                        opportunityAssignedSkuTerms?.Any(s =>
                            caseAttributeNameList.Any(f => s.StartsWith(f, StringComparison.OrdinalIgnoreCase))) ==
                        true))
                    opportunitiesFilteredByCaseAttributes.Add(opportunity);
            }

            var opportunitiesNotFilteredByCaseAttributes =
                opportunities.Except(opportunitiesFilteredByCaseAttributes).ToList();

            var opportunitiesFilteredByResourceServiceLines = FilterOpportunitiesByResourceServiceLines(filterObj, opportunitiesNotFilteredByCaseAttributes);

            var opportunitiesFilteredByCaseAttributesAndResourceServiceLines =
                opportunitiesFilteredByCaseAttributes.Union(await opportunitiesFilteredByResourceServiceLines);

            return opportunitiesFilteredByCaseAttributesAndResourceServiceLines.OrderBy(x => x.StartDate).ThenBy(y => y.ClientName).ToList();
        }

        private async Task<IEnumerable<OpportunityData>> FilterOpportunitiesByResourceServiceLines(DemandFilterCriteria filterObj, IList<OpportunityData> opportunities)
        {
            if (opportunities.Count <= 0)
                return Enumerable.Empty<OpportunityData>();

            var caseAttributeNameList = filterObj.CaseAttributeNames;
            var listPipelineId = string.Join(",", opportunities.Select(x => x.PipelineId.ToString()));
            var taggedPipelineIds = await
                _staffingApiClient.GetOpportunitiesByResourceServiceLines(listPipelineId, caseAttributeNameList);
            var opportunitiesFilteredByResourceServiceLines =
                opportunities.Where(o => taggedPipelineIds.Contains(o.PipelineId.ToString()));

            return opportunitiesFilteredByResourceServiceLines;
        }

        private IList<OpportunityData> FilterOpportunitiesByStaffFromSupplyFilter(DemandFilterCriteria filterObj, IEnumerable<Guid?> planningBoardPipelineIds, IList<OpportunityData> opportunities)
        {
            if (filterObj.IsStaffedFromSupply)
            {
                opportunities = opportunities?.Where(x => planningBoardPipelineIds.Contains(x.PipelineId)).ToList();
            }
            return opportunities;
        }

        private static DateTime? CalculateLikelyEndDate(string duration, DateTime? startDate)
        {
            if (string.IsNullOrEmpty(duration))
                return null;
            /*
             * Duration consists of months and weeks in the form of
             * 4.25 --> 4 months and 1 week 
             */
            var durationSplit = duration.Split('.');
            var months = durationSplit[0];
            var endDate = startDate ?? DateTime.Now;
            if (!string.IsNullOrEmpty(months))
                endDate = endDate.AddMonths(Convert.ToInt32(months));
            var weeksCode = durationSplit[1];
            var weeks = 0;
            switch (weeksCode)
            {
                case "25":
                    weeks = 1;
                    break;
                case "50":
                    weeks = 2;
                    break;
                case "75":
                    weeks = 3;
                    break;
                default:
                    weeks = 0;
                    break;
            }

            endDate = endDate.AddDays(weeks * 7);
            return endDate;
        }

        #endregion
    }
}