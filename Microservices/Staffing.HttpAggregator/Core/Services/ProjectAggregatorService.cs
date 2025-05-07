using Microsoft.Graph.Models.Security;
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
    public class ProjectAggregatorService : IProjectAggregatorService
    {
        private readonly IOpportunityService _opportunityService;
        private readonly ICaseService _caseService;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IRevenueApiClient _revenueApiClient;
        private readonly IBasisApiClient _basisApiClient;
        private readonly ISignalRHubClient _signalRHubClient;

        public ProjectAggregatorService(IOpportunityService opportunityService, ICaseService caseService, IStaffingApiClient staffingApiClient,
            IResourceApiClient resourceApiClient, IRevenueApiClient revenueApiClient, IBasisApiClient basisApiClient,
            ICCMApiClient ccmApiClient, ISignalRHubClient signalRHubClient)
        {
            _opportunityService = opportunityService;
            _caseService = caseService;
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _revenueApiClient = revenueApiClient;
            _basisApiClient = basisApiClient;
            _ccmApiClient = ccmApiClient;
            _signalRHubClient = signalRHubClient;
        }
        public async Task<IEnumerable<ProjectData>> GetOpportunitiesAndCasesWithAllocationsBySelectedValues(DemandFilterCriteria filterCriteria, string loggedInUser = null)
        {
            var projects = new List<ProjectData>();

            // Get All Employees including terminated as user can see back dated allocation on a case
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var revenueByServiceLinesTask = Task.FromResult<IList<Revenue>>(null);
            var staffingAllocationsBySupplyCriteriaTask = Task.FromResult<IList<ResourceAssignmentViewModel>>(null);
            var pipelineChangesInDateRangeTask = Task.FromResult<IList<CaseOppChanges>>(null);
            var caseChangesInDateRangeTask = Task.FromResult<IList<CaseOppChanges>>(null);

            var casePlanningBoardDataByProjectEndDateAndBucketIdsTask = Task.FromResult(Enumerable.Empty<CasePlanningBoard>());

            //this parameter refers to cases that are added to staffed by supply row on case planning board
            if (filterCriteria.IsStaffedFromSupply)
            {
                casePlanningBoardDataByProjectEndDateAndBucketIdsTask = _staffingApiClient.GetCasePlanningBoardDataByProjectEndDateAndBucketIds(filterCriteria.StartDate, "1");
            }

            if (filterCriteria.CaseAttributeNames.Length > 0)
            {
                var firstDayOfStartDate = new DateTime(filterCriteria.StartDate.Year, filterCriteria.StartDate.Month, 1);
                var lastDayOfEndDate = new DateTime(filterCriteria.EndDate.Year, filterCriteria.EndDate.Month, DateTime.DaysInMonth(filterCriteria.EndDate.Year, filterCriteria.EndDate.Month));
                revenueByServiceLinesTask = _revenueApiClient.GetRevenueByServiceLine(filterCriteria.CaseAttributeNames, firstDayOfStartDate, lastDayOfEndDate);
            }

            if (filterCriteria.DemandTypes.Contains(Constants.DemandType.CasesStaffedBySupply) && filterCriteria.supplyFilterCriteria != null)
            {
                var supplyCriteria = filterCriteria.supplyFilterCriteria;
                staffingAllocationsBySupplyCriteriaTask = _staffingApiClient.GetResourceAllocationsBySelectedSupplyValues(supplyCriteria.OfficeCodes, supplyCriteria.StartDate, supplyCriteria.EndDate,
                    supplyCriteria.StaffingTags, supplyCriteria.LevelGrades);
            }

            if (filterCriteria.CaseTypeCodes.Split(',').Contains(((int)Constants.CaseType.Billable).ToString()) && filterCriteria.DemandTypes.Contains(Constants.DemandType.Opportunity))
            {
                // Getting all the opportunities that were updated in BOSS and their updated start or end date lies in the date range;
                pipelineChangesInDateRangeTask = _staffingApiClient.GetPipelineChangesByDateRange(filterCriteria.StartDate, filterCriteria.EndDate);
            }

            // Get changes done on the case like notes
            caseChangesInDateRangeTask = _staffingApiClient.GetCaseChangesByDateRange(filterCriteria.StartDate, filterCriteria.EndDate);
            await Task.WhenAll(employeesIncludingTerminatedTask, skuTermsDataTask, officesTask, investmentCategoriesTask, revenueByServiceLinesTask,
                staffingAllocationsBySupplyCriteriaTask, pipelineChangesInDateRangeTask, caseChangesInDateRangeTask, casePlanningBoardDataByProjectEndDateAndBucketIdsTask);

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var offices = officesTask.Result;
            var skuTerms = skuTermsDataTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var revenueByServiceLines = revenueByServiceLinesTask.Result;
            var staffingAllocationsBySupplyCriteria = staffingAllocationsBySupplyCriteriaTask.Result;
            var pipelineChangesInDateRange = pipelineChangesInDateRangeTask.Result;
            var caseChangesInDateRange = caseChangesInDateRangeTask.Result;


            var casePlanningBoardDataByProjectEndDateAndBucketIds = casePlanningBoardDataByProjectEndDateAndBucketIdsTask.Result;

            var planningBoardOldCaseCodes = new List<string>();
            var planningBoardPipelineIds = new List<Guid?>();

            if (filterCriteria.IsStaffedFromSupply)
            {
                planningBoardOldCaseCodes = casePlanningBoardDataByProjectEndDateAndBucketIds.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).Select(x => x.OldCaseCode)?.ToList();
                planningBoardPipelineIds = casePlanningBoardDataByProjectEndDateAndBucketIds.Where(x => string.IsNullOrEmpty(x.OldCaseCode) &&
                                                            (x.PipelineId != null && x.PipelineId != Guid.Empty)).Select(x => x.PipelineId)?.ToList();
            }

            if (staffingAllocationsBySupplyCriteria != null)
                staffingAllocationsBySupplyCriteria =
                    await GetStaffingAllocationsBySupplyCriteriaFilteredByAdditionalFilters(staffingAllocationsBySupplyCriteria, filterCriteria.supplyFilterCriteria);

            var lstPipelineIdsOfOppsUpdatedInBOSSInDateRange = pipelineChangesInDateRange?.Count() > 0
                ? string.Join(",", pipelineChangesInDateRange?.Select(x => x.PipelineId).Distinct())
                : "";

            var lstCasesUpdatedInBOSSInDateRange = caseChangesInDateRange?.Count() > 0
                ? string.Join(",", caseChangesInDateRange?.Select(x => x.OldCaseCode).Distinct())
                : "";

            var revenueByServiceLinesOpps = new List<Revenue>();
            var revenueByServiceLinesCases = new List<Revenue>();

            if (revenueByServiceLines != null && revenueByServiceLines.Count() > 0)
            {
                revenueByServiceLinesOpps = revenueByServiceLines.Where(
                        x => x.CaseCode == 0 && x.OpportunityId != null).GroupBy(x => x.OpportunityId).Select(x => x.First()).ToList();
                revenueByServiceLinesCases = revenueByServiceLines.Where(
                        x => x.CaseCode > 0).ToList();
            }

            if (filterCriteria.ProjectStartIndex > 1)
            {
                //if page is scrolled and none of active demand types are selected, then return empty
                if (!IsAnyCaseTypeDemandSelected(filterCriteria))
                {
                    return Enumerable.Empty<ProjectData>();
                }
                else
                {
                    var cases = await _caseService.GetActiveCasesExceptNewDemandsAndAllocationsByOffices(filterCriteria,
                    employeesIncludingTerminated, offices, skuTerms, investmentCategories,
                    revenueByServiceLinesCases, staffingAllocationsBySupplyCriteria?.Where(x => x.OldCaseCode != null), lstCasesUpdatedInBOSSInDateRange, planningBoardOldCaseCodes);
                    var paginatedCases = cases.Skip(filterCriteria.ProjectStartIndex - 1).Take(filterCriteria.PageSize).ToList();

                    projects.AddRange(ConvertCaseToProjectModel(paginatedCases, employeesIncludingTerminated));
                    return projects;
                }
                
            }

            var casesDataTask = Task.FromResult(Enumerable.Empty<CaseData>());


            var opportunitiesDataTask = _opportunityService.GetOpportunities(filterCriteria, employeesIncludingTerminated, offices, skuTerms, investmentCategories,
                revenueByServiceLinesOpps, staffingAllocationsBySupplyCriteria?.Where(x => x.OldCaseCode == null && x.PipelineId != null),
                lstPipelineIdsOfOppsUpdatedInBOSSInDateRange, planningBoardPipelineIds, loggedInUser);


            var newDemandsDataTask = _caseService.GetNewDemandCasesAndAllocationsByOffices(filterCriteria, employeesIncludingTerminated, offices, skuTerms,
                investmentCategories, revenueByServiceLinesCases, staffingAllocationsBySupplyCriteria?.Where(x => x.OldCaseCode != null), lstCasesUpdatedInBOSSInDateRange,
                planningBoardOldCaseCodes, loggedInUser);

            if (IsAnyCaseTypeDemandSelected(filterCriteria))
            {
                casesDataTask = _caseService.GetActiveCasesExceptNewDemandsAndAllocationsByOffices(filterCriteria, employeesIncludingTerminated, offices, skuTerms,
                        investmentCategories, revenueByServiceLinesCases, staffingAllocationsBySupplyCriteria?.Where(x => x.OldCaseCode != null), lstCasesUpdatedInBOSSInDateRange,
                        planningBoardOldCaseCodes, loggedInUser);
            }

            await Task.WhenAll(opportunitiesDataTask, newDemandsDataTask, casesDataTask);

            var opportunities = opportunitiesDataTask.Result;
            var newDemands = newDemandsDataTask.Result;
            var allCases = casesDataTask.Result;

            var casesStartingPages = allCases.Skip(filterCriteria.ProjectStartIndex - 1).Take(filterCriteria.PageSize).ToList();
            // Get unique cases as user might have pinned New demands and it will treat as case bases on what date user has selected
            var caseAndNewDemands = newDemands.ToList();
            caseAndNewDemands.AddRange(casesStartingPages);

            var caseAndNewDemandsDistinct = caseAndNewDemands.GroupBy(c => c.OldCaseCode).Select(g => g.FirstOrDefault());

            var pipelineIds = string.Join(",", opportunities.Select(c => c.PipelineId));
            var oldCaseCodes = string.Join(",", caseAndNewDemandsDistinct.Select(c => c.OldCaseCode));

            var projectDetails = await _staffingApiClient.GetCasePlanningProjectDetails(oldCaseCodes, pipelineIds, null);

            projects.AddRange(ConvertOpportunityToProjectModel(opportunities, employeesIncludingTerminated, projectDetails));
            projects.AddRange(ConvertCaseToProjectModel(caseAndNewDemandsDistinct, employeesIncludingTerminated, projectDetails));

            return projects;

        }

        public async Task<ProjectDataViewModel> GetOpportunitiesAndNewDemandWithAllocationsBySelectedValues(DemandFilterCriteria filterCriteria, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterCriteria.OfficeCodes))
            {
                return new ProjectDataViewModel();
            }
            var projects = new List<ProjectData>();
            var pinnedProjects = new List<ProjectData>();
            var hiddenProjects = new List<ProjectData>();
            var staffingAllocationsBySupplyCriteriaTask = Task.FromResult<IList<ResourceAssignmentViewModel>>(null);

            // Get All Employees including terminated as user can see back dated allocation on a case
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var revenueByServiceLinesTask = Task.FromResult<IList<Revenue>>(null);
            var caseChangesInDateRangeTask = Task.FromResult<IList<CaseOppChanges>>(null);
            var casePlanningBoardDataByProjectEndDateAndBucketIdsTask = Task.FromResult(Enumerable.Empty<CasePlanningBoard>()); 
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();
            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();

            //this parameter refers to cases that are added to staffed by supply row on case planning board
            if (filterCriteria.IsStaffedFromSupply)
            {
                casePlanningBoardDataByProjectEndDateAndBucketIdsTask = _staffingApiClient.GetCasePlanningBoardDataByProjectEndDateAndBucketIds(filterCriteria.StartDate, "1");
            }

            if (filterCriteria.CaseAttributeNames.Length > 0)
            {
                var firstDayOfStartDate = new DateTime(filterCriteria.StartDate.Year, filterCriteria.StartDate.Month, 1);
                var lastDayOfEndDate = new DateTime(filterCriteria.EndDate.Year, filterCriteria.EndDate.Month, DateTime.DaysInMonth(filterCriteria.EndDate.Year, filterCriteria.EndDate.Month));
                revenueByServiceLinesTask = _revenueApiClient.GetRevenueByServiceLine(filterCriteria.CaseAttributeNames, firstDayOfStartDate, lastDayOfEndDate);
            }

            if (filterCriteria.DemandTypes.Contains(Constants.DemandType.CasesStaffedBySupply) && filterCriteria.supplyFilterCriteria != null)
            {
                var supplyCriteria = filterCriteria.supplyFilterCriteria;
                staffingAllocationsBySupplyCriteriaTask = _staffingApiClient.GetResourceAllocationsBySelectedSupplyValues(supplyCriteria.OfficeCodes, supplyCriteria.StartDate, supplyCriteria.EndDate,
                    supplyCriteria.StaffingTags, supplyCriteria.LevelGrades);
            }


            // Get changes done on the case like notes
            caseChangesInDateRangeTask = _staffingApiClient.GetCaseChangesByDateRange(filterCriteria.StartDate, filterCriteria.EndDate);

            await Task.WhenAll(employeesIncludingTerminatedTask, skuTermsDataTask, officesTask, investmentCategoriesTask, revenueByServiceLinesTask,
               caseChangesInDateRangeTask, casePlanningBoardDataByProjectEndDateAndBucketIdsTask, caseRoleTypeListTask, commitmentTypeListTask,
               caseAttributeLookupListTask, staffingAllocationsBySupplyCriteriaTask);

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var offices = officesTask.Result;
            var skuTerms = skuTermsDataTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var revenueByServiceLines = revenueByServiceLinesTask.Result;
            var caseChangesInDateRange = caseChangesInDateRangeTask.Result;
            var casePlanningBoardDataByProjectEndDateAndBucketIds = casePlanningBoardDataByProjectEndDateAndBucketIdsTask.Result;
            var caseAttributeLookupList = caseAttributeLookupListTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;
            var staffingAllocationsBySupplyCriteria = staffingAllocationsBySupplyCriteriaTask.Result;

            var planningBoardOldCaseCodes = casePlanningBoardDataByProjectEndDateAndBucketIds?.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).Select(x => x.OldCaseCode)?.ToList();
            var planningBoardPipelineIds = casePlanningBoardDataByProjectEndDateAndBucketIds?.Where(x => string.IsNullOrEmpty(x.OldCaseCode) &&
                                                            (x.PipelineId != null && x.PipelineId != Guid.Empty)).Select(x => x.PipelineId)?.ToList();

            var lstCasesUpdatedInBOSSInDateRange = caseChangesInDateRange?.Count() > 0
                ? string.Join(",", caseChangesInDateRange?.Select(x => x.OldCaseCode).Distinct())
                : "";

            var revenueByServiceLinesOpps = new List<Revenue>();
            var revenueByServiceLinesCases = new List<Revenue>();

            if (revenueByServiceLines != null && revenueByServiceLines.Count() > 0)
            {
                revenueByServiceLinesOpps = revenueByServiceLines.Where(
                        x => x.CaseCode == 0 && x.OpportunityId != null).GroupBy(x => x.OpportunityId).Select(x => x.First()).ToList();
                revenueByServiceLinesCases = revenueByServiceLines.Where(
                        x => x.CaseCode > 0).ToList();
            }

            if (staffingAllocationsBySupplyCriteria != null)
                staffingAllocationsBySupplyCriteria =
                    await GetStaffingAllocationsBySupplyCriteriaFilteredByAdditionalFilters(staffingAllocationsBySupplyCriteria, filterCriteria.supplyFilterCriteria);

            var opportunitiesDataTask = _opportunityService.GetOpportunitiesForNewStaffingTab(filterCriteria, staffingAllocationsBySupplyCriteria?.Where(x => x.OldCaseCode == null && x.PipelineId != null), employeesIncludingTerminated, offices, skuTerms, investmentCategories,
                    revenueByServiceLinesOpps, planningBoardPipelineIds, caseAttributeLookupList, caseRoleTypeList, commitmentTypeList, loggedInUser);

            var pinnedOpportunitiesDataTask = _opportunityService.GetPinnedOpportunitiesDetails(filterCriteria, filterCriteria.OpportunityExceptionShowList, employeesIncludingTerminated, offices, skuTerms, investmentCategories,
                    revenueByServiceLinesOpps, planningBoardPipelineIds, caseAttributeLookupList, caseRoleTypeList, commitmentTypeList, loggedInUser);

            var hiddenOpportunitiesDataTask = _opportunityService.GetHiddenOpportunitiesDetails(filterCriteria, employeesIncludingTerminated, offices, skuTerms, investmentCategories,
                    revenueByServiceLinesOpps, planningBoardPipelineIds, caseAttributeLookupList, caseRoleTypeList, commitmentTypeList, loggedInUser);

            var newDemandsDataTask = _caseService.GetNewDemandCasesAndAllocationsByOfficesForNewStaffingTab(filterCriteria, staffingAllocationsBySupplyCriteria?.Where(x => x.OldCaseCode != null),employeesIncludingTerminated, offices, skuTerms,
                    investmentCategories, revenueByServiceLinesCases, lstCasesUpdatedInBOSSInDateRange,
                    planningBoardOldCaseCodes, loggedInUser);

            var pinnedCasesDatatask = _caseService.GetPinnedCasesDetails(filterCriteria, filterCriteria.CaseExceptionShowList, employeesIncludingTerminated, offices, skuTerms,
                    investmentCategories, revenueByServiceLinesCases, lstCasesUpdatedInBOSSInDateRange,
                    planningBoardOldCaseCodes, loggedInUser);

            var hiddenCasesDatatask = _caseService.GetHiddenCasesDetails(filterCriteria, employeesIncludingTerminated, offices, skuTerms,
                    investmentCategories, revenueByServiceLinesCases, lstCasesUpdatedInBOSSInDateRange,
                    planningBoardOldCaseCodes, loggedInUser);

            await Task.WhenAll(opportunitiesDataTask, pinnedOpportunitiesDataTask, hiddenOpportunitiesDataTask, newDemandsDataTask,
                pinnedCasesDatatask, hiddenCasesDatatask);

            var opportunities = opportunitiesDataTask.Result;
            var pinnedOpportunities = pinnedOpportunitiesDataTask.Result;
            var hiddenOpportunities = hiddenOpportunitiesDataTask.Result;
            var newDemands = newDemandsDataTask.Result;
            var pinnedCases = pinnedCasesDatatask.Result;
            var hiddenCases = hiddenCasesDatatask.Result;

            var caseAndNewDemands = newDemands.ToList();
            var caseAndNewDemandsDistinct = caseAndNewDemands.GroupBy(c => c.OldCaseCode).Select(g => g.FirstOrDefault());

            projects.AddRange(ConvertOpportunityToProjectModel(opportunities, employeesIncludingTerminated));
            projects.AddRange(ConvertCaseToProjectModel(caseAndNewDemandsDistinct, employeesIncludingTerminated));
           

            pinnedProjects.AddRange(ConvertOpportunityToProjectModel(pinnedOpportunities, employeesIncludingTerminated));
            pinnedProjects.AddRange(ConvertCaseToProjectModel(pinnedCases, employeesIncludingTerminated));

            hiddenProjects.AddRange(ConvertOpportunityToProjectModel(hiddenOpportunities, employeesIncludingTerminated));
            hiddenProjects.AddRange(ConvertCaseToProjectModel(hiddenCases, employeesIncludingTerminated));

            var projectsViewModel = new ProjectDataViewModel
            {
                projects = projects,
                pinnedProjects = pinnedProjects,
                hiddenProjects = hiddenProjects
            };

            return projectsViewModel;
        }

        public async Task<ProjectData> GetOpportunityDetailsWithAllocationByPipelineId(string pipelineId, string loggedInUser)
        {
            // Get All Employees including terminated as user can see back dated allocation on a case
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();
            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();            

            await Task.WhenAll(employeesIncludingTerminatedTask, skuTermsDataTask, officesTask, investmentCategoriesTask,
               caseRoleTypeListTask, commitmentTypeListTask, caseAttributeLookupListTask);

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var offices = officesTask.Result;
            var skuTerms = skuTermsDataTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var caseAttributeLookupList = caseAttributeLookupListTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            var opportunityDetailsWithAllocations = await _opportunityService.GetPinnedOpportunitiesDetails(null, pipelineId, 
                employeesIncludingTerminated, offices, skuTerms, investmentCategories,null, null,
                caseAttributeLookupList, caseRoleTypeList, commitmentTypeList, loggedInUser);
            var opportunityDetails = ConvertOpportunityToProjectModel(opportunityDetailsWithAllocations, employeesIncludingTerminated);
            return opportunityDetails.FirstOrDefault();

        }

        public async Task<ProjectData> GetCaseDetailsWithAllocationByCaseCode(string oldCaseCode, string loggedInUser)
        {
            // Get All Employees including terminated as user can see back dated allocation on a case
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();
            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();

            await Task.WhenAll(employeesIncludingTerminatedTask, skuTermsDataTask, officesTask, investmentCategoriesTask,
               caseRoleTypeListTask, commitmentTypeListTask, caseAttributeLookupListTask);

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var offices = officesTask.Result;
            var skuTerms = skuTermsDataTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;

            var caseDetailsWithAllocations = await _caseService.GetPinnedCasesDetails(null, oldCaseCode,
                employeesIncludingTerminated, offices, skuTerms, investmentCategories, null, null,null,
                loggedInUser);
            var caseDetails = ConvertCaseToProjectModel(caseDetailsWithAllocations, employeesIncludingTerminated);
            return caseDetails.FirstOrDefault();

        }

        private static bool IsAnyCaseTypeDemandSelected(DemandFilterCriteria filterCriteria)
        {
            var caseTypes = new[] { Constants.DemandType.ActiveCase, Constants.DemandType.CaseEnding, Constants.DemandType.StaffedCase, Constants.DemandType.CasesStaffedBySupply };
            return caseTypes.Any(caseType => filterCriteria.DemandTypes.Contains(caseType));
        }


        private async Task<IList<ResourceAssignmentViewModel>> GetStaffingAllocationsBySupplyCriteriaFilteredByAdditionalFilters(
            IList<ResourceAssignmentViewModel> staffingAllocationsBySupplyCriteria, SupplyFilterCriteria supplyFilterCriteria)
        {
            string unfilteredStaffingAllocationsEmployeeCodes = string.Join(',', staffingAllocationsBySupplyCriteria.Select(x => x.EmployeeCode));
            var employeeWithPracticeAreaAffiliationsTask = GetEmployeesWithAffiliations(unfilteredStaffingAllocationsEmployeeCodes, supplyFilterCriteria.PracticeAreaCodes, string.Empty);

            var employeesFilteredBySupplyCriteriaTask = GetResourcesFilteredBySupplyCriteria(supplyFilterCriteria);

            await Task.WhenAll(employeeWithPracticeAreaAffiliationsTask, employeesFilteredBySupplyCriteriaTask);

            var employeesWithPracticeAreaAffiliations = employeeWithPracticeAreaAffiliationsTask.Result;
            var employeesFilteredBySupplyCriteria = employeesFilteredBySupplyCriteriaTask.Result;

            var filterStaffingAllocationsByEmployeePosition = FilterStaffingAllocationsByEmployeePosition(staffingAllocationsBySupplyCriteria, employeesFilteredBySupplyCriteria);

            var filteredStaffingAllocationsBySupplyCriteria = FilterStaffingAllocationsBySelectedPracticeAreaAffiliations(supplyFilterCriteria.PracticeAreaCodes, employeesWithPracticeAreaAffiliations, filterStaffingAllocationsByEmployeePosition);

            return filteredStaffingAllocationsBySupplyCriteria;
        }

        private IList<ResourceAssignmentViewModel> FilterStaffingAllocationsBySelectedPracticeAreaAffiliations(string practiceAreaCodes,
            IEnumerable<EmployeePracticeArea> employeesWithPracticeAreaAffiliations,
            IList<ResourceAssignmentViewModel> staffingAllocationsBySupplyCriteria)
        {
            if (string.IsNullOrEmpty(practiceAreaCodes))
                return staffingAllocationsBySupplyCriteria;

            var employeesWithSelectedPracticeArea = string.Join(',', employeesWithPracticeAreaAffiliations.Select(x => x.EmployeeCode));
            staffingAllocationsBySupplyCriteria = staffingAllocationsBySupplyCriteria.Where(x => employeesWithSelectedPracticeArea.Contains(x.EmployeeCode)).ToList();

            return staffingAllocationsBySupplyCriteria;
        }
        private Task<IEnumerable<EmployeePracticeArea>> GetEmployeesWithAffiliations(string employeeCodes, string practiceAreaCodes, string affiliationRoleCodes)
        {
            //Passing empty in the role Code because home screen does not need filtering by role codes
            return !string.IsNullOrEmpty(practiceAreaCodes) ? _basisApiClient.GetPracticeAreaAffiliationsByEmployeeCodes(employeeCodes, practiceAreaCodes, string.Empty)
                : Task.FromResult<IEnumerable<EmployeePracticeArea>>(new List<EmployeePracticeArea>());
        }

        public async Task<ProjectDataViewModel> GetOnGoingCasesWithAllocationsBySelectedValues(DemandFilterCriteria filterCriteria, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(filterCriteria.OfficeCodes))
            {
                return new ProjectDataViewModel();
            }
            var projects = new List<ProjectData>();
            var pinnedProjects = new List<ProjectData>();
            var hiddenProjects = new List<ProjectData>();
            var staffingAllocationsBySupplyCriteriaTask = Task.FromResult<IList<ResourceAssignmentViewModel>>(null);
            // Get All Employees including terminated as user can see back dated allocation on a case
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var revenueByServiceLinesTask = Task.FromResult<IList<Revenue>>(null);
            var caseChangesInDateRangeTask = Task.FromResult<IList<CaseOppChanges>>(null);
            var casePlanningBoardDataByProjectEndDateAndBucketIdsTask = Task.FromResult(Enumerable.Empty<CasePlanningBoard>());
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();
            var caseAttributeLookupListTask = _ccmApiClient.GetCaseAttributeLookupList();

            //this parameter refers to cases that are added to staffed by supply row on case planning board
            if (filterCriteria.IsStaffedFromSupply)
            {
                casePlanningBoardDataByProjectEndDateAndBucketIdsTask = _staffingApiClient.GetCasePlanningBoardDataByProjectEndDateAndBucketIds(filterCriteria.StartDate, "1");
            }

            if (filterCriteria.CaseAttributeNames.Length > 0)
            {
                var firstDayOfStartDate = new DateTime(filterCriteria.StartDate.Year, filterCriteria.StartDate.Month, 1);
                var lastDayOfEndDate = new DateTime(filterCriteria.EndDate.Year, filterCriteria.EndDate.Month, DateTime.DaysInMonth(filterCriteria.EndDate.Year, filterCriteria.EndDate.Month));
                revenueByServiceLinesTask = _revenueApiClient.GetRevenueByServiceLine(filterCriteria.CaseAttributeNames, firstDayOfStartDate, lastDayOfEndDate);
            }

            if (filterCriteria.DemandTypes.Contains(Constants.DemandType.CasesStaffedBySupply) && filterCriteria.supplyFilterCriteria != null)
            {
                var supplyCriteria = filterCriteria.supplyFilterCriteria;
                staffingAllocationsBySupplyCriteriaTask = _staffingApiClient.GetResourceAllocationsBySelectedSupplyValues(supplyCriteria.OfficeCodes, supplyCriteria.StartDate, supplyCriteria.EndDate,
                    supplyCriteria.StaffingTags, supplyCriteria.LevelGrades);
            }

            // Get changes done on the case like notes
            caseChangesInDateRangeTask = _staffingApiClient.GetCaseChangesByDateRange(filterCriteria.StartDate, filterCriteria.EndDate);

            await Task.WhenAll(employeesIncludingTerminatedTask, skuTermsDataTask, officesTask, investmentCategoriesTask, revenueByServiceLinesTask,
               caseChangesInDateRangeTask, casePlanningBoardDataByProjectEndDateAndBucketIdsTask, caseRoleTypeListTask, commitmentTypeListTask,
               caseAttributeLookupListTask, staffingAllocationsBySupplyCriteriaTask);

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var offices = officesTask.Result;
            var skuTerms = skuTermsDataTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var revenueByServiceLines = revenueByServiceLinesTask.Result;
            var caseChangesInDateRange = caseChangesInDateRangeTask.Result;
            var casePlanningBoardDataByProjectEndDateAndBucketIds = casePlanningBoardDataByProjectEndDateAndBucketIdsTask.Result;
            var caseAttributeLookupList = caseAttributeLookupListTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;
            var staffingAllocationsBySupplyCriteria = staffingAllocationsBySupplyCriteriaTask.Result;

            var planningBoardOldCaseCodes = casePlanningBoardDataByProjectEndDateAndBucketIds?.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).Select(x => x.OldCaseCode)?.ToList();
            var planningBoardPipelineIds = casePlanningBoardDataByProjectEndDateAndBucketIds?.Where(x => string.IsNullOrEmpty(x.OldCaseCode) &&
                                                            (x.PipelineId != null && x.PipelineId != Guid.Empty)).Select(x => x.PipelineId)?.ToList();

            var lstCasesUpdatedInBOSSInDateRange = caseChangesInDateRange?.Count() > 0
                ? string.Join(",", caseChangesInDateRange?.Select(x => x.OldCaseCode).Distinct())
                : "";

            var revenueByServiceLinesOpps = new List<Revenue>();
            var revenueByServiceLinesCases = new List<Revenue>();
            Task<IEnumerable<CaseData>> pinnedCasesDatatask = Task.FromResult(Enumerable.Empty<CaseData>());
            Task<IEnumerable<CaseData>> newDemandsDataTask = Task.FromResult(Enumerable.Empty<CaseData>());

            if (revenueByServiceLines != null && revenueByServiceLines.Count() > 0)
            {
                revenueByServiceLinesOpps = revenueByServiceLines.Where(
                        x => x.CaseCode == 0 && x.OpportunityId != null).GroupBy(x => x.OpportunityId).Select(x => x.First()).ToList();
                revenueByServiceLinesCases = revenueByServiceLines.Where(
                        x => x.CaseCode > 0).ToList();
            }

            var ongoingCasesDataTask = _caseService.GetOnGoingCasesAndAllocationsByOfficesForNewStaffingtab(filterCriteria, staffingAllocationsBySupplyCriteria?.Where(x => x.OldCaseCode != null), employeesIncludingTerminated, offices, skuTerms,
                    investmentCategories, revenueByServiceLinesCases, lstCasesUpdatedInBOSSInDateRange,
                    planningBoardOldCaseCodes, loggedInUser);

            if (filterCriteria.ProjectStartIndex <= 1)
            {
                newDemandsDataTask = _caseService.GetNewDemandCasesAndAllocationsByOffices(filterCriteria, employeesIncludingTerminated, offices, skuTerms,
                investmentCategories, revenueByServiceLinesCases, null, lstCasesUpdatedInBOSSInDateRange,
                planningBoardOldCaseCodes, loggedInUser);

                pinnedCasesDatatask = _caseService.GetPinnedCasesDetails(filterCriteria, filterCriteria.CaseExceptionShowList, employeesIncludingTerminated, offices, skuTerms,
                    investmentCategories, revenueByServiceLinesCases, lstCasesUpdatedInBOSSInDateRange,
                    planningBoardOldCaseCodes, loggedInUser);
            }

            var hiddenCasesDatatask = _caseService.GetHiddenCasesDetails(filterCriteria, employeesIncludingTerminated, offices, skuTerms,
                    investmentCategories, revenueByServiceLinesCases, lstCasesUpdatedInBOSSInDateRange,
                    planningBoardOldCaseCodes, loggedInUser);

            await Task.WhenAll(newDemandsDataTask, ongoingCasesDataTask, pinnedCasesDatatask, hiddenCasesDatatask);

            var newDemand = newDemandsDataTask.Result;
            var ongoingCases = ongoingCasesDataTask.Result;
            var pinnedCases = pinnedCasesDatatask.Result;
            var hiddenCases = hiddenCasesDatatask.Result;


            IEnumerable<CaseData> ongoingAndNewDemandCases = ongoingCases.Concat(newDemand.Where(x => !ongoingCases.Select(y => y.OldCaseCode).Contains(x.OldCaseCode))).ToList();

            projects.AddRange(ConvertCaseToProjectModel(ongoingAndNewDemandCases, employeesIncludingTerminated));
            pinnedProjects.AddRange(ConvertCaseToProjectModel(pinnedCases, employeesIncludingTerminated));
            hiddenProjects.AddRange(ConvertCaseToProjectModel(hiddenCases, employeesIncludingTerminated));

            //STAF-5502: fix to make all ongoing cases as active case otherwise few were coming as new demand and showing up as orange in front-end. Need to think of permanent solution
            foreach (var item in projects)
            {
                item.Type = Constants.DemandType.ActiveCase;
            }
            foreach (var item in pinnedProjects)
            {
                item.Type = Constants.DemandType.ActiveCase;
            }

            var projectsViewModel = new ProjectDataViewModel
            {
                projects = projects,
                pinnedProjects = pinnedProjects,
                hiddenProjects = hiddenProjects
            };

            return projectsViewModel;
        }

        public async Task<IEnumerable<ProjectData>> GetProjectsForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
                return Enumerable.Empty<ProjectData>();

            var casesDataTask = _caseService.GetCasesForTypeahead(searchString);
            var opportunitiesDataTask = _opportunityService.GetOpportunitiesForTypeahead(searchString);

            await Task.WhenAll(casesDataTask, opportunitiesDataTask);

            var opportunities = opportunitiesDataTask.Result;
            var cases = casesDataTask.Result;

            var projects = new List<ProjectData>();
            projects.AddRange(ConvertOpportunityToProjectModel(opportunities));
            projects.AddRange(ConvertCaseToProjectModel(cases));

            return projects.OrderBy(o => o.ProjectStatus).ThenBy(o => o.ClientName).ThenByDescending(t => t.StartDate);
        }

        public async Task<IEnumerable<ProjectData>> GetCasesForTypeahead(string searchString)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
                return Enumerable.Empty<ProjectData>();

            var cases = await _caseService.GetCasesForTypeahead(searchString);

            var projects = new List<ProjectData>();
            projects.AddRange(ConvertCaseToProjectModel(cases));

            return projects.OrderBy(o => o.ProjectStatus).ThenBy(o => o.ClientName).ThenByDescending(t => t.StartDate);
        }

        public async Task<IEnumerable<PlanningCardViewModel>> GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(string employeeCode, string officeCodes, string staffingTags,
            bool isStaffedFromSupply, string loggedInUser = null)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return Enumerable.Empty<PlanningCardViewModel>();
            var bucketIds = isStaffedFromSupply
                ? ConfigurationUtility.GetValue("BucketIdsForStaffedFromMySupplyFilter")
                : null;

            // Get All Employees including terminated as user can see back dated allocation on a case
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var planningCardsTask = _staffingApiClient.GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(employeeCode, officeCodes, staffingTags, bucketIds);
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var commitmentTypesTask = _staffingApiClient.GetCommitmentTypeList();
            var officesTask = _ccmApiClient.GetOfficeList();

            //var casePlanningBoardDataByProjectEndDateAndBucketIdsTask = _staffingApiClient.GetCasePlanningBoardDataByProjectEndDateAndBucketIds(DateTime.MinValue, "1");


            await Task.WhenAll(employeesIncludingTerminatedTask, planningCardsTask, investmentCategoriesTask, commitmentTypesTask, officesTask);

            //var casePlanningBoardDataByProjectEndDateAndBucketIds = casePlanningBoardDataByProjectEndDateAndBucketIdsTask.Result;
            //var casePlanningBoardDataByProjectEndDateAndBucketIdsPlanningCards = casePlanningBoardDataByProjectEndDateAndBucketIds.Where(x => (x.PlanningCardId != null && x.PlanningCardId != Guid.Empty)).ToList();
            //var planningBoardPlanningCardIds = string.Join(",", casePlanningBoardDataByProjectEndDateAndBucketIdsPlanningCards.Select(x => x.PlanningCardId)?.Distinct());

            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var planningCards = planningCardsTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var commitmentTypes = commitmentTypesTask.Result;
            var offices = officesTask.Result;

            var lstPlanningCardIds = string.Join(',', planningCards.Select(x => x.Id.ToString()));

            var casePlanningViewNotes = Enumerable.Empty<CaseViewNote>();
            var staCommitmentDetailsTask = _staffingApiClient.GetProjectSTACommitmentDetails(null, null, lstPlanningCardIds);
            var projectDetailsTask =  _staffingApiClient.GetCasePlanningProjectDetails(null, null, lstPlanningCardIds);

            await Task.WhenAll(projectDetailsTask, staCommitmentDetailsTask);

            var projectDetails = projectDetailsTask.Result;
            var staCommitmentDetails = staCommitmentDetailsTask.Result;
            


            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotes = await _staffingApiClient.GetCaseViewNotesByPlanningCardIds(lstPlanningCardIds, loggedInUser);

            }
 

            var planningCardAndAllocations = ConvertPlanningCardToViewModel(planningCards, employeesIncludingTerminated, investmentCategories, commitmentTypes, offices, casePlanningViewNotes, projectDetails,staCommitmentDetails);


            return planningCardAndAllocations;

        }

        public async Task<CaseViewNoteViewModel> UpsertCaseViewNote(CaseViewNote caseViewNote)
        {
            var notesTask = _staffingApiClient.UpsertCaseViewNote(caseViewNote);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(notesTask, resourcesTask);

            var resources = resourcesTask.Result;
            var noteList = new List<CaseViewNote>();
            noteList.Add(notesTask.Result);

            var response = await _signalRHubClient.GetUpdateOnSharedNotes(caseViewNote.SharedWith);

            var upsertedNote = ConvertToCaseViewNotesViewModel(noteList, resources).FirstOrDefault();

            return upsertedNote;
        }

        public async Task<IEnumerable<CaseViewNoteViewModel>> GetCaseViewNote(string oldCaseCode, string piplineId, string planningCardId, string loggedInUser)
        {
            var caseViewNotesTask = _staffingApiClient.GetCaseViewNotes(oldCaseCode, piplineId, planningCardId, loggedInUser);

            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(caseViewNotesTask, resourcesDataTask);

            var caseViewNotes = caseViewNotesTask.Result;
            var resources = resourcesDataTask.Result;

            if (caseViewNotes.Count() <= 0)
            {
                return Enumerable.Empty<CaseViewNoteViewModel>();
            }

            return ConvertToCaseViewNotesViewModel(caseViewNotes, resources);
        }

        private IList<ResourceAssignmentViewModel> FilterStaffingAllocationsByEmployeePosition(IList<ResourceAssignmentViewModel> staffingAllocationsBySupplyCriteria, IEnumerable<Resource> employeesFilteredBySupplyCriteria)
        {
            if (employeesFilteredBySupplyCriteria == null)
            {
                return staffingAllocationsBySupplyCriteria;
            }

            var filteredResult = staffingAllocationsBySupplyCriteria.Join(employeesFilteredBySupplyCriteria,
                sa => sa.EmployeeCode,
                e => e.EmployeeCode,
                (allocation, employee) => allocation
                ).ToList();

            return filteredResult;
        }

        private Task<IEnumerable<Resource>> GetResourcesFilteredBySupplyCriteria(SupplyFilterCriteria supplyCriteria)
        {
            return string.IsNullOrEmpty(supplyCriteria.PositionCodes)
                ? Task.FromResult<IEnumerable<Resource>>(null)
                : _resourceApiClient.GetActiveEmployeesFilteredBySelectedValues(supplyCriteria.OfficeCodes, supplyCriteria.StartDate, supplyCriteria.EndDate, supplyCriteria.LevelGrades, supplyCriteria.PositionCodes);
        }
        private IList<PlanningCardViewModel> ConvertPlanningCardToViewModel(IEnumerable<PlanningCard> planningCards, IEnumerable<Resource> resources,
            IEnumerable<InvestmentCategory> investmentCategories, IEnumerable<CommitmentType> commitmentTypes, IEnumerable<Office> offices,
            IEnumerable<CaseViewNote> caseViewNotes, IEnumerable<CasePlanningProjectPreferences> projectDetails,  IEnumerable<CaseOppCommitmentViewModel> staCommitmentDetails = null)
        {
            var viewModel = planningCards.Select(item => new PlanningCardViewModel
            {
                Id = item.Id,
                Name = item.Name,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Office = item.Office,
                IsShared = item.IsShared,
                SharedOfficeCodes = item.SharedOfficeCodes,
                SharedOfficeAbbreviations = !string.IsNullOrEmpty(item.SharedOfficeCodes) ? GetOfficeAbbreviationListForPlanningCards(item.SharedOfficeCodes, offices) : null,
                SharedStaffingTags = item.SharedStaffingTags,
                IncludeInCapacityReporting = item.IncludeInCapacityReporting,
                PegOpportunityId = item.PegOpportunityId,
                ProbabilityPercent = item.ProbabilityPercent,
                IncludeInDemand = projectDetails.Where(x => x.PlanningCardId == item.Id).FirstOrDefault()?.IncludeInDemand,
                IsFlagged = projectDetails.Where(x => x.PlanningCardId == item.Id).FirstOrDefault()?.IsFlagged,
                CreatedBy = item.CreatedBy,
                MergedCaseCode = item.MergedCaseCode,
                isMerged = item.IsMerged,
                SKUTerm = item.SkuTerm,
                CombinedSkuTerm = CalculateCombinedSkuTerm(item.SkuTerm),
                allocations = ConvertToResourceAssignmentViewModel(item.allocations, resources, investmentCategories, commitmentTypes).ToList(),
                CasePlanningViewNotes = ConvertToCaseViewNotesViewModel(caseViewNotes.Where(x => x.PlanningCardId.Equals(item.Id)).ToList(), resources),
                isSTACommitmentCreated = staCommitmentDetails?.Any( x=> x.PlanningCardId == item.Id) ?? false
            }).ToList();

            return viewModel;
        }
        private string GetOfficeAbbreviationListForPlanningCards(string officeCodes, IEnumerable<Office> offices)
        {
            return string.Join(",", offices.Where(x => officeCodes.Contains(x.OfficeCode.ToString())).OrderBy(z => z.OfficeAbbreviation).Select(y => y.OfficeAbbreviation));
        }

        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations,
            IEnumerable<Resource> resources, IEnumerable<InvestmentCategory> investmentCategories, IEnumerable<CommitmentType> commitmentTypes)
        {
            if (placeholderAllocations == null)
                return Enumerable.Empty<ResourceAssignmentViewModel>();

            var resourceAllocationsViewModel = (from placeholderAllocation in placeholderAllocations
                                                join ic in investmentCategories on placeholderAllocation.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                                                from investmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                                                join res in resources on placeholderAllocation.EmployeeCode equals res.EmployeeCode into resAllocGroups
                                                from resource in resAllocGroups.DefaultIfEmpty()
                                                join ct in commitmentTypes on placeholderAllocation.CommitmentTypeCode equals ct.CommitmentTypeCode into resCommitmentTypeList
                                                from commitmentType in resCommitmentTypeList.DefaultIfEmpty()
                                                select new ResourceAssignmentViewModel
                                                {
                                                    Id = placeholderAllocation.Id,
                                                    PlanningCardId = placeholderAllocation.PlanningCardId,
                                                    OldCaseCode = placeholderAllocation.OldCaseCode,
                                                    CaseName = null,
                                                    CaseTypeCode = null,
                                                    ClientName = null,
                                                    PipelineId = placeholderAllocation.PipelineId,
                                                    OpportunityName = null,
                                                    EmployeeCode = placeholderAllocation.EmployeeCode,
                                                    EmployeeName = resource?.FullName,
                                                    InternetAddress = resource?.InternetAddress,
                                                    CurrentLevelGrade = placeholderAllocation.CurrentLevelGrade,
                                                    OperatingOfficeCode = placeholderAllocation.OperatingOfficeCode,
                                                    OperatingOfficeAbbreviation = resource?.SchedulingOffice?.OfficeAbbreviation,
                                                    Allocation = placeholderAllocation.Allocation,
                                                    StartDate = placeholderAllocation.StartDate,
                                                    EndDate = placeholderAllocation.EndDate,
                                                    InvestmentCode = placeholderAllocation.InvestmentCode,
                                                    InvestmentName = investmentCategory?.InvestmentName,
                                                    CaseRoleCode = placeholderAllocation.CaseRoleCode,
                                                    CaseStartDate = null,
                                                    CaseEndDate = null,
                                                    OpportunityStartDate = null,
                                                    OpportunityEndDate = null,
                                                    ServiceLineCode = placeholderAllocation.ServiceLineCode,
                                                    ServiceLineName = placeholderAllocation.ServiceLineName,
                                                    LastUpdatedBy = placeholderAllocation.LastUpdatedBy,
                                                    Notes = placeholderAllocation.Notes ?? "",
                                                    CommitmentTypeCode = placeholderAllocation.CommitmentTypeCode,
                                                    CommitmentTypeName = commitmentType?.CommitmentTypeName,
                                                    IsPlaceholderAllocation = placeholderAllocation.IsPlaceholderAllocation,
                                                    PositionGroupCode = placeholderAllocation?.PositionGroupCode
                                                }).ToList();

            return resourceAllocationsViewModel;
        }
        private IEnumerable<ProjectData> ConvertCaseToProjectModel(IEnumerable<CaseData> cases, List<Resource> resources = null, 
            IEnumerable<CasePlanningProjectPreferences> projectDetails = null)
        {
            var projects = cases.Select(item => new ProjectData
            {
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                CaseCode = item.CaseCode,
                CaseName = item.CaseName,
                OldCaseCode = item.OldCaseCode,
                CaseTypeCode = item.CaseTypeCode,
                CaseType = item.CaseType,
                CaseManagerCode = item.CaseManagerCode,
                CaseManagerFullName = item.CaseManagerName,
                CaseManagerOfficeAbbreviation = item.CaseManagerOfficeAbbreviation,
                ManagingOfficeCode = item.ManagingOfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeName = item.ManagingOfficeName,
                BillingOfficeCode = item.BillingOfficeCode,
                BillingOfficeAbbreviation = item.BillingOfficeAbbreviation,
                BillingOfficeName = item.BillingOfficeName,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Type = item.Type,
                CaseRoll = item.CaseRoll,
                IsPrivateEquity = item.IsPrivateEquity,
                CaseAttributes = item.CaseAttributes,
                AllocatedResources = item.AllocatedResources,
                PlaceholderAllocations = item.PlaceholderAllocations,
                SkuTerm = item.SkuTerm,
                CombinedSkuTerm = CalculateCombinedSkuTerm(item.SkuTerm),
                SkuCaseTerms = item.SKUCaseTerms,
                IncludeInDemand = projectDetails?.Where(x => x.OldCaseCode == item.OldCaseCode).FirstOrDefault()?.IncludeInDemand,
                IsFlagged = projectDetails?.Where(x => x.OldCaseCode == item.OldCaseCode).FirstOrDefault()?.IsFlagged,
                Notes = item.Notes,
                CaseServedByRingfence = item.CaseServedByRingfence,
                ProjectStatus = SetProjectStatus(item.EndDate),
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                CasePlanningViewNotes = resources == null
                                        ? Enumerable.Empty<CaseViewNoteViewModel>()
                                        : ConvertToCaseViewNotesViewModel(item.CasePlanningViewNotes, resources),
                PegOpportunityId = item.PegOpportunityId,
                isSTACommitmentCreated = item.isSTACommitmentCreated

            });

            return projects;
        }

        private IEnumerable<ProjectData> ConvertOpportunityToProjectModel(IEnumerable<OpportunityData> opportunities, List<Resource> resources = null, 
            IEnumerable<CasePlanningProjectPreferences> projectDetails = null)
        {
            var projects = opportunities.Select(item => new ProjectData
            {
                PipelineId = item.PipelineId,
                CortexId = item.CortexId,
                EstimatedTeamSize = item?.EstimatedTeamSize,
                PricingTeamSize = item?.PricingTeamSize,
                OpportunityName = item.OpportunityName,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                CoordinatingPartnerName = item.CoordinatingPartnerName,
                BillingPartnerCode = item.BillingPartnerCode,
                BillingPartnerName = item.BillingPartnerName,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeCode = item.ManagingOfficeCode,
                StartDate = item.StartDate,
                OriginalStartDate = item.OriginalStartDate,
                OverrideStartDate = item.OverrideStartDate,
                OriginalEndDate = item.OriginalEndDate,
                OverrideEndDate = item.OverrideEndDate,
                OriginalProbabilityPercent = item.OriginalProbabilityPercent,
                OverrideProbabilityPercent = item.OverrideProbabilityPercent,
                EndDate = item.EndDate,
                ProbabilityPercent = item.ProbabilityPercent,
                Type = item.Type,
                AllocatedResources = item.AllocatedResources,
                PlaceholderAllocations = item.PlaceholderAllocations,
                SkuTerm = item.SKUTerm,
                CombinedSkuTerm = CalculateCombinedSkuTerm(item.SKUTerm),
                SkuCaseTerms = item.SKUCaseTerms,
                IncludeInDemand = projectDetails?.Where(x => x.PipelineId == item.PipelineId).FirstOrDefault()?.IncludeInDemand,
                IsFlagged = projectDetails?.Where(x => x.PipelineId == item.PipelineId).FirstOrDefault()?.IsFlagged,
                ProjectStatus = SetProjectStatus(item.EndDate),
                CaseAttributes = item.CaseAttributes,
                Notes = item.Notes,
                CaseServedByRingfence = item.CaseServedByRingfence,
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                isStartDateUpdatedInBOSS = item.isStartDateUpdatedInBOSS,
                isEndDateUpdatedInBOSS = item.isEndDateUpdatedInBOSS,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                CasePlanningViewNotes = resources == null
                                        ? Enumerable.Empty<CaseViewNoteViewModel>()
                                        : ConvertToCaseViewNotesViewModel(item.CasePlanningViewNotes, resources),
                isSTACommitmentCreated = item.isSTACommitmentCreated
            });
            return projects;
        }

        private string CalculateCombinedSkuTerm(IEnumerable<SKUDemand> skuDemands)
        {
            var combinedSkuTerm = "";
            Dictionary<string, double> skuDictionary = new Dictionary<string, double>();
            if (skuDemands != null)
            {
                foreach (var skuDemand in skuDemands?.ToList())
                {
                    if (skuDemand.level == null)
                    {
                        continue;
                    }
                    if (!skuDictionary.ContainsKey(skuDemand.level))
                    {
                        skuDictionary.Add(skuDemand.level, Math.Round((float)skuDemand.AggregateDemand, 2));
                    }
                    else
                    {
                        skuDictionary[skuDemand.level] = Math.Round((skuDictionary[skuDemand.level] + (float)skuDemand.AggregateDemand), 2);
                    }
                }
            }

            //order the dictionary in descending order
            foreach (var sku in skuDictionary.OrderByDescending(item => item.Key))
            {
                combinedSkuTerm = combinedSkuTerm == "" ? $"{sku.Value}{sku.Key}"
                    : $"{combinedSkuTerm} + {sku.Value}{sku.Key}";
            }
            return combinedSkuTerm;
        }

        private IEnumerable<CaseViewNoteViewModel> ConvertToCaseViewNotesViewModel(IEnumerable<CaseViewNote> caseViewNotes, IEnumerable<Resource> resources)
        {
            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            IEnumerable<CaseViewNoteViewModel> caseViewNotesModel = caseViewNotes.Select(note => new CaseViewNoteViewModel
            {
                Id = note.Id,
                OldCaseCode = note?.OldCaseCode,
                PipelineId = note?.PipelineId,
                PlanningCardId = note?.PlanningCardId,
                Note = note.Note ?? "",
                IsPrivate = note.IsPrivate,
                SharedWith = note.SharedWith,
                SharedWithDetails = string.IsNullOrEmpty(note.SharedWith)
                                    ? new List<Resource>()
                                    : resources?.Where(x => note.SharedWith.Split(',').Contains(x.EmployeeCode)).ToList(),
                CreatedBy = note.CreatedBy,
                CreatedByName = resources.FirstOrDefault(x => x.EmployeeCode == note.CreatedBy).FullName,
                LastUpdatedBy = note.LastUpdatedBy,
                LastUpdated = TimeZoneInfo.ConvertTimeToUtc(note.LastUpdated, est)
            });

            return caseViewNotesModel;
        }

        private string SetProjectStatus(DateTime? date)
        {
            if (date == null || Convert.ToDateTime(date).Date >= DateTime.Now.Date)
            {
                return Convert.ToString(ProjectStatus.Active);
            }
            return Convert.ToString(ProjectStatus.Inactive);
        }
    }
}
