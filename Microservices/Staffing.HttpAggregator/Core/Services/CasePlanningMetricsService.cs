using Microsoft.ApplicationInsights.AspNetCore;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Core.Helpers;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Staffing.HttpAggregator.Core.Helpers.Constants;

namespace Staffing.HttpAggregator.Core.Services
{
    public class CasePlanningMetricsService : ICasePlanningMetricsService
    {
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IOpportunityService _opportunityService;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICaseService _caseService;
        private readonly IRevenueApiClient _revenueApiClient;
        private readonly IPlanningCardService _planningCardService;

        public CasePlanningMetricsService(ICCMApiClient ccmApiClient,
            IStaffingApiClient staffingApiClient, IOpportunityService opportunityService,
            IResourceApiClient resourceApiClient, ICaseService caseService, IRevenueApiClient revenueApiClient, IPlanningCardService planningCardService)
        {
            _ccmApiClient = ccmApiClient;
            _staffingApiClient = staffingApiClient;
            _opportunityService = opportunityService;
            _resourceApiClient = resourceApiClient;
            _caseService = caseService;
            _revenueApiClient = revenueApiClient;
            _planningCardService = planningCardService;
        }

        //todo : remove this code as this end point is no longer used
        //public async Task<IEnumerable<CasePlanningBoardColumn>> GetCasePlanningBoardData(DemandFilterCriteria demandFilterCriteria, string employeeCode)
        //{
        //    demandFilterCriteria.StartDate = demandFilterCriteria.StartDate.GetMondayOfWeek();
        //    var caseAttributeNames = string.IsNullOrEmpty(demandFilterCriteria.CaseAttributeNames)
        //        ? Constants.ServiceLineCodes.GeneralConsulting
        //        : demandFilterCriteria.CaseAttributeNames;
        //    var planningCardsTask = Task.FromResult<IEnumerable<PlanningCard>>(null);

        //    // Getting the case planning board data
        //    var casePlanningBoardDataTask = _staffingApiClient.GetCasePlanningBoardDataByDateRange(demandFilterCriteria.StartDate);

        //    // Get employee data and master data
        //    var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
        //    var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
        //    var officesListTask = _ccmApiClient.GetOfficeList();
        //    var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
        //    var revenueByServiceLinesTask = Task.FromResult<IList<Revenue>>(null);

        //    if (demandFilterCriteria.CaseAttributeNames.Length > 0)
        //    {
        //        var firstDayOfStartDate = new DateTime(demandFilterCriteria.StartDate.Year, demandFilterCriteria.StartDate.Month, 1);
        //        var lastDayOfEndDate = new DateTime(demandFilterCriteria.EndDate.Year, demandFilterCriteria.EndDate.Month, DateTime.DaysInMonth(demandFilterCriteria.EndDate.Year, demandFilterCriteria.EndDate.Month));
        //        revenueByServiceLinesTask = _revenueApiClient.GetRevenueByServiceLine(demandFilterCriteria.CaseAttributeNames, firstDayOfStartDate, lastDayOfEndDate);
        //    }

        //    Task.WaitAll(casePlanningBoardDataTask, employeesIncludingTerminatedTask, skuTermsDataTask, officesListTask, investmentCategoriesTask, revenueByServiceLinesTask);

        //    var casePlanningBoardData = casePlanningBoardDataTask.Result;
        //    var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
        //    var officesList = officesListTask.Result;
        //    var skuTerms = skuTermsDataTask.Result; //TODO: remove the following line once new SKU logic is implemented
        //    var investmentCategories = investmentCategoriesTask.Result;
        //    var revenueByServiceLines = revenueByServiceLinesTask.Result;

        //    var revenueByServiceLinesOpps = new List<Revenue>();
        //    var revenueByServiceLinesCases = new List<Revenue>();

        //    if (revenueByServiceLines != null && revenueByServiceLines.Count() > 0)
        //    {
        //        revenueByServiceLinesOpps = revenueByServiceLines.Where(
        //                x => x.CaseCode == 0 && x.OpportunityId != null).GroupBy(x => x.OpportunityId).Select(x => x.First()).ToList();
        //        revenueByServiceLinesCases = revenueByServiceLines.Where(
        //                x => x.CaseCode > 0).ToList();
        //    }

        //    var boardOpps = casePlanningBoardData.CasePlanningBoardData.Where(x => string.IsNullOrEmpty(x.OldCaseCode) &&
        //                                                (x.PipelineId != null && x.PipelineId != Guid.Empty)).ToList();
        //    var boardCases = casePlanningBoardData.CasePlanningBoardData.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).ToList();

        //    var pipelineIds = string.Join(",", boardOpps.Select(x => x.PipelineId)?.Distinct());
        //    var oldCaseCodes = string.Join(",", boardCases.Select(x => x.OldCaseCode)?.Distinct());


        //    // Getting cases/opps
        //    //var projectsDataTask = _projectAggregatorService.GetOpportunitiesAndCasesWithAllocationsBySelectedValues(demandFilterCriteria);
        //    var oppsDataTask = _opportunityService.GetOpportunitiesByPipelineIdsAndFilterValues(pipelineIds, demandFilterCriteria, employeesIncludingTerminated, officesList,
        //        skuTerms, investmentCategories, revenueByServiceLinesOpps);

        //    var casesDataTask = _caseService.GetNewDemandCasesByOldCaseCodesAndFilterValues(oldCaseCodes, demandFilterCriteria, employeesIncludingTerminated, officesList,
        //        skuTerms, revenueByServiceLinesCases);

        //    // Getting the planning cards based on demandFilterCriteria
        //    if (demandFilterCriteria.DemandTypes.Contains(Constants.DemandType.PlanningCards))
        //    {
        //        planningCardsTask = _staffingApiClient.GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(employeeCode, demandFilterCriteria.OfficeCodes, caseAttributeNames);
        //    }

        //    await Task.WhenAll(oppsDataTask, casesDataTask, planningCardsTask);

        //    var projectsData = new List<ProjectData>();

        //    projectsData.AddRange(ConvertOpportunityToProjectModel(oppsDataTask.Result));
        //    projectsData.AddRange(ConvertCaseToProjectModel(casesDataTask.Result));
        //    var planningCards = planningCardsTask.Result;

        //    //get only new planning cards that are shared with selected office
        //    planningCards = planningCards.Where(x => x.IsShared.HasValue && (bool)x.IsShared);

        //    var columns = ConvertDataToColumns(projectsData, planningCards, casePlanningBoardData.CasePlanningBoardData, demandFilterCriteria.StartDate, officesList);

        //    return columns;
        //}

        public async Task<IEnumerable<CasePlanningBoardColumn>> GetCasePlanningBoardColumnsData(DemandFilterCriteria demandFilterCriteria, string employeeCode)
        { 
            demandFilterCriteria.StartDate = demandFilterCriteria.StartDate.GetMondayOfWeek();
            var caseAttributeNames = string.IsNullOrEmpty(demandFilterCriteria.CaseAttributeNames)
                ? ServiceLineCodes.GeneralConsulting
                : demandFilterCriteria.CaseAttributeNames;
            var planningCardsTask = Task.FromResult<IEnumerable<PlanningCard>>(null);

            // Getting the case planning board data
            var casePlanningBoardDataTask = _staffingApiClient.GetCasePlanningBoardDataByDateRange(demandFilterCriteria.StartDate, demandFilterCriteria.EndDate, employeeCode);

            // Get employee data and master data
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
            var officesListTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var revenueByServiceLinesTask = Task.FromResult<IList<Revenue>>(null);

            if (demandFilterCriteria.CaseAttributeNames.Length > 0)
            {
                var firstDayOfStartDate = new DateTime(demandFilterCriteria.StartDate.Year, demandFilterCriteria.StartDate.Month, 1);
                var lastDayOfEndDate = new DateTime(demandFilterCriteria.EndDate.Year, demandFilterCriteria.EndDate.Month, DateTime.DaysInMonth(demandFilterCriteria.EndDate.Year, demandFilterCriteria.EndDate.Month));
                revenueByServiceLinesTask = _revenueApiClient.GetRevenueByServiceLine(demandFilterCriteria.CaseAttributeNames, firstDayOfStartDate, lastDayOfEndDate);
            }

            Task.WaitAll(casePlanningBoardDataTask, employeesIncludingTerminatedTask, skuTermsDataTask, officesListTask, investmentCategoriesTask, revenueByServiceLinesTask);

            var casePlanningBoardData = casePlanningBoardDataTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var officesList = officesListTask.Result;
            var skuTerms = skuTermsDataTask.Result; //TODO: remove the following line once new SKU logic is implemented
            var investmentCategories = investmentCategoriesTask.Result;
            var revenueByServiceLines = revenueByServiceLinesTask.Result;

            var revenueByServiceLinesOpps = new List<Revenue>();
            var revenueByServiceLinesCases = new List<Revenue>();

            if (revenueByServiceLines != null && revenueByServiceLines.Count() > 0)
            {
                revenueByServiceLinesOpps = revenueByServiceLines.Where(
                        x => x.CaseCode == 0 && x.OpportunityId != null).GroupBy(x => x.OpportunityId).Select(x => x.First()).ToList();
                revenueByServiceLinesCases = revenueByServiceLines.Where(
                        x => x.CaseCode > 0).ToList();
            }

            var boardOpps = casePlanningBoardData.CasePlanningBoardData.Where(x => string.IsNullOrEmpty(x.OldCaseCode) &&
                                                        (x.PipelineId != null && x.PipelineId != Guid.Empty)).ToList();
            var boardCases = casePlanningBoardData.CasePlanningBoardData.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).ToList();
            var boardPlanningCards = casePlanningBoardData.CasePlanningBoardData.Where(x => x.PlanningCardId != null && x.PlanningCardId != Guid.Empty).ToList();

            var pipelineIds = string.Join(",", boardOpps.Select(x => x.PipelineId)?.Distinct());
            var oldCaseCodes = string.Join(",", boardCases.Select(x => x.OldCaseCode)?.Distinct());
            var planningCardIds = boardPlanningCards.Select(x => x.PlanningCardId)?.Distinct().ToList();

            // Getting cases/opps
            //var projectsDataTask = _projectAggregatorService.GetOpportunitiesAndCasesWithAllocationsBySelectedValues(demandFilterCriteria);
            var oppsDataTask = _opportunityService.GetFilteredOpportunitiesByPipelineIds(pipelineIds, demandFilterCriteria, employeesIncludingTerminated, officesList,
                skuTerms, investmentCategories, revenueByServiceLinesOpps, employeeCode);

            var casesDataTask = _caseService.GetFilteredNewDemandCasesByOldCaseCodes(oldCaseCodes, demandFilterCriteria, employeesIncludingTerminated, officesList,
                skuTerms, revenueByServiceLinesCases, employeeCode);

            // Getting the planning cards based on demandFilterCriteria
            if (demandFilterCriteria.DemandTypes.Contains(DemandType.PlanningCards))
            {
                planningCardsTask = _staffingApiClient.GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(employeeCode, demandFilterCriteria.OfficeCodes, caseAttributeNames);
            }

            await Task.WhenAll(oppsDataTask, casesDataTask, planningCardsTask);

            var projectsData = new List<ProjectData>();

            projectsData.AddRange(ConvertOpportunityToProjectModel(oppsDataTask.Result));
            projectsData.AddRange(ConvertCaseToProjectModel(casesDataTask.Result));
            var planningCards = planningCardsTask.Result;

            //get only new planning cards that are shared with selected office
            planningCards = planningCards?.Where(x => planningCardIds.Contains(x.Id) && x.IsShared.HasValue && (bool)x.IsShared);

            //add notes to planning cards
            var planningCardsWithNotes = await _planningCardService.GetPlanningCardsWithNotes(planningCards, employeeCode);

            var convertedCasesAndOpps = ConvertProjectDataToCasePlanningBoardViewModel(projectsData, casePlanningBoardData.CasePlanningBoardData);
            var convertedPlanningCards = ConvertPlanningCardsToCasePlanningBoardColumnViewModel(planningCardsWithNotes, casePlanningBoardData.CasePlanningBoardData, officesList);

            var sixColumnView = GetDataForSixColumnView(projectsData, planningCardsWithNotes, casePlanningBoardData.CasePlanningBoardData, demandFilterCriteria.StartDate, officesList);

            return sixColumnView;
        }

        public async Task<CasePlanningBoardColumn> GetCasePlanningBoardNewDemandsData(DemandFilterCriteria demandFilterCriteria, string employeeCode)
        {
            demandFilterCriteria.StartDate = demandFilterCriteria.StartDate.GetMondayOfWeek();
            var caseAttributeNames = string.IsNullOrEmpty(demandFilterCriteria.CaseAttributeNames)
                ? ServiceLineCodes.GeneralConsulting
                : demandFilterCriteria.CaseAttributeNames;
            var planningCardsTask = Task.FromResult<IEnumerable<PlanningCard>>(null);

            // Getting the case planning board data
            var casePlanningBoardDataTask = _staffingApiClient.GetCasePlanningBoardDataByDateRange(demandFilterCriteria.StartDate, null, employeeCode);

            // Get employee data and master data
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var skuTermsDataTask = _staffingApiClient.GetSKUTermList();
            var officesListTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var revenueByServiceLinesTask = Task.FromResult<IList<Revenue>>(null);

            if (demandFilterCriteria.CaseAttributeNames.Length > 0)
            {
                var firstDayOfStartDate = new DateTime(demandFilterCriteria.StartDate.Year, demandFilterCriteria.StartDate.Month, 1);
                var lastDayOfEndDate = new DateTime(demandFilterCriteria.EndDate.Year, demandFilterCriteria.EndDate.Month, DateTime.DaysInMonth(demandFilterCriteria.EndDate.Year, demandFilterCriteria.EndDate.Month));
                revenueByServiceLinesTask = _revenueApiClient.GetRevenueByServiceLine(demandFilterCriteria.CaseAttributeNames, firstDayOfStartDate, lastDayOfEndDate);
            }

            // Getting the case opp changes data by office codes
            var caseOppChangesTask = _staffingApiClient.GetCaseOppChangesByOfficesAndDateRange(demandFilterCriteria.OfficeCodes, demandFilterCriteria.StartDate, demandFilterCriteria.EndDate);

            Task.WaitAll(casePlanningBoardDataTask, employeesIncludingTerminatedTask, skuTermsDataTask, officesListTask, investmentCategoriesTask, revenueByServiceLinesTask, caseOppChangesTask);

            var casePlanningBoardData = casePlanningBoardDataTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var officesList = officesListTask.Result;
            var skuTerms = skuTermsDataTask.Result; //TODO: remove the following line once new SKU logic is implemented
            var investmentCategories = investmentCategoriesTask.Result;
            var revenueByServiceLines = revenueByServiceLinesTask.Result;
            var caseOppChanges = caseOppChangesTask.Result;

            var revenueByServiceLinesOpps = new List<Revenue>();
            var revenueByServiceLinesCases = new List<Revenue>();

            if (revenueByServiceLines != null && revenueByServiceLines.Count() > 0)
            {
                revenueByServiceLinesOpps = revenueByServiceLines.Where(
                        x => x.CaseCode == 0 && x.OpportunityId != null).GroupBy(x => x.OpportunityId).Select(x => x.First()).ToList();
                revenueByServiceLinesCases = revenueByServiceLines.Where(
                        x => x.CaseCode > 0).ToList();
            }

            var boardPipelineIds = GetCasePlanningBoardPipelineIds(casePlanningBoardData.CasePlanningBoardData);

            var boardOldCaseCodes = GetCasePlanningBoardOldCaseCodes(casePlanningBoardData.CasePlanningBoardData);

            var planningCardIds = GetCasePlanningBoardPlanningCardIds(casePlanningBoardData.CasePlanningBoardPlanningCardData);

            var pipelineIdsWithOverrides = GetPipelineIdsWithOverrides(caseOppChanges);
            var oldCaseCodesWithOverrides = GetOldCaseCodesWithOverrides(caseOppChanges);

            // Getting cases/opps
            var oppsDataTask = _opportunityService.GetOpportunitiesByPipelineIdsAndFilterValues(pipelineIdsWithOverrides, demandFilterCriteria, employeesIncludingTerminated, officesList,
                skuTerms, investmentCategories, revenueByServiceLinesOpps, employeeCode);

            var casesDataTask = _caseService.GetNewDemandCasesByOldCaseCodesAndFilterValues(oldCaseCodesWithOverrides, demandFilterCriteria, employeesIncludingTerminated, officesList,
                skuTerms, revenueByServiceLinesCases, employeeCode);

            // Getting the planning cards based on demandFilterCriteria
            if (demandFilterCriteria.DemandTypes.Contains(DemandType.PlanningCards))
            {
                planningCardsTask = _staffingApiClient.GetPlanningCardAndAllocationsByEmployeeCodeAndFilters(employeeCode, demandFilterCriteria.OfficeCodes, caseAttributeNames);
            }

            await Task.WhenAll(oppsDataTask, casesDataTask, planningCardsTask);

            var oppsData = oppsDataTask.Result.Where(x => !boardPipelineIds.Contains(x.PipelineId));
            var casesData = casesDataTask.Result.Where(x => !boardOldCaseCodes.Contains(x.OldCaseCode));
            //get only new planning cards that are shared with selected office and are not part of 6 column view
            var planningCardsData = planningCardsTask.Result.Where(x => !planningCardIds.Contains(x.Id) && x.IsShared.HasValue && (bool)x.IsShared);

            //get Planning Card notes here
            var planningCardsWithNotes = await _planningCardService.GetPlanningCardsWithNotes(planningCardsData, employeeCode);

            var projectsData = new List<ProjectData>();
            projectsData.AddRange(ConvertOpportunityToProjectModel(oppsData));
            projectsData.AddRange(ConvertCaseToProjectModel(casesData));
            
            var newDemandsColumn = GetDataForNewDemandsColumn(projectsData, planningCardsWithNotes, officesList);

            return newDemandsColumn;
        }

        public async Task<IEnumerable<StaffableTeamsColumn>> GetCasePlanningBoardStaffableTeams(string officeCodes, DateTime startWeek, DateTime endWeek)
        {
            // Getting the case planning board data
            var casePlanningBoardStaffableTeamsTask = _staffingApiClient.GetCasePlanningBoardStaffableTeamsByOfficesAndDateRange(officeCodes, startWeek, endWeek);
            var officesHierarchyByOfficesTask = _ccmApiClient.GetOfficeHierarchyByOffices(officeCodes);

            Task.WaitAll(casePlanningBoardStaffableTeamsTask, officesHierarchyByOfficesTask);

            var casePlanningBoardStaffableTeams = casePlanningBoardStaffableTeamsTask.Result;
            var officesHierarchyByOffices = officesHierarchyByOfficesTask.Result;

            //recursively get all the child offices for the selected offices
            var childOffices = new List<StaffableTeamsColumn>();
            var numberOfWeeksForCasePlanningBoard = Convert.ToInt16(ConfigurationUtility.GetValue("NumberOfWeeksForCasePlanningBoard"));

            for (var i = 1; i <= numberOfWeeksForCasePlanningBoard; i++)
            {
                var columnDate = startWeek.Date.AddDays((i - 1) * 7).Date;

                var column = new StaffableTeamsColumn()
                {
                    WeekOf = columnDate,
                    StaffableTeams = GetChildOffices(officesHierarchyByOffices, casePlanningBoardStaffableTeams.Where(x => x.WeekOf.Date == columnDate).ToList())
                };

                childOffices.Add(column);
            }

            return childOffices;
        }

        private StaffableTeamViewModel GetChildOffices(OfficeHierarchy office, List<CasePlanningBoardStaffableTeams> casePlanningBoardStaffableTeams)
        {
            var childOffices = new List<StaffableTeamViewModel>();
            if (office.Children != null && office.Children.Count > 0)
            {
                foreach (var childOffice in office.Children)
                {
                    childOffices.Add(GetChildOffices(childOffice, casePlanningBoardStaffableTeams));
                }
            }
            var staffableTeams = new StaffableTeamViewModel
            {
                OfficeCode = Convert.ToInt16(office.Value),
                OfficeName = office.Text,
                GCTeamCount = casePlanningBoardStaffableTeams.Where(x => x.OfficeCode == Convert.ToInt16(office.Value)).FirstOrDefault()?.GCTeamCount ?? 0,
                PegTeamCount = casePlanningBoardStaffableTeams.Where(x => x.OfficeCode == Convert.ToInt16(office.Value)).FirstOrDefault()?.PegTeamCount ?? 0,
                staffableTeamChildren = childOffices

            };
            return staffableTeams;
        }

        #region Private methods

        private string GetOldCaseCodesWithOverrides(IList<CaseOppChanges> caseOppChanges)
        {
            var casesWithOverrides = caseOppChanges?.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).ToList();
            var oldCaseCodes = string.Join(",", casesWithOverrides.Select(x => x.OldCaseCode)?.Distinct());

            return oldCaseCodes;
        }
        private string GetPipelineIdsWithOverrides(IList<CaseOppChanges> caseOppChanges)
        {
            var oppsWithOverrides = caseOppChanges?.Where(x => string.IsNullOrEmpty(x.OldCaseCode) &&
                                                        x.PipelineId != null && x.PipelineId != Guid.Empty).ToList();
            var pipelineIds = string.Join(",", oppsWithOverrides.Select(x => x.PipelineId)?.Distinct());

            return pipelineIds;
        }

        private List<Guid?> GetCasePlanningBoardPipelineIds(IList<CasePlanningBoard> casePlanningBoardData) 
        {
            var boardOpps = casePlanningBoardData?.Where(x => string.IsNullOrEmpty(x.OldCaseCode) &&
                                                x.PipelineId != null && x.PipelineId != Guid.Empty).ToList();
            var boardPipelineIds = boardOpps.Select(x => x.PipelineId)?.Distinct().ToList();
            return boardPipelineIds;
        }

        private List<string> GetCasePlanningBoardOldCaseCodes(IList<CasePlanningBoard> casePlanningBoardData) 
        {
            var boardCases = casePlanningBoardData?.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).ToList();
            var boardOldCaseCodes = boardCases.Select(x => x.OldCaseCode)?.Distinct().ToList();
            return boardOldCaseCodes;
        }

        private List<Guid?> GetCasePlanningBoardPlanningCardIds(IList<CasePlanningBoard> casePlanningBoardPlanningCardData) 
        {
            var boardPlanningCards = casePlanningBoardPlanningCardData?.Where(x => x.PlanningCardId != Guid.Empty).ToList();
            var boardPlanningCardIds = boardPlanningCards.Select(x => x.PlanningCardId)?.Distinct().ToList();
            return boardPlanningCardIds;
        }

        private CasePlanningBoardColumn GetDataForNewDemandsColumn(List<ProjectData> projectsData, IEnumerable<PlanningCard> planningCardsData, IEnumerable<Office> officesList)
        {
            var oppsAndCases = ConvertProjectDataToCasePlanningBoardViewModel(projectsData, null);
            var planningCards = ConvertPlanningCardsToCasePlanningBoardColumnViewModel(planningCardsData, null, officesList);

            var newCasesColumnProjects = oppsAndCases.Concat(planningCards);
            newCasesColumnProjects = SortProjectsForCasePlanningBoard(newCasesColumnProjects).ToList();

            var newCasesColumn = GetNewDemandsColumn(newCasesColumnProjects);
            return newCasesColumn;
        }

        private IEnumerable<CasePlanningBoardColumn> GetDataForSixColumnView(IEnumerable<ProjectData> projectsData,
            IEnumerable<PlanningCard> planningCards, IEnumerable<CasePlanningBoard> casePlanningBoardData, DateTime startDate, IEnumerable<Office> offices)
        {
            var convertedCasesAndOpps = ConvertProjectDataToCasePlanningBoardViewModel(projectsData, casePlanningBoardData);
            var convertedPlanningCards = ConvertPlanningCardsToCasePlanningBoardColumnViewModel(planningCards, casePlanningBoardData, offices);

            var allProjects = convertedCasesAndOpps.Concat(convertedPlanningCards);
            allProjects = SortProjectsForCasePlanningBoard(allProjects).ToList();

            var casePlanningBoardColumns = new List<CasePlanningBoardColumn>();

            var demandMetricsProjects = GetProjectsToBeIncludedInDemandMetricsColumn(allProjects);

            var columnsData = GetSixColumnsView(allProjects, startDate);
            var sixColumnsViewData = columnsData.Item1;

            casePlanningBoardColumns.Add(demandMetricsProjects);
            casePlanningBoardColumns.AddRange(sixColumnsViewData);
            //casePlanningBoardColumns.Concat(sixColumnsViewData.ToList());

            casePlanningBoardColumns.Reverse();

            return casePlanningBoardColumns;
        }
        private IEnumerable<CasePlanningBoardColumn> ConvertDataToColumns(IEnumerable<ProjectData> projectsData,
            IEnumerable<PlanningCard> planningCards, IEnumerable<CasePlanningBoard> casePlanningBoardData, DateTime startDate, IEnumerable<Office> offices)
        {
            var convertedCasesAndOpps = ConvertProjectDataToCasePlanningBoardViewModel(projectsData, casePlanningBoardData);
            var convertedPlanningCards = ConvertPlanningCardsToCasePlanningBoardColumnViewModel(planningCards, casePlanningBoardData, offices);

            var newCasesColumnProjects = convertedCasesAndOpps.Concat(convertedPlanningCards);
            newCasesColumnProjects = SortProjectsForCasePlanningBoard(newCasesColumnProjects).ToList();

            var casePlanningBoardColumns = new List<CasePlanningBoardColumn>();

            var demandMetricsProjects = GetProjectsToBeIncludedInDemandMetricsColumn(newCasesColumnProjects);

            var columnsData = GetSixColumnsView(newCasesColumnProjects, startDate);
            var sixColumnsViewData = columnsData.Item1;
            newCasesColumnProjects = columnsData.Item2;

            var newCasesColumn = GetNewDemandsColumn(newCasesColumnProjects);

            casePlanningBoardColumns.Add(demandMetricsProjects);
            casePlanningBoardColumns.Concat(sixColumnsViewData);
            casePlanningBoardColumns.Add(newCasesColumn);

            casePlanningBoardColumns.Reverse();

            return casePlanningBoardColumns;
        }

        private CasePlanningBoardColumn GetProjectsToBeIncludedInDemandMetricsColumn(IEnumerable<CasePlanningBoardViewModel> allProjects)
        {
            return GetCasePlanningBoardColumn(CasePlanningBoardColumns.ProjectsToBeIncludedInDemandMetrics, allProjects);
        }

        private (List<CasePlanningBoardColumn>, List<CasePlanningBoardViewModel> casePlanningBoardViews) GetSixColumnsView(IEnumerable<CasePlanningBoardViewModel> allProjects, DateTime startDate)
        {
            var casePlanningBoardColumns = new List<CasePlanningBoardColumn>();
            var numberOfWeeksForCasePlanningBoard = Convert.ToInt16(ConfigurationUtility.GetValue("NumberOfWeeksForCasePlanningBoard"));
            var allProjectsExceptColumnProjects = allProjects.ToList();

            for (var i = numberOfWeeksForCasePlanningBoard; i >= 1; i--)
            {
                var columnDate = startDate.Date.AddDays((i - 1) * 7).Date;
                var projectsForColumn = allProjectsExceptColumnProjects?.Where(x => x.Date.HasValue && x.Date.Value.Date == columnDate);
                var column = GetCasePlanningBoardColumn(columnDate.ToString(), projectsForColumn);

                casePlanningBoardColumns.Add(column);
                allProjectsExceptColumnProjects = allProjectsExceptColumnProjects?.Except(projectsForColumn)?.ToList();
            }
            return (casePlanningBoardColumns, allProjectsExceptColumnProjects);
        }

        private CasePlanningBoardColumn GetNewDemandsColumn(IEnumerable<CasePlanningBoardViewModel> allProjects)
        {
            return GetCasePlanningBoardColumn(CasePlanningBoardColumns.NewDemandsColumn, allProjects?.Where(x => x.Date == null)?.ToList());
        }

        private CasePlanningBoardColumn GetCasePlanningBoardColumn(string columnName, IEnumerable<CasePlanningBoardViewModel> projects)
        {
            return new CasePlanningBoardColumn()
            {
                Title = columnName,
                Projects = projects
            };
        }

        private IEnumerable<CasePlanningBoardViewModel> SortProjectsForCasePlanningBoard(IEnumerable<CasePlanningBoardViewModel> casePlanningBoardViewModels)
        {
            return casePlanningBoardViewModels.OrderBy(p => !string.IsNullOrEmpty(p.StaffingOfficeAbbreviation) ? p.StaffingOfficeAbbreviation : p.ManagingOfficeAbbreviation)
            .ThenByDescending(q => q.ClientPrioritySortOrder.HasValue)
            .ThenBy(r => r.ClientPrioritySortOrder)
            .ThenBy(s => s.StartDate)
            .ThenBy(t => t.ClientName);
        }

        private IEnumerable<CasePlanningBoardViewModel> ConvertProjectDataToCasePlanningBoardViewModel(IEnumerable<ProjectData> projectsData,
            IEnumerable<CasePlanningBoard> casePlanningBoardData)
        {
            var projects = new List<CasePlanningBoardViewModel>();

            var newDemands = projectsData?.Where(x => !string.IsNullOrEmpty(x.OldCaseCode))?.Select(item => new CasePlanningBoardViewModel
            {
                PlanningBoardId = casePlanningBoardData?.FirstOrDefault(x => x.OldCaseCode == item.OldCaseCode)?.Id,
                BucketId = casePlanningBoardData?.FirstOrDefault(x => x.OldCaseCode == item.OldCaseCode)?.BucketId,
                BucketName = casePlanningBoardData?.FirstOrDefault(x => x.OldCaseCode == item.OldCaseCode)?.BucketName,
                Date = casePlanningBoardData?.FirstOrDefault(x => x.OldCaseCode == item.OldCaseCode)?.Date,
                IncludeInDemand = casePlanningBoardData?.FirstOrDefault(x => x.OldCaseCode == item.OldCaseCode)?.IncludeInDemand,
                CaseCode = item.CaseCode,
                CaseName = item.CaseName,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                OldCaseCode = item.OldCaseCode,
                CaseTypeCode = item.CaseTypeCode,
                CaseType = item.CaseType,
                CaseManagerCode = item.CaseManagerCode,
                CaseManagerName = item.CaseManagerFullName,
                CaseManagerOfficeAbbreviation = item.CaseManagerOfficeAbbreviation,
                ManagingOfficeCode = item.ManagingOfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeName = item.ManagingOfficeName,
                BillingOfficeCode = item.BillingOfficeCode,
                BillingOfficeAbbreviation = item.BillingOfficeAbbreviation,
                BillingOfficeName = item.BillingOfficeName,
                OriginalStartDate = item.OriginalStartDate,
                OverrideStartDate = item.OverrideStartDate,
                StartDate = item.StartDate,
                OriginalEndDate = item.OriginalEndDate,
                OverrideEndDate = item.OverrideEndDate,
                EndDate = item.EndDate,
                Type = item.Type,
                CaseRoll = item.CaseRoll,
                IsPrivateEquity = item.IsPrivateEquity,
                CaseAttributes = item.CaseAttributes,
                CaseServedByRingfence = item.CaseServedByRingfence,
                Notes = item.Notes,
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                //AllocatedResources = item.AllocatedResources,
                LatestCasePlanningBoardViewNote = item.CasePlanningViewNotes.FirstOrDefault(),
                PlaceholderAllocations = item.PlaceholderAllocations,
                SKUTerm = item.SkuTerm,
                CombinedSkuTerm = CalculateCombinedSkuTerm(item.SkuTerm),
                EstimatedTeamSize = item?.EstimatedTeamSize,
                StaffingOfficeCode = item.StaffingOfficeCode,
                StaffingOfficeAbbreviation = item.StaffingOfficeAbbreviation,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                IsPlaceholderCreatedFromCortex = item.IsPlaceholderCreatedFromCortex
            });

            var opps = projectsData?.Where(x => string.IsNullOrEmpty(x.OldCaseCode) && (x.PipelineId.HasValue || x.PipelineId != Guid.Empty))?.Select(item => new CasePlanningBoardViewModel
            {
                PlanningBoardId = casePlanningBoardData?.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.Id,
                BucketId = casePlanningBoardData?.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.BucketId,
                BucketName = casePlanningBoardData?.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.BucketName,
                Date = casePlanningBoardData?.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.Date,
                IncludeInDemand = casePlanningBoardData?.FirstOrDefault(x => x.PipelineId == item.PipelineId)?.IncludeInDemand,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                PipelineId = item.PipelineId,
                EstimatedTeamSize = item?.EstimatedTeamSize,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                CoordinatingPartnerName = item.CoordinatingPartnerName,
                BillingPartnerCode = item.BillingPartnerCode,
                BillingPartnerName = item.BillingPartnerName,
                OpportunityName = item.OpportunityName,
                OriginalProbabilityPercent = item.OriginalProbabilityPercent,
                OverrideProbabilityPercent = item.OverrideProbabilityPercent,
                ProbabilityPercent = item.ProbabilityPercent,
                CaseManagerCode = item.CoordinatingPartnerCode, //co-ordinating partner is considered as case manager here
                CaseManagerName = item.CoordinatingPartnerName,
                CaseManagerOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeCode = item.ManagingOfficeCode,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeName = item.ManagingOfficeName,
                BillingOfficeCode = item.BillingOfficeCode,
                BillingOfficeAbbreviation = item.BillingOfficeAbbreviation,
                BillingOfficeName = item.BillingOfficeName,
                OriginalStartDate = item.OriginalStartDate,
                OverrideStartDate= item.OverrideStartDate,
                StartDate = item.StartDate,
                OriginalEndDate = item.OriginalEndDate,
                OverrideEndDate= item.OverrideEndDate,
                EndDate = item.EndDate,
                Type = item.Type,
                Notes = item.Notes,
                CaseServedByRingfence = item.CaseServedByRingfence,
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                //AllocatedResources = item.AllocatedResources,
                PlaceholderAllocations = item.PlaceholderAllocations,
                SKUTerm = item.SkuTerm,
                CombinedSkuTerm = CalculateCombinedSkuTerm(item.SkuTerm),
                isStartDateUpdatedInBOSS = item.isStartDateUpdatedInBOSS,
                isEndDateUpdatedInBOSS = item.isEndDateUpdatedInBOSS,
                StaffingOfficeCode = item.StaffingOfficeCode,
                StaffingOfficeAbbreviation = item.StaffingOfficeAbbreviation,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                LatestCasePlanningBoardViewNote = item.CasePlanningViewNotes.FirstOrDefault(),
                IsPlaceholderCreatedFromCortex = item.IsPlaceholderCreatedFromCortex
            });

            projects = newDemands.Concat(opps).ToList();

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

        private IEnumerable<CasePlanningBoardViewModel> ConvertPlanningCardsToCasePlanningBoardColumnViewModel(IEnumerable<PlanningCard> planningCards,
            IEnumerable<CasePlanningBoard> casePlanningBoardData, IEnumerable<Office> offices)
        {
            if (planningCards == null || !planningCards.Any())
            {
                return Enumerable.Empty<CasePlanningBoardViewModel>();
            }

            var projects = planningCards?.Select(item => new CasePlanningBoardViewModel
            {
                PlanningBoardId = casePlanningBoardData?.FirstOrDefault(x => x.PlanningCardId == item.Id)?.Id,
                BucketId = casePlanningBoardData?.FirstOrDefault(x => x.PlanningCardId == item.Id)?.BucketId ?? null,
                BucketName = casePlanningBoardData?.FirstOrDefault(x => x.PlanningCardId == item.Id)?.BucketName ?? null,
                Date = casePlanningBoardData?.FirstOrDefault(x => x.PlanningCardId == item.Id)?.Date ?? null,
                IncludeInDemand = casePlanningBoardData?.FirstOrDefault(x => x.PlanningCardId == item.Id)?.IncludeInDemand,
                PlanningCardId = item.Id,
                Name = item.Name,
                OriginalStartDate = item.StartDate,
                StartDate = item.StartDate,
                OriginalEndDate = item.EndDate,
                EndDate = item.EndDate,
                Office = item.Office,
                IsShared = item.IsShared,
                SharedOfficeCodes = item.SharedOfficeCodes,
                SharedOfficeAbbreviations = !string.IsNullOrEmpty(item.SharedOfficeCodes) ? GetOfficeAbbreviationListForPlanningCards(item.SharedOfficeCodes, offices) : null,
                SharedStaffingTags = item.SharedStaffingTags,
                SKUTerm = item.SkuTerm,
                CombinedSkuTerm = CalculateCombinedSkuTerm(item.SkuTerm),
                LatestCasePlanningBoardViewNote = ConverToCasePlanningViewNoteViewModel(item.CasePlanningViewNotes).FirstOrDefault(),
                PegOpportunityId = item.PegOpportunityId
            });

            return projects;
        }

        private string GetOfficeAbbreviationListForPlanningCards(string officeCodes, IEnumerable<Office> offices)
        {
            return string.Join(",", offices.Where(x => officeCodes.Contains(x.OfficeCode.ToString())).OrderBy(z => z.OfficeAbbreviation).Select(y => y.OfficeAbbreviation));
        }

        private IEnumerable<ProjectData> ConvertCaseToProjectModel(IEnumerable<CaseData> cases)
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
                Notes = item.Notes,
                CaseServedByRingfence = item.CaseServedByRingfence,
                ProjectStatus = Convert.ToDateTime(item.EndDate).Date >= DateTime.Now.Date ? Convert.ToString(ProjectStatus.Active) : Convert.ToString(ProjectStatus.Inactive),
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                StaffingOfficeCode = item.StaffingOfficeCode,
                StaffingOfficeAbbreviation = item.StaffingOfficeAbbreviation,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                EstimatedTeamSize = item.EstimatedTeamSize,
                CasePlanningViewNotes = ConverToCasePlanningViewNoteViewModel(item.CasePlanningViewNotes),
                IsPlaceholderCreatedFromCortex = item.IsPlaceholderCreatedFromCortex
            });

            return projects;
        }

        private IEnumerable<ProjectData> ConvertOpportunityToProjectModel(IEnumerable<OpportunityData> opportunities)
        {
            var projects = opportunities.Select(item => new ProjectData
            {
                PipelineId = item.PipelineId,
                EstimatedTeamSize = item?.EstimatedTeamSize,
                OpportunityName = item.OpportunityName,
                CoordinatingPartnerCode = item.CoordinatingPartnerCode,
                CoordinatingPartnerName = item.CoordinatingPartnerName,
                BillingOfficeCode = item.BillingOfficeCode,
                BillingOfficeAbbreviation = item.BillingOfficeAbbreviation,
                BillingPartnerCode = item.BillingPartnerCode,
                BillingPartnerName = item.BillingPartnerName,
                ClientCode = item.ClientCode,
                ClientName = item.ClientName,
                ManagingOfficeAbbreviation = item.ManagingOfficeAbbreviation,
                ManagingOfficeCode = item.ManagingOfficeCode,
                OriginalStartDate = item.OriginalStartDate,
                OverrideStartDate = item.OverrideStartDate,
                StartDate = item.StartDate,
                OriginalEndDate = item.OriginalEndDate,
                OverrideEndDate = item.OverrideEndDate,
                EndDate = item.EndDate,
                OriginalProbabilityPercent = item.OriginalProbabilityPercent,
                OverrideProbabilityPercent = item.OverrideProbabilityPercent,
                ProbabilityPercent = item.ProbabilityPercent,
                Type = item.Type,
                AllocatedResources = item.AllocatedResources,
                PlaceholderAllocations = item.PlaceholderAllocations,
                SkuTerm = item.SKUTerm,
                ProjectStatus = item.EndDate == null ? Convert.ToString(ProjectStatus.Active) : Convert.ToDateTime(item.EndDate).Date >= DateTime.Now.Date ? Convert.ToString(ProjectStatus.Active) : Convert.ToString(ProjectStatus.Inactive),
                CaseAttributes = item.CaseAttributes,
                Notes = item.Notes,
                CaseServedByRingfence = item.CaseServedByRingfence,
                ClientPriority = item.ClientPriority,
                ClientPrioritySortOrder = item.ClientPrioritySortOrder,
                isStartDateUpdatedInBOSS = item.isStartDateUpdatedInBOSS,
                isEndDateUpdatedInBOSS = item.isEndDateUpdatedInBOSS,
                StaffingOfficeCode = item.StaffingOfficeCode,
                StaffingOfficeAbbreviation = item.StaffingOfficeAbbreviation,
                IndustryPracticeAreaCode = item.IndustryPracticeAreaCode,
                IndustryPracticeArea = item.IndustryPracticeArea,
                CapabilityPracticeAreaCode = item.CapabilityPracticeAreaCode,
                CapabilityPracticeArea = item.CapabilityPracticeArea,
                CasePlanningViewNotes = ConverToCasePlanningViewNoteViewModel(item.CasePlanningViewNotes),
                IsPlaceholderCreatedFromCortex = item.IsPlaceholderCreatedFromCortex
            });
            return projects;
        }

        private IEnumerable<CaseViewNoteViewModel> ConverToCasePlanningViewNoteViewModel(IEnumerable<CaseViewNote> latestCasePlanningViewNotes)
        {
            var note = latestCasePlanningViewNotes.Select(item => new CaseViewNoteViewModel
            {
                Id = item.Id,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                PlanningCardId = item.PlanningCardId,
                Note = item.Note,
                IsPrivate = item.IsPrivate,
                SharedWith = item.SharedWith,
                CreatedBy = item.CreatedBy,
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = item.LastUpdatedBy

            }).ToList();

            return note;
        }
        #endregion
    }
}
