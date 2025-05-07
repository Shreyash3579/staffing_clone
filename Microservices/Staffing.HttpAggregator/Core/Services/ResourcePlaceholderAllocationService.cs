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
    public class ResourcePlaceholderAllocationService : IResourcePlaceholderAllocationService
    {
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IResourceAllocationService _resourceAllocationService;
        public ResourcePlaceholderAllocationService(IStaffingApiClient staffingApiClient, 
            IResourceApiClient resourceApiClient, ICCMApiClient ccmApiClient, 
            IPipelineApiClient pipelineApiClient, IResourceAllocationService resourceAllocationService)
        {
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _ccmApiClient = ccmApiClient;
            _pipelineApiClient = pipelineApiClient;
            _resourceAllocationService = resourceAllocationService;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpsertPlaceholderAllocations(IEnumerable<ResourceAssignmentViewModel> placeholderAllocations)
        {
            if (!placeholderAllocations.Any())
            {
                return Enumerable.Empty<ResourceAssignmentViewModel>();
            }
            var listOldCaseCodes = string.Join(",", placeholderAllocations.Select(x => x.OldCaseCode).Distinct());
            var listPipelineId = string.Join(",", placeholderAllocations.Select(x => x.PipelineId).Distinct());
            var listPlanningCardId = string.Join(",", placeholderAllocations.Select(x => x.PlanningCardId).Distinct());
            var resourcesTask = _resourceApiClient.GetEmployees();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var caseDataTask = !string.IsNullOrEmpty(listOldCaseCodes) ? _ccmApiClient.GetCaseDataByCaseCodes(listOldCaseCodes) : null;
            var opportunityDataTask = placeholderAllocations.First().PipelineId != null
                ? _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(placeholderAllocations.First().PipelineId.ToString())
                : null;
            var planningCardDataTask = placeholderAllocations.First().PlanningCardId != null
                ? _staffingApiClient.GetPlanningCardByPlanningCardIds(placeholderAllocations.First().PlanningCardId.ToString()) : null;
            var commitmentTypesTask = _staffingApiClient.GetCommitmentTypeList();

            if (caseDataTask != null)
                await Task.WhenAll(resourcesTask, officesTask, caseDataTask, investmentCategoriesTask, commitmentTypesTask);
            else if( opportunityDataTask != null)
                await Task.WhenAll(resourcesTask, officesTask, opportunityDataTask, investmentCategoriesTask, commitmentTypesTask);
            else
                await Task.WhenAll(resourcesTask, investmentCategoriesTask);

            var resources = resourcesTask.Result;
            var casesData = caseDataTask?.Result;
            var opportunityData = opportunityDataTask?.Result;
            var planningCardData = planningCardDataTask?.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var offices = officesTask.Result;
            var commitmentTypes = commitmentTypesTask.Result;

            var allocations = ConvertToScheduleMasterPlacholderModel(placeholderAllocations, resources, casesData, opportunityData, null, offices, commitmentTypes);

            var allocatedResources = await _staffingApiClient.UpsertPlaceholderAllocations(allocations);
            var skuTerms = await
                _staffingApiClient.GetSKUTermForProjects(listOldCaseCodes, listPipelineId, listPlanningCardId);

            return ConvertToResourceAssignmentViewModel(allocatedResources, casesData, opportunityData, planningCardData, resources, investmentCategories, commitmentTypes, skuTerms);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpsertCaseRollsAndPlaceholderAllocations(IEnumerable<CaseRoll> caseRolls, IEnumerable<ResourceAssignmentViewModel> resourceAllocations)
        {
            var caseRollTask = _staffingApiClient.UpsertCaseRolls(caseRolls);

            var placeholderAllocations = resourceAllocations.Where(ra => ra.PlanningCardId != null);
            var resourceAllocationsForCase= resourceAllocations.Where(ra => ra.OldCaseCode != null);

            var placeholderAllocationsTask = UpsertPlaceholderAllocations(placeholderAllocations);
            var allocationsTask = _resourceAllocationService.UpsertResourceAllocations(resourceAllocationsForCase);

            await Task.WhenAll(caseRollTask, placeholderAllocationsTask, allocationsTask);


            var result = allocationsTask.Result.Concat(placeholderAllocationsTask.Result);
            return result;
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<ScheduleMasterPlaceholder>();

            var caseDataTask = _ccmApiClient.GetCaseDataByCaseCodes(oldCaseCodes);
            var placeholderAllocationsDataTask = _staffingApiClient.GetPlaceholderAllocationsByCaseCodes(oldCaseCodes);
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            await Task.WhenAll(caseDataTask, placeholderAllocationsDataTask,
                resourcesDataTask, officeListDataTask, commitmentTypeListTask);

            var caseData = caseDataTask.Result;
            var placeholderAllocations = placeholderAllocationsDataTask.Result;
            var resources = resourcesDataTask.Result;
            var offices = officeListDataTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;


            return ConvertToScheduleMasterPlacholderModel(placeholderAllocations, resources, caseData, null, null, offices, commitmentTypeList);
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return Enumerable.Empty<ScheduleMasterPlaceholder>();

            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineIds);
            var placeholderAllocationsDataTask = _staffingApiClient.GetPlaceholderAllocationsByPipelineIds(pipelineIds);
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(pipelineIds);
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            await Task.WhenAll(opportunityDataTask,  placeholderAllocationsDataTask,
                resourcesDataTask, officeListDataTask, pipelineChangesTask, commitmentTypeListTask);

            var opportunityData = opportunityDataTask.Result;
            var pipelineChangesData = pipelineChangesTask.Result;
            var placeholderAllocations = placeholderAllocationsDataTask.Result;
            var resources = resourcesDataTask.Result;
            var offices = officeListDataTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            pipelineChangesData.Join(opportunityData, (pipelineChange) => pipelineChange.PipelineId, (opportunity) => opportunity.PipelineId, (pipelineChange, opportunity) =>
            {
                opportunity.isStartDateUpdatedInBOSS = pipelineChange.StartDate.HasValue && pipelineChange.StartDate != DateTime.MinValue && opportunity.StartDate != pipelineChange.StartDate;
                opportunity.isEndDateUpdatedInBOSS = pipelineChange.EndDate.HasValue && pipelineChange.EndDate != DateTime.MinValue && opportunity.EndDate != pipelineChange.EndDate;
                opportunity.StartDate = pipelineChange.StartDate ?? opportunity.StartDate;
                opportunity.EndDate = pipelineChange.EndDate ?? opportunity.EndDate;
                opportunity.ProbabilityPercent = pipelineChange.ProbabilityPercent ?? opportunity.ProbabilityPercent;
                opportunity.Notes = pipelineChange.Notes;
                opportunity.StaffingOfficeCode = pipelineChange.StaffingOfficeCode;

                return opportunity;
            }).ToList();


            return ConvertToScheduleMasterPlacholderModel(placeholderAllocations, resources, null, opportunityData, null, offices, commitmentTypeList);
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByPlanningCardIds(string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<ScheduleMasterPlaceholder>();

            var planningCardsTask = _staffingApiClient.GetPlanningCardByPlanningCardIds(planningCardIds);
            var placeholderAllocationsDataTask = _staffingApiClient.GetPlaceholderAllocationsByPlanningCardIds(planningCardIds);
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();
            await Task.WhenAll(planningCardsTask, placeholderAllocationsDataTask,
                resourcesDataTask, officeListDataTask, commitmentTypeListTask);
            var planningCards = planningCardsTask.Result;
            var placeholderAllocations = placeholderAllocationsDataTask.Result;
            var resources = resourcesDataTask.Result;
            var offices = officeListDataTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            return ConvertToScheduleMasterPlacholderModel(placeholderAllocations, resources, null, null, planningCards, offices, commitmentTypeList);
        }

        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetAllocationsByPlanningCardIds(string planningCardIds, string effectiveFromDate)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return Enumerable.Empty<ScheduleMasterPlaceholder>();

            var planningCardsTask = _staffingApiClient.GetPlanningCardByPlanningCardIds(planningCardIds);
            var allocationsDataTask = _staffingApiClient.GetAllocationsByPlanningCardIds(planningCardIds);
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();
            await Task.WhenAll(planningCardsTask, allocationsDataTask,
                resourcesDataTask, officeListDataTask, commitmentTypeListTask);
            var planningCards = planningCardsTask.Result;
            var allocations = allocationsDataTask.Result;
            var resources = resourcesDataTask.Result;
            var offices = officeListDataTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            if (!string.IsNullOrEmpty(effectiveFromDate))
            {
                allocations = allocations.Where(allocation => allocation.EndDate >= DateTime.Parse(effectiveFromDate)).ToList();
            }

            return ConvertToScheduleMasterPlacholderModel(allocations, resources, null, null, planningCards, offices, commitmentTypeList);
        }
        public async Task<IEnumerable<ScheduleMasterPlaceholder>> GetPlaceholderAllocationsByEmployeeCode(string employeeCode, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return Enumerable.Empty<ScheduleMasterPlaceholder>();

            var placeholderAllocationsTask = _staffingApiClient.GetPlaceholderAllocationsByEmployeeCodes(employeeCode, startDate, endDate);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            await Task.WhenAll(placeholderAllocationsTask, resourcesTask, officeListDataTask, commitmentTypeListTask);

            var placeholderAllocations = placeholderAllocationsTask.Result;
            var resources = resourcesTask.Result;
            var offices = officeListDataTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            var listCaseCodes = placeholderAllocations.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).Select(x => x.OldCaseCode).Distinct();
            var listPipeLineIds = placeholderAllocations.Where(x => string.IsNullOrEmpty(x.OldCaseCode) && x.PipelineId.HasValue).Select(x => x.PipelineId).Distinct();
            var listPlanningCardIds = placeholderAllocations.Where(x => x.PlanningCardId.HasValue).Select(x => x.PlanningCardId).Distinct();

            var casesDataTask = _ccmApiClient.GetCaseDataByCaseCodes(string.Join(",", listCaseCodes));
            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(string.Join(",", listPipeLineIds));
            var planningCardsTask = _staffingApiClient.GetPlanningCardByPlanningCardIds(string.Join(",", listPlanningCardIds));

            await Task.WhenAll(casesDataTask, opportunityDataTask, planningCardsTask);

            var caseData = casesDataTask.Result;
            var opportunityData = opportunityDataTask.Result;
            var planningCards = planningCardsTask.Result;

            return ConvertToScheduleMasterPlaceholderModel(placeholderAllocations, resources, caseData, opportunityData, planningCards, offices, commitmentTypeList);
        }

        #region Private methods
        private static IEnumerable<ScheduleMasterPlaceholder> ConvertToScheduleMasterPlacholderModel(
             IEnumerable<ResourceAssignmentViewModel> placeholderAllocations, IEnumerable<Resource> resources,
             IEnumerable<CaseData> cases, IEnumerable<OpportunityData> opportunityData, IEnumerable<PlanningCard> planningCards,
             IEnumerable<Office> offices, IEnumerable<CommitmentType> commitmentTypes)
        {
            //data cannot be NULL when used in JOINS. Hence, assigning cases to empty when null
            cases = cases ?? Enumerable.Empty<CaseData>();
            opportunityData = opportunityData ?? Enumerable.Empty<OpportunityData>();
            planningCards = planningCards ?? Enumerable.Empty<PlanningCard>();

            var allocations = (from placeholderAllocation in placeholderAllocations
                               join ccm in cases on placeholderAllocation.OldCaseCode equals ccm?.OldCaseCode into resCasesGroups
                               from caseItem in resCasesGroups.DefaultIfEmpty()
                               join opp in opportunityData on placeholderAllocation.PipelineId equals opp?.PipelineId into resOppsGroups
                               from opportunityItem in resOppsGroups.DefaultIfEmpty()
                               join pc in planningCards on placeholderAllocation.PlanningCardId equals pc?.Id into resPlanningCardsGroups
                               from planningCardItem in resPlanningCardsGroups.DefaultIfEmpty()
                               join res in resources on placeholderAllocation.EmployeeCode equals res.EmployeeCode into resAllocGroups
                               from resource in resAllocGroups.DefaultIfEmpty()
                               join o in offices on placeholderAllocation.OperatingOfficeCode equals o.OfficeCode into resAllocOffices
                               from office in resAllocOffices.DefaultIfEmpty()
                               join res in resources on opportunityItem?.CoordinatingPartnerCode equals res.EmployeeCode into oppCoordPartner
                               from coordinatingPartner in oppCoordPartner.DefaultIfEmpty()
                               join res in resources on opportunityItem?.BillingPartnerCode equals res.EmployeeCode into oppBillPartner
                               from billingPartner in oppBillPartner.DefaultIfEmpty()
                               join o in offices on coordinatingPartner?.SchedulingOffice.OfficeCode equals o.OfficeCode into resAllocManagerOffices
                               from opportunityManagingOffice in resAllocManagerOffices.DefaultIfEmpty()
                               join o in offices on billingPartner?.SchedulingOffice.OfficeCode equals o.OfficeCode into resAllocBillingOffices
                               from opportunityBillingOffice in resAllocBillingOffices.DefaultIfEmpty()
                               join comm in commitmentTypes on placeholderAllocation.CommitmentTypeCode equals comm?.CommitmentTypeCode into resCommitmentType
                               from commitmentItem in resCommitmentType.DefaultIfEmpty()
                               select new ScheduleMasterPlaceholder()
                               {
                                   Id = placeholderAllocation.Id ?? Guid.NewGuid(),
                                   PlanningCardId = placeholderAllocation.PlanningCardId,
                                   PlanningCardTitle = placeholderAllocation.PlanningCardTitle,
                                   OldCaseCode = placeholderAllocation.OldCaseCode,
                                   CaseCode = caseItem?.CaseCode,
                                   CaseName = caseItem?.CaseName,
                                   ClientCode = caseItem?.ClientCode ?? opportunityItem?.ClientCode,
                                   ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                   OpportunityName = opportunityItem?.OpportunityName,
                                   PipelineId = placeholderAllocation?.PipelineId,
                                   CaseTypeCode = caseItem?.CaseTypeCode,
                                   CaseTypeName = caseItem?.CaseType,
                                   EmployeeName = resource?.FullName,
                                   EmployeeCode = placeholderAllocation.EmployeeCode,
                                   ServiceLineCode = resource?.ServiceLine?.ServiceLineCode ?? placeholderAllocation.ServiceLineCode,
                                   ServiceLineName = resource?.ServiceLine?.ServiceLineName ?? placeholderAllocation.ServiceLineName,
                                   CurrentLevelGrade = placeholderAllocation.CurrentLevelGrade,
                                   Allocation = (short?)placeholderAllocation.Allocation,
                                   StartDate = placeholderAllocation.StartDate,
                                   EndDate = placeholderAllocation.EndDate,
                                   InvestmentCode = (short?)placeholderAllocation.InvestmentCode,
                                   InvestmentName = placeholderAllocation?.InvestmentName,
                                   CaseRoleCode = placeholderAllocation.CaseRoleCode,
                                   CaseRoleName = placeholderAllocation.CaseRoleName,
                                   LastUpdatedBy = placeholderAllocation.LastUpdatedBy,
                                   Notes = placeholderAllocation.Notes ?? "",
                                   IsPlaceholderAllocation = placeholderAllocation.IsPlaceholderAllocation,
                                   ManagingOfficeCode = caseItem?.ManagingOfficeCode ?? opportunityManagingOffice?.OfficeCode,
                                   ManagingOfficeAbbreviation = caseItem?.ManagingOfficeAbbreviation ?? opportunityManagingOffice?.OfficeAbbreviation,
                                   ManagingOfficeName = caseItem?.ManagingOfficeName ?? opportunityManagingOffice?.OfficeName,
                                   BillingOfficeCode = caseItem?.BillingOfficeCode ?? opportunityBillingOffice?.OfficeCode,
                                   BillingOfficeAbbreviation = caseItem?.BillingOfficeAbbreviation ?? opportunityBillingOffice?.OfficeAbbreviation,
                                   BillingOfficeName = caseItem?.BillingOfficeName ?? opportunityBillingOffice?.OfficeName,
                                   OperatingOfficeCode = (short?)placeholderAllocation.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   OperatingOfficeName = office?.OfficeName,

                                   CommitmentTypeCode = placeholderAllocation.CommitmentTypeCode,
                                   CommitmentTypeName = commitmentItem?.CommitmentTypeName,
                                   IsConfirmed = placeholderAllocation?.isConfirmed,
                                   PositionGroupCode = placeholderAllocation?.PositionGroupCode,
                                   JoiningDate = resource?.StartDate,
                                   InternetAddress = resource?.InternetAddress,
                               }).ToList();

            return allocations;
        }

        private static IEnumerable<ScheduleMasterPlaceholder> ConvertToScheduleMasterPlaceholderModel(
             IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations, IEnumerable<Resource> resources,
             IEnumerable<CaseData> cases, IEnumerable<OpportunityData> opportunityData, IEnumerable<PlanningCard> planningCards,
             IEnumerable<Office> offices, IEnumerable<CommitmentType> commitmentTypes)
        {
            //data cannot be NULL when used in JOINS. Hence, assigning cases to empty when null
            cases = cases ?? Enumerable.Empty<CaseData>();
            opportunityData = opportunityData ?? Enumerable.Empty<OpportunityData>();
            planningCards = planningCards ?? Enumerable.Empty<PlanningCard>();

            var allocations = (from placeholderAllocation in placeholderAllocations
                               join ccm in cases on placeholderAllocation.OldCaseCode equals ccm?.OldCaseCode into resCasesGroups
                               from caseItem in resCasesGroups.DefaultIfEmpty()
                               join opp in opportunityData on placeholderAllocation?.PipelineId equals opp?.PipelineId into resOppsGroups
                               from opportunityItem in resOppsGroups.DefaultIfEmpty()
                               join pc in planningCards on placeholderAllocation?.PlanningCardId equals pc?.Id into resPlanningCardsGroups
                               from planningCardItem in resPlanningCardsGroups.DefaultIfEmpty()
                               join res in resources on placeholderAllocation.EmployeeCode equals res.EmployeeCode into resAllocGroups
                               from resource in resAllocGroups.DefaultIfEmpty()
                               join o in offices on Convert.ToInt64(placeholderAllocation.OperatingOfficeCode) equals o.OfficeCode into resAllocOffices
                               from office in resAllocOffices.DefaultIfEmpty()
                               join res in resources on opportunityItem?.CoordinatingPartnerCode equals res.EmployeeCode into oppCoordPartner
                               from coordinatingPartner in oppCoordPartner.DefaultIfEmpty()
                               join res in resources on opportunityItem?.BillingPartnerCode equals res.EmployeeCode into oppBillPartner
                               from billingPartner in oppBillPartner.DefaultIfEmpty()
                               join o in offices on coordinatingPartner?.SchedulingOffice.OfficeCode equals o.OfficeCode into resAllocManagerOffices
                               from opportunityManagingOffice in resAllocManagerOffices.DefaultIfEmpty()
                               join o in offices on billingPartner?.SchedulingOffice.OfficeCode equals o.OfficeCode into resAllocBillingOffices
                               from opportunityBillingOffice in resAllocBillingOffices.DefaultIfEmpty()
                               join comm in commitmentTypes on placeholderAllocation.CommitmentTypeCode equals comm?.CommitmentTypeCode into resCommitmentType
                               from commitmentItem in resCommitmentType.DefaultIfEmpty()
                               select new ScheduleMasterPlaceholder()
                               {
                                   Id = placeholderAllocation.Id ?? Guid.NewGuid(),
                                   PlanningCardId = placeholderAllocation.PlanningCardId,
                                   OldCaseCode = placeholderAllocation.OldCaseCode,
                                   CaseCode = caseItem?.CaseCode,
                                   CaseName = caseItem?.CaseName,
                                   ClientCode = caseItem?.ClientCode ?? opportunityItem?.ClientCode,
                                   ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                   OpportunityName = opportunityItem?.OpportunityName,
                                   PipelineId = placeholderAllocation?.PipelineId,
                                   CaseTypeCode = caseItem?.CaseTypeCode,
                                   CaseTypeName = caseItem?.CaseType,
                                   EmployeeName = resource?.FullName,
                                   EmployeeCode = placeholderAllocation.EmployeeCode,
                                   ServiceLineCode = resource?.ServiceLine?.ServiceLineCode ?? placeholderAllocation.ServiceLineCode,
                                   ServiceLineName = resource?.ServiceLine?.ServiceLineName ?? placeholderAllocation.ServiceLineName,
                                   CurrentLevelGrade = placeholderAllocation.CurrentLevelGrade,
                                   OperatingOfficeCode = placeholderAllocation.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   OperatingOfficeName = office?.OfficeName,
                                   Allocation = placeholderAllocation.Allocation,
                                   StartDate = placeholderAllocation.StartDate,
                                   EndDate = placeholderAllocation.EndDate,
                                   InvestmentCode = placeholderAllocation.InvestmentCode,
                                   InvestmentName = placeholderAllocation?.InvestmentName,
                                   CaseRoleCode = placeholderAllocation.CaseRoleCode,
                                   CaseRoleName = placeholderAllocation.CaseRoleName,
                                   LastUpdatedBy = placeholderAllocation.LastUpdatedBy,
                                   Notes = placeholderAllocation.Notes ?? "",
                                   IsPlaceholderAllocation = placeholderAllocation.IsPlaceholderAllocation,
                                   ManagingOfficeCode = caseItem?.ManagingOfficeCode ?? opportunityManagingOffice?.OfficeCode,
                                   ManagingOfficeAbbreviation = caseItem?.ManagingOfficeAbbreviation ?? opportunityManagingOffice?.OfficeAbbreviation,
                                   ManagingOfficeName = caseItem?.ManagingOfficeName ?? opportunityManagingOffice?.OfficeName,
                                   BillingOfficeCode = caseItem?.BillingOfficeCode ?? opportunityBillingOffice?.OfficeCode,
                                   BillingOfficeAbbreviation = caseItem?.BillingOfficeAbbreviation ?? opportunityBillingOffice?.OfficeAbbreviation,
                                   BillingOfficeName = caseItem?.BillingOfficeName ?? opportunityBillingOffice?.OfficeName,
                                   CommitmentTypeCode = placeholderAllocation.CommitmentTypeCode,
                                   CommitmentTypeName = commitmentItem?.CommitmentTypeName,
                                   IsConfirmed = placeholderAllocation?.IsConfirmed,
                                   IsPlanningCardShared = placeholderAllocation.IsPlanningCardShared,
                                   PlanningCardTitle = placeholderAllocation.PlanningCardTitle,
                                   ProbabilityPercent = opportunityItem?.ProbabilityPercent,
                                   IncludeInCapacityReporting = placeholderAllocation?.IncludeInCapacityReporting
                               }).ToList();

            return allocations;
        }
        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(
            IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations, IEnumerable<CaseData> cases,
            IEnumerable<OpportunityData> opportunityData, IEnumerable<PlanningCard> planningCardData, IEnumerable<Resource> resources, IEnumerable<InvestmentCategory> investmentCategories,
            IEnumerable<CommitmentType> commitmentTypes, IEnumerable<SKUDemand> skuTerms)
        {
            //data cannot be NULL when used in JOINS. Hence, assigning cases to empty when null
            cases = cases ?? Enumerable.Empty<CaseData>();
            opportunityData = opportunityData ?? Enumerable.Empty<OpportunityData>();
            planningCardData = planningCardData ?? Enumerable.Empty<PlanningCard>();

            var resourceAllocationsViewModel = (from placeholderAllocation in placeholderAllocations
                                                join caseData in cases on placeholderAllocation.OldCaseCode equals caseData.OldCaseCode into caseAllocGroups
                                                from caseItem in caseAllocGroups.DefaultIfEmpty()
                                                join opp in opportunityData on placeholderAllocation.PipelineId equals opp?.PipelineId into resOppsGroups
                                                from opportunityItem in resOppsGroups.DefaultIfEmpty()
                                                join planningCard in planningCardData on placeholderAllocation.PlanningCardId equals planningCard?.Id into resPlanningCardGroups
                                                from planningCardItem in resPlanningCardGroups.DefaultIfEmpty()
                                                join ic in investmentCategories on placeholderAllocation.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                                                from investmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                                                join res in resources on placeholderAllocation.EmployeeCode equals res.EmployeeCode into resAllocGroups
                                                from resource in resAllocGroups.DefaultIfEmpty()
                                                join ct in commitmentTypes on placeholderAllocation.CommitmentTypeCode equals ct?.CommitmentTypeCode into resCommitmentTypeList
                                                from commitmentType in resCommitmentTypeList.DefaultIfEmpty()
                                                select new ResourceAssignmentViewModel
                                                {
                                                    Id = placeholderAllocation.Id,
                                                    PlanningCardId = placeholderAllocation.PlanningCardId,
                                                    PlanningCardTitle = planningCardItem?.Name,
                                                    OldCaseCode = placeholderAllocation.OldCaseCode,
                                                    CaseName = caseItem?.CaseName,
                                                    CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                                    ClientName = caseItem?.ClientName,
                                                    PipelineId = placeholderAllocation.PipelineId,
                                                    OpportunityName = opportunityItem?.OpportunityName,
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
                                                    CaseStartDate = caseItem?.StartDate,
                                                    CaseEndDate = caseItem?.EndDate,
                                                    OpportunityStartDate = opportunityItem?.StartDate,
                                                    OpportunityEndDate = opportunityItem?.EndDate,
                                                    LastUpdatedBy = placeholderAllocation.LastUpdatedBy,
                                                    Notes = placeholderAllocation.Notes ?? "",
                                                    ServiceLineName = placeholderAllocation.ServiceLineName,
                                                    ServiceLineCode = placeholderAllocation.ServiceLineCode,
                                                    IsPlaceholderAllocation = placeholderAllocation.IsPlaceholderAllocation,
                                                    CommitmentTypeCode = placeholderAllocation.CommitmentTypeCode,
                                                    CommitmentTypeName = commitmentType?.CommitmentTypeName,
                                                    PositionGroupCode = placeholderAllocation?.PositionGroupCode,
                                                    IncludeInCapacityReporting = planningCardItem?.IncludeInCapacityReporting,
                                                    IsPlanningCardShared = planningCardItem?.IsShared,
                                                    SkuTerms = skuTerms?.Where(x => x.PipelineId == placeholderAllocation.PipelineId ||
                                                        x.OldCaseCode == placeholderAllocation.OldCaseCode ||
                                                        x.PlanningCardId == placeholderAllocation.PlanningCardId),
                                                    CombinedSkuTerm = CalculateCombinedSkuTerm(skuTerms?.Where(x => x.PipelineId == placeholderAllocation.PipelineId ||
                                                        x.OldCaseCode == placeholderAllocation.OldCaseCode ||
                                                        x.PlanningCardId == placeholderAllocation.PlanningCardId))
                                                }).ToList();

            return resourceAllocationsViewModel;
        }

        private static string CalculateCombinedSkuTerm(IEnumerable<SKUDemand> skuDemands)
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
        #endregion

    }
}
