using Staffing.HttpAggregator.Contracts;
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
    public class ResourceAllocationService : IResourceAllocationService
    {
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IPipelineApiClient _pipelineApiClient;
        public ResourceAllocationService(IStaffingApiClient staffingApiClient,
            IResourceApiClient resourceApiClient, ICCMApiClient ccmApiClient,
            IPipelineApiClient pipelineApiClient)
        {
            _staffingApiClient = staffingApiClient;
            _resourceApiClient = resourceApiClient;
            _ccmApiClient = ccmApiClient;
            _pipelineApiClient = pipelineApiClient;
        }
        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourceAllocations(IEnumerable<ResourceAssignmentViewModel> resourceAllocations)
        {
            if (!resourceAllocations.Any())
            {
                return Enumerable.Empty<ResourceAssignmentViewModel>();
            }

            var listOldCaseCodes = string.Join(",", resourceAllocations.Where(y => !string.IsNullOrEmpty(y.OldCaseCode))
                .Select(x => x.OldCaseCode).Distinct());
            //Allocations might get converted from Opp to Case. In that case we need their Opp name for analytics data. Hence no check for oldcasecode 
            var listPipelineIds = string.Join(",", resourceAllocations.Where(y => y.PipelineId != null && y.PipelineId != Guid.Empty)
                .Select(x => x.PipelineId.ToString()).Distinct());

            var resourcesTask = _resourceApiClient.GetEmployees();
            var officesTask = _ccmApiClient.GetOfficeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var caseRoleTypesTask = _staffingApiClient.GetCaseRoleTypeList();
            var caseDataTask = !string.IsNullOrEmpty(listOldCaseCodes) ? _ccmApiClient.GetCaseDataByCaseCodes(listOldCaseCodes) : null;
            var opportunityDataTask = !string.IsNullOrEmpty(listPipelineIds)
                ? _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(listPipelineIds)
                : null;
            if (caseDataTask != null)
                await Task.WhenAll(resourcesTask, officesTask, investmentCategoriesTask, caseRoleTypesTask, caseDataTask);
            else
                await Task.WhenAll(resourcesTask, officesTask, investmentCategoriesTask, caseRoleTypesTask, opportunityDataTask);

            var resources = resourcesTask.Result;
            var offices = officesTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var caseRoleTypes = caseRoleTypesTask.Result;
            var casesData = caseDataTask?.Result;
            var opportunityData = opportunityDataTask?.Result;

            var allocations = ConvertToResourceAllocationModel(resourceAllocations, resources, offices, casesData, opportunityData,
                investmentCategories, caseRoleTypes);

            var allocatedResources = await _staffingApiClient.UpsertResourceAllocations(allocations);

            return ConvertToResourceAssignmentViewModel(allocatedResources, casesData, opportunityData, investmentCategories, caseRoleTypes);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByEmployeeCodes(
           string employeeCodes, DateTime? startDate, DateTime? endDate, List<Resource> resources = null)
        {
            if (string.IsNullOrEmpty(employeeCodes)) return Enumerable.Empty<ResourceAssignmentViewModel>();

            var resourcesStaffingTask = _staffingApiClient.GetResourceAllocationsByEmployeeCodes(employeeCodes, startDate, endDate);
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var resourcesTask = resources == null || resources.Count() == 0
                    ? _resourceApiClient.GetEmployeesIncludingTerminated()
                    : Task.FromResult(new List<Resource>());

            await Task.WhenAll(resourcesStaffingTask, investmentCategoriesTask, resourcesTask);

            var resourcesStaffing = resourcesStaffingTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var listCaseCodes = resourcesStaffing.Select(x => x.OldCaseCode).Distinct();
            var listPipeLineId = resourcesStaffing.Select(x => x.PipelineId).Distinct();

            var casesDataTask = _ccmApiClient.GetCaseDataByCaseCodes(string.Join(",", listCaseCodes));
            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(string.Join(",", listPipeLineId));

            await Task.WhenAll(casesDataTask, opportunityDataTask);

            var caseData = casesDataTask.Result;
            var opportunityData = opportunityDataTask.Result;
            var caseManagerCodes = caseData.Select(x => x.CaseManagerCode).Distinct();

            if (resources == null || resources.Count() == 0)
                resources = resourcesTask.Result.Where(x => employeeCodes.Contains(x.EmployeeCode) || caseManagerCodes.Contains(x.EmployeeCode)).ToList();
            else
                resources = resources.Where(x => employeeCodes.Contains(x.EmployeeCode) || caseManagerCodes.Contains(x.EmployeeCode)).ToList();
 

            return ConvertToResourceAssignmentViewModel(resourcesStaffing, caseData, opportunityData, investmentCategories, resources);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByEmployeeCodes(
           string employeeCodes, DateTime? startDate, DateTime? endDate, List<Resource> resources = null)
        {
            if (string.IsNullOrEmpty(employeeCodes)) return Enumerable.Empty<ResourceAssignmentViewModel>();

            var resourcesStaffingTask = _staffingApiClient.GetPlaceholderAllocationsByEmployeeCodes(employeeCodes, startDate, endDate);
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();

            var resourcesTask = resources == null || resources.Count() == 0
                    ? _resourceApiClient.GetEmployeesIncludingTerminated()
                    : Task.FromResult(new List<Resource>());

            await Task.WhenAll(resourcesStaffingTask, investmentCategoriesTask, resourcesTask);

            var resourcesStaffing = resourcesStaffingTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;

            if (resources == null || resources.Count() == 0)
                resources = resourcesTask.Result.Where(x => employeeCodes.Contains(x.EmployeeCode)).ToList();
            else
                resources = resources.Where(x => employeeCodes.Contains(x.EmployeeCode)).ToList();

            var listCaseCodes = resourcesStaffing.Select(x => x.OldCaseCode).Distinct();
            var listPipeLineId = resourcesStaffing.Select(x => x.PipelineId).Distinct();

            var casesDataTask = _ccmApiClient.GetCaseDataBasicByCaseCodes(string.Join(",", listCaseCodes));
            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(string.Join(",", listPipeLineId));

            await Task.WhenAll(casesDataTask, opportunityDataTask);

            var caseData = casesDataTask.Result;
            var opportunityData = opportunityDataTask.Result;

            return ConvertToResourceAssignmentViewModel(resourcesStaffing, caseData, opportunityData, investmentCategories, resources);
        }

        private async Task<IEnumerable<ResourceAssignmentViewModel>> GetUpdatedPrePostAllocationsOverlappedByCaseRoll(IList<ResourceAssignmentViewModel> resourceAllocations)
        {
            var prePostallocations = resourceAllocations.Where(x => x.InvestmentCode == 4);
            resourceAllocations = resourceAllocations.Where(x => x.InvestmentCode != 4 || x.InvestmentCode == null).ToList();
            var allocationIdsToDelete = new List<Guid>();
            var allocationsToUpsert = new List<ResourceAssignmentViewModel>();
            if (prePostallocations.Any())
            {
                foreach (var allocation in resourceAllocations)
                {
                    allocationsToUpsert.Add(allocation);

                    foreach (var prePostAllocation in prePostallocations.Where(x => x.EmployeeCode == allocation.EmployeeCode && x.OldCaseCode == allocation.OldCaseCode))
                    {
                        //if allocation and pre-post are partially-ovelapping
                        if (allocation.EndDate < prePostAllocation.EndDate && allocation.EndDate >= prePostAllocation.StartDate)
                        {
                            prePostAllocation.StartDate = allocation.EndDate.Value.AddDays(1);
                            allocationsToUpsert.Add(prePostAllocation);
                        }
                        //if allocation and pre-post are fully-ovelapping
                        else if (allocation.EndDate >= prePostAllocation.EndDate)
                        {
                            allocationIdsToDelete.Add((Guid)prePostAllocation.Id);
                        }
                        //if allocation and pre-post are non-ovelapping
                        else if (allocation.StartDate > prePostAllocation.EndDate || allocation.EndDate < prePostAllocation.StartDate)
                        {
                            allocationsToUpsert.Add(prePostAllocation);
                        }
                    }
                }


                var listAllocationIdsToDelete = string.Join(",", allocationIdsToDelete.Distinct());

                if (!string.IsNullOrEmpty(listAllocationIdsToDelete))
                {
                    await _staffingApiClient.DeleteResourceAllocationByIds(listAllocationIdsToDelete, "Case Roll");
                }
            }
            else
            {
                allocationsToUpsert.AddRange(resourceAllocations);
            }

            return allocationsToUpsert;

        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpsertCaseRollsAndAllocations(IEnumerable<CaseRoll> caseRolls, IEnumerable<ResourceAssignmentViewModel> resourceAllocations)
        {
            var caseRollTask = _staffingApiClient.UpsertCaseRolls(caseRolls);

            var updatedAllocationsOverlappedByPrePost = await GetUpdatedPrePostAllocationsOverlappedByCaseRoll(resourceAllocations.ToList());

            var allocationsTask = UpsertResourceAllocations(updatedAllocationsOverlappedByPrePost);

            await Task.WhenAll(caseRollTask, allocationsTask);

            return allocationsTask.Result;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> RevertCaseRollAndAllocations(CaseRoll caseRoll, IEnumerable<ResourceAssignmentViewModel> resourceAllocations)
        {
            var rolledScheduleIdsToDelete = string.Join(",", resourceAllocations.Select(x => x.Id));
            var deleteRolledAllocationTask = _staffingApiClient.DeleteRolledAllocationsByScheduleIds(rolledScheduleIdsToDelete, caseRoll.LastUpdatedBy);

            var caseRollTask = Task.FromResult<string>(null);
            var rolledScheduleIds = caseRoll.RolledScheduleIds.Split(",").ToList();
            if (!rolledScheduleIds.Select(x => x.ToLower()).Except(resourceAllocations.Select(x => x.Id.ToString().ToLower())).Any())
            {
                caseRollTask = _staffingApiClient.DeleteCaseRollsByIds(caseRoll.Id.ToString(), caseRoll.LastUpdatedBy);
            }
            var allocationsTask = UpsertResourceAllocations(resourceAllocations);

            await Task.WhenAll(caseRollTask, allocationsTask, deleteRolledAllocationTask);

            return allocationsTask.Result;
        }

        public async Task<IEnumerable<CaseRoleAllocationViewModel>> GetCaseRoleAllocationsByOldCaseCodes(string oldCaseCodes)
        {
            if (string.IsNullOrEmpty(oldCaseCodes))
                return Enumerable.Empty<CaseRoleAllocationViewModel>();

            var resourcesTask = _resourceApiClient.GetEmployees();
            var resourcesAllocationsTask = _staffingApiClient.GetResourceAllocationsByCaseCodes(oldCaseCodes);
            await Task.WhenAll(resourcesTask, resourcesAllocationsTask);
            var resources = resourcesTask.Result;
            var resourcesAllocations = resourcesAllocationsTask.Result.Where(x => x.CaseRoleCode != null).ToList();

            if (resourcesAllocations.Count() < 1)
                return Enumerable.Empty<CaseRoleAllocationViewModel>();

            return GetCaseRoleAllocations(resourcesAllocations, resources);
        }

        public async Task<IEnumerable<CaseRoleAllocationViewModel>> GetCaseRoleAllocationsByPipelineIds(string pipelineIds)
        {
            if (string.IsNullOrEmpty(pipelineIds))
                return Enumerable.Empty<CaseRoleAllocationViewModel>();

            var resourcesTask = _resourceApiClient.GetEmployees();
            var resourcesAllocationsTask = _staffingApiClient.GetResourceAllocationsByPipelineIds(pipelineIds);
            await Task.WhenAll(resourcesTask, resourcesAllocationsTask);
            var resources = resourcesTask.Result;
            var resourcesAllocations = resourcesAllocationsTask.Result.Where(x => x.CaseRoleCode != null).ToList();

            if (resourcesAllocations.Count() < 1)
                return Enumerable.Empty<CaseRoleAllocationViewModel>();

            return GetCaseRoleAllocations(resourcesAllocations, resources);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date)
        {
            if (string.IsNullOrEmpty(employeeCode))
                return Enumerable.Empty<ResourceAssignmentViewModel>();

            var resourcesTask = _resourceApiClient.GetEmployees();
            var officesTask = _ccmApiClient.GetOfficeList();
            var resourceAllocationsTask = _staffingApiClient.GetLastTeamByEmployeeCode(employeeCode, date);
            var caseRoleTypesTask = _staffingApiClient.GetCaseRoleTypeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();

            await Task.WhenAll(resourcesTask, officesTask, resourceAllocationsTask, caseRoleTypesTask, investmentCategoriesTask);

            var resources = resourcesTask.Result;
            var offices = officesTask.Result;
            var resourceAllocations = resourceAllocationsTask.Result;
            var caseRoleTypes = caseRoleTypesTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;

            var listCaseCodes = resourceAllocations.Select(x => x.OldCaseCode).Distinct();
            var listPipeLineId = resourceAllocations.Select(x => x.PipelineId).Distinct();

            var casesDataTask = _ccmApiClient.GetCaseDataByCaseCodes(string.Join(",", listCaseCodes));
            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesByPipelineIds(string.Join(",", listPipeLineId));

            await Task.WhenAll(casesDataTask, opportunityDataTask);

            var casesData = casesDataTask.Result;
            var opportunityData = opportunityDataTask.Result;

            var allocations = ConvertToResourceAllocationModel(resourceAllocations, resources, offices, casesData, opportunityData,
                investmentCategories, caseRoleTypes);
            allocations = allocations.Where(x => !string.IsNullOrEmpty(x.EmployeeName)); // NOTE: Only get the allocations for active resources.

            return ConvertToResourceAssignmentViewModel(allocations, casesData, opportunityData, investmentCategories, caseRoleTypes);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetAllocationsWithinDateRangeForOfficeClosure(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate, string staffingTags)
        {
            if (string.IsNullOrEmpty(officeCodes)
                || string.IsNullOrEmpty(caseTypeCodes)
                || startDate == null || startDate == DateTime.MinValue
                || endDate == null || endDate == DateTime.MinValue)
            {
                return Enumerable.Empty<ResourceAssignmentViewModel>();
            }
            if (string.IsNullOrEmpty(staffingTags))
            {
                staffingTags = Constants.ServiceLineCodes.GeneralConsulting;
            }
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officesTask = _ccmApiClient.GetOfficeList();
            var resourceAllocationsTask = _staffingApiClient.GetResourceAllocationsBySelectedSupplyValues(officeCodes, startDate, endDate, staffingTags, null);
            var caseRoleTypesTask = _staffingApiClient.GetCaseRoleTypeList();
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            //var officeClosureChangesTask = _staffingApiClient.GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(officeCodes, caseTypeCodes, startDate, endDate);

            await Task.WhenAll(resourcesTask, officesTask, resourceAllocationsTask, caseRoleTypesTask, investmentCategoriesTask);

            var resources = resourcesTask.Result;
            var offices = officesTask.Result;
            var resourceAllocations = resourceAllocationsTask.Result;
            var caseRoleTypes = caseRoleTypesTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            //var officeClosureChanges = officeClosureChangesTask.Result;

            //var affectedCasesForClosure = officeClosureChanges.OldCaseCodes ?? "";

            resourceAllocations = resourceAllocations.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).ToList();

            var listCaseCodes = resourceAllocations.Select(x => x.OldCaseCode).Distinct();
            var casesData = await _ccmApiClient.GetCaseDataByCaseCodes(string.Join(",", listCaseCodes));

            var allocations = ConvertToResourceAllocationModel(resourceAllocations, resources, offices, casesData, null,
                investmentCategories, caseRoleTypes);

            var selectedCaseTypeCodes = caseTypeCodes.Split(',').ToList();

            allocations = allocations.Where(x => selectedCaseTypeCodes.Contains(x.CaseTypeCode.ToString())).ToList();

            return ConvertToResourceAssignmentViewModel(allocations, casesData, null, investmentCategories, caseRoleTypes);
        }

        #region Private Methods

        private IEnumerable<CaseRoleAllocationViewModel> GetCaseRoleAllocations(IList<ResourceAssignmentViewModel> resourcesAllocations, List<Resource> resources)
        {
            var allocations = (
                                    from resourceAllocation in resourcesAllocations
                                    join r in resources on resourceAllocation.EmployeeCode equals r.EmployeeCode
                                    select new CaseRoleAllocationViewModel
                                    {
                                        EmployeeCode = resourceAllocation.EmployeeCode,
                                        CaseRoleCode = resourceAllocation.CaseRoleCode,
                                        EmployeeName = r.FullName,
                                        OfficeCode = r.SchedulingOffice.OfficeCode,
                                        OfficeAbbreviation = r.SchedulingOffice.OfficeAbbreviation,
                                        OldCaseCode = resourceAllocation.OldCaseCode,
                                        PipelineId = Convert.ToString(resourceAllocation.PipelineId)
                                    }
                                ).ToList();
            return allocations;
        }

        private static IEnumerable<ResourceAllocation> ConvertToResourceAllocationModel(
            IEnumerable<ResourceAssignmentViewModel> resourceAllocations, IEnumerable<Resource> resources,
            IEnumerable<Office> offices, IEnumerable<CaseData> cases, IEnumerable<OpportunityData> opportunityData, IEnumerable<InvestmentCategory> investmentCategories,
            IEnumerable<CaseRoleType> caseRoleTypes)
        {
            //data cannot be NULL when used in JOINS. Hence, assigning cases to empty when null
            cases = cases ?? Enumerable.Empty<CaseData>();
            opportunityData = opportunityData ?? Enumerable.Empty<OpportunityData>();

            var allocations = (from resourceAllocation in resourceAllocations
                               join ccm in cases on resourceAllocation.OldCaseCode equals ccm?.OldCaseCode into resCasesGroups
                               from caseItem in resCasesGroups.DefaultIfEmpty()
                               join opp in opportunityData on resourceAllocation.PipelineId equals opp?.PipelineId into resOppsGroups
                               from opportunityItem in resOppsGroups.DefaultIfEmpty()
                               join res in resources on resourceAllocation.EmployeeCode equals res.EmployeeCode into resAllocGroups
                               from resource in resAllocGroups.DefaultIfEmpty()
                               join o in offices on resourceAllocation.OperatingOfficeCode equals o.OfficeCode into resAllocOffices
                               from office in resAllocOffices.DefaultIfEmpty()
                               join ic in investmentCategories on resourceAllocation.InvestmentCode equals ic.InvestmentCode into resAllocInvestCat
                               from investmentCategory in resAllocInvestCat.DefaultIfEmpty()
                               join crt in caseRoleTypes on resourceAllocation.CaseRoleCode equals crt.CaseRoleCode into resAllocCaseRoleType
                               from caseRoleType in resAllocCaseRoleType.DefaultIfEmpty()
                               join res in resources on opportunityItem?.CoordinatingPartnerCode equals res.EmployeeCode into oppCoordPartner
                               from coordinatingPartner in oppCoordPartner.DefaultIfEmpty()
                               join res in resources on opportunityItem?.BillingPartnerCode equals res.EmployeeCode into oppBillPartner
                               from billingPartner in oppBillPartner.DefaultIfEmpty()
                               join o in offices on coordinatingPartner?.SchedulingOffice.OfficeCode equals o.OfficeCode into resAllocManagerOffices
                               from opportunityManagingOffice in resAllocManagerOffices.DefaultIfEmpty()
                               join o in offices on billingPartner?.SchedulingOffice.OfficeCode equals o.OfficeCode into resAllocBillingOffices
                               from opportunityBillingOffice in resAllocBillingOffices.DefaultIfEmpty()
                               select new ResourceAllocation()
                               {
                                   Id = resourceAllocation.Id ?? Guid.NewGuid(),
                                   OldCaseCode = resourceAllocation.OldCaseCode,
                                   CaseName = caseItem?.CaseName,
                                   CaseCode = caseItem?.CaseCode,
                                   CaseTypeCode = caseItem?.CaseTypeCode,
                                   CaseTypeName = caseItem?.CaseType,
                                   ClientCode = caseItem?.ClientCode ?? opportunityItem?.ClientCode,
                                   ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                   PipelineId = resourceAllocation.PipelineId,
                                   OpportunityName = opportunityItem?.OpportunityName,
                                   EmployeeCode = resourceAllocation.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   Fte = resource?.FTE ?? 0,
                                   InternetAddress = resource?.InternetAddress,
                                   ServiceLineCode = resource?.ServiceLine?.ServiceLineCode,
                                   ServiceLineName = resource?.ServiceLine?.ServiceLineName,
                                   Position = resource?.Position?.PositionName,//TODO: remove after anaplan chnages
                                   PositionCode = resource?.Position?.PositionCode,
                                   PositionName = resource?.Position?.PositionName,
                                   PositionGroupName = resource?.Position?.PositionGroupName,
                                   CurrentLevelGrade = resourceAllocation.CurrentLevelGrade,
                                   BillCode = resource?.BillCode ?? 1,
                                   OperatingOfficeCode = (int)resourceAllocation.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   OperatingOfficeName = office?.OfficeName,
                                   ManagingOfficeCode = caseItem?.ManagingOfficeCode ?? opportunityManagingOffice?.OfficeCode,
                                   ManagingOfficeAbbreviation = caseItem?.ManagingOfficeAbbreviation ?? opportunityManagingOffice?.OfficeAbbreviation,
                                   ManagingOfficeName = caseItem?.ManagingOfficeName ?? opportunityManagingOffice?.OfficeName,
                                   BillingOfficeCode = caseItem?.BillingOfficeCode ?? opportunityBillingOffice?.OfficeCode,
                                   BillingOfficeAbbreviation = caseItem?.BillingOfficeAbbreviation ?? opportunityBillingOffice?.OfficeAbbreviation,
                                   BillingOfficeName = caseItem?.BillingOfficeName ?? opportunityBillingOffice?.OfficeName,
                                   Allocation = (int)resourceAllocation.Allocation,
                                   StartDate = (DateTime)resourceAllocation.StartDate,
                                   EndDate = resourceAllocation.EndDate,
                                   InvestmentCode = resourceAllocation.InvestmentCode,
                                   InvestmentName = investmentCategory?.InvestmentName,
                                   CaseRoleCode = resourceAllocation.CaseRoleCode,
                                   CaseRoleName = caseRoleType?.CaseRoleName,
                                   LastUpdatedBy = resourceAllocation.LastUpdatedBy,
                                   Notes = resourceAllocation.Notes ?? ""
                               }).ToList();

            return allocations;
        }

        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ResourceAllocation> resourceAllocations,
            IEnumerable<CaseData> cases, IEnumerable<OpportunityData> opportunityData, IEnumerable<InvestmentCategory> investmentCategories, IEnumerable<CaseRoleType> caseRoleTypes)
        {
            //data cannot be NULL when used in JOINS. Hence, assigning cases to empty when null
            cases = cases ?? Enumerable.Empty<CaseData>();
            opportunityData = opportunityData ?? Enumerable.Empty<OpportunityData>();

            var resourceAllocationsViewModel = (from resourceAllocation in resourceAllocations
                                                join caseData in cases on resourceAllocation.OldCaseCode equals caseData.OldCaseCode into resAllocGroups
                                                from caseItem in resAllocGroups.DefaultIfEmpty()
                                                join opp in opportunityData on resourceAllocation.PipelineId equals opp?.PipelineId into resOppsGroups
                                                from opportunityItem in resOppsGroups.DefaultIfEmpty()
                                                join ic in investmentCategories on resourceAllocation.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                                                from investmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                                                join crt in caseRoleTypes on resourceAllocation.CaseRoleCode equals crt.CaseRoleCode into resAllocCaseRoleType
                                                from caseRoleType in resAllocCaseRoleType.DefaultIfEmpty()
                                                select new ResourceAssignmentViewModel
                                                {
                                                    Id = resourceAllocation.Id,
                                                    OldCaseCode = resourceAllocation.OldCaseCode,
                                                    CaseName = resourceAllocation.CaseName,
                                                    CaseTypeCode = (Constants.CaseType?)resourceAllocation.CaseTypeCode,
                                                    ClientName = resourceAllocation.ClientName,
                                                    PipelineId = resourceAllocation.PipelineId,
                                                    OpportunityName = resourceAllocation.OpportunityName,
                                                    EmployeeCode = resourceAllocation.EmployeeCode,
                                                    EmployeeName = resourceAllocation.EmployeeName,
                                                    InternetAddress = resourceAllocation.InternetAddress,
                                                    CurrentLevelGrade = resourceAllocation.CurrentLevelGrade,
                                                    OperatingOfficeCode = resourceAllocation.OperatingOfficeCode,
                                                    OperatingOfficeAbbreviation = resourceAllocation.OperatingOfficeAbbreviation,
                                                    Allocation = resourceAllocation.Allocation,
                                                    StartDate = resourceAllocation.StartDate,
                                                    EndDate = resourceAllocation.EndDate,
                                                    InvestmentCode = resourceAllocation.InvestmentCode,
                                                    InvestmentName = investmentCategory?.InvestmentName,
                                                    CaseRoleCode = resourceAllocation.CaseRoleCode,
                                                    CaseRoleName = caseRoleType?.CaseRoleName,
                                                    CaseStartDate = caseItem?.StartDate,
                                                    CaseEndDate = caseItem?.EndDate,
                                                    OpportunityStartDate = opportunityItem?.StartDate,
                                                    OpportunityEndDate = opportunityItem?.EndDate,
                                                    ProbabilityPercent = opportunityItem?.ProbabilityPercent,
                                                    LastUpdatedBy = resourceAllocation.LastUpdatedBy,
                                                    Notes = resourceAllocation.Notes ?? ""
                                                }).ToList();

            return resourceAllocationsViewModel;
        }

        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ResourceAssignmentViewModel> resourceAllocations,
            IEnumerable<CaseData> cases, IEnumerable<OpportunityData> opportunities, IEnumerable<InvestmentCategory> investmentCategories,
            IEnumerable<Resource> resources)
        {
            var resourceAllocationsViewModel = (from allocation in resourceAllocations
                                                join caseData in cases on allocation.OldCaseCode equals caseData.OldCaseCode into resAllocGroups
                                                from caseItem in resAllocGroups.DefaultIfEmpty()
                                                join OpportunityData in opportunities on allocation.PipelineId equals OpportunityData.PipelineId into resAllocOppGroups
                                                from opportunityItem in resAllocOppGroups.DefaultIfEmpty()
                                                join ic in investmentCategories on allocation.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                                                from investmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                                                join re in resources on allocation.EmployeeCode.ToUpper() equals re.EmployeeCode.ToUpper() into resAllocResources
                                                from resource in resAllocResources.DefaultIfEmpty()
                                                join caseManagerRe in resources on caseItem?.CaseManagerCode?.ToUpper() equals caseManagerRe?.EmployeeCode.ToUpper() into caseManagerResources
                                                from caseMangagerResources in caseManagerResources.DefaultIfEmpty()
                                                select new ResourceAssignmentViewModel
                                                {
                                                    Id = allocation.Id,
                                                    OldCaseCode = allocation.OldCaseCode,
                                                    PipelineId = allocation.PipelineId,
                                                    CaseName = caseItem?.CaseName,
                                                    ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                                    OpportunityName = opportunityItem?.OpportunityName,
                                                    CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                                    EmployeeCode = allocation.EmployeeCode,
                                                    EmployeeName = resource.FullName,
                                                    CurrentLevelGrade = allocation.CurrentLevelGrade,
                                                    OperatingOfficeCode = allocation.OperatingOfficeCode,
                                                    ServiceLineCode = allocation.ServiceLineCode,
                                                    Allocation = allocation.Allocation,
                                                    StartDate = allocation.StartDate,
                                                    EndDate = allocation.EndDate,
                                                    InvestmentCode = allocation.InvestmentCode,
                                                    InvestmentName = investmentCategory?.InvestmentName,
                                                    CaseRoleCode = allocation.CaseRoleCode,
                                                    LastUpdatedBy = allocation.LastUpdatedBy,
                                                    Notes = allocation.Notes,
                                                    IsPlaceholderAllocation = allocation.IsPlaceholderAllocation,
                                                    CaseManagerName = caseMangagerResources?.FullName
                                                }).ToList();

            return resourceAllocationsViewModel;
        }

        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations,
    IEnumerable<CaseData> cases, IEnumerable<OpportunityData> opportunities, IEnumerable<InvestmentCategory> investmentCategories,
    IEnumerable<Resource> resources)
        {
            var resourceAllocationsViewModel = (from allocation in placeholderAllocations
                                                join caseData in cases on allocation.OldCaseCode equals caseData.OldCaseCode into resAllocGroups
                                                from caseItem in resAllocGroups.DefaultIfEmpty()
                                                join OpportunityData in opportunities on allocation.PipelineId equals OpportunityData.PipelineId into resAllocOppGroups
                                                from opportunityItem in resAllocOppGroups.DefaultIfEmpty()
                                                join ic in investmentCategories on allocation.InvestmentCode equals ic.InvestmentCode into resAllocInvestmentCat
                                                from investmentCategory in resAllocInvestmentCat.DefaultIfEmpty()
                                                join re in resources on allocation.EmployeeCode.ToUpper() equals re.EmployeeCode.ToUpper() into resAllocResources
                                                from resource in resAllocResources.DefaultIfEmpty()
                                                select new ResourceAssignmentViewModel
                                                {
                                                    Id = allocation.Id,
                                                    OldCaseCode = allocation.OldCaseCode,
                                                    PipelineId = allocation.PipelineId,
                                                    PlanningCardId = allocation.PlanningCardId,
                                                    PlanningCardTitle = allocation.PlanningCardTitle,
                                                    IsPlanningCardShared = allocation.IsPlanningCardShared,
                                                    CaseName = caseItem?.CaseName,
                                                    ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                                    OpportunityName = opportunityItem?.OpportunityName,
                                                    CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                                    EmployeeCode = allocation.EmployeeCode,
                                                    EmployeeName = resource.FullName,
                                                    CurrentLevelGrade = allocation.CurrentLevelGrade,
                                                    OperatingOfficeCode = allocation.OperatingOfficeCode,
                                                    ServiceLineCode = allocation.ServiceLineCode,
                                                    Allocation = allocation.Allocation,
                                                    StartDate = allocation.StartDate,
                                                    EndDate = allocation.EndDate,
                                                    InvestmentCode = allocation.InvestmentCode,
                                                    InvestmentName = investmentCategory?.InvestmentName,
                                                    CaseRoleCode = allocation.CaseRoleCode,
                                                    LastUpdatedBy = allocation.LastUpdatedBy,
                                                    Notes = allocation.Notes,
                                                    IsPlaceholderAllocation = allocation.IsPlaceholderAllocation,
                                                }).ToList();

            return resourceAllocationsViewModel;
        }
        #endregion
    }
}
