using Microsoft.Graph.Models.TermStore;
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
    public class StaffingService : IStaffingService
    {
        private readonly IPipelineApiClient _pipelineApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IBasisApiClient _basisApiClient;
        private readonly ICCMApiClient _ccmApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly ICommonResourcesService _commonResourcesService;
        private readonly ISignalRHubClient _signalRHubClient;

        public StaffingService(IPipelineApiClient pipelineApiClient, IStaffingApiClient staffingApiClient, ICCMApiClient ccmApiClient,
            IResourceApiClient resourceApiClient,
            ICommonResourcesService commonResourcesService,ISignalRHubClient signalRHubClient,
            IBasisApiClient basisApiClient)
        {
            _pipelineApiClient = pipelineApiClient;
            _staffingApiClient = staffingApiClient;
            _ccmApiClient = ccmApiClient;
            _resourceApiClient = resourceApiClient;
            _commonResourcesService = commonResourcesService;
            _signalRHubClient = signalRHubClient;
            _basisApiClient = basisApiClient;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetHistoricalStaffingAllocationsForEmployee(string employeeCode)
        {
            var allocationHistory = await _staffingApiClient.GetHistoricalStaffingAllocationsForEmployee(employeeCode);

            var pipelineIdList = string.Join(",", allocationHistory.Where(x => x.PipelineId.HasValue && string.IsNullOrEmpty(x.OldCaseCode)).Select(y => y.PipelineId).Distinct());
            var caseList = string.Join(",", allocationHistory.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).Select(y => y.OldCaseCode).Distinct());
            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineIdList);
            var caseDataTask = _ccmApiClient.GetCasesWithTaxonomiesByCaseCodes(caseList);
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(pipelineIdList);
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();

            await Task.WhenAll(opportunityDataTask, caseDataTask, officeListDataTask, resourcesDataTask, pipelineChangesTask);
            var opportunities = opportunityDataTask.Result;
            var cases = caseDataTask.Result;
            var offices = officeListDataTask.Result;
            var resources = resourcesDataTask.Result;
            var pipelineChanges = pipelineChangesTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            pipelineChanges.Join(opportunities, (pipeline) => pipeline.PipelineId, (opportunity) => opportunity.PipelineId, (pipeline, opportunity) =>
            {
                opportunity.StartDate = pipeline.StartDate ?? opportunity.StartDate;
                opportunity.EndDate = pipeline.EndDate ?? opportunity.EndDate;
                opportunity.ProbabilityPercent = pipeline.ProbabilityPercent ?? opportunity.ProbabilityPercent;

                return opportunity;
            }).ToList();

            var resourceHistoricalAllocations = ConvertToResourceAllocationModelByResourceWithCaseManagerDetails(allocationHistory, cases, opportunities,
                resources.FirstOrDefault(r => r.EmployeeCode == employeeCode), offices, caseRoleTypeList, resources);
            return resourceHistoricalAllocations;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetStaffingAllocationsForEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (effectiveFromDate == null) effectiveFromDate = DateTime.Now.Date;
            if (effectiveToDate.HasValue && effectiveFromDate > effectiveToDate)
            {
                throw new ArgumentException("EffectiveToDate should be greater than EffectiveFromDate");
            }
            var allocations = await _staffingApiClient.GetStaffingAllocationsForEmployee(employeeCode);

            if (allocations == null)
                return Enumerable.Empty<ResourceAssignmentViewModel>();
            var allocationBetweenDateRange = allocations.Where(allocation =>
            {
                if (effectiveToDate == null)
                    return allocation.EndDate >= effectiveFromDate;
                else
                    return allocation.StartDate <= effectiveToDate && allocation.EndDate >= effectiveFromDate;
            });

            var pipelineIdList = string.Join(",", allocationBetweenDateRange.Where(x => x.PipelineId.HasValue && string.IsNullOrEmpty(x.OldCaseCode)).Select(y => y.PipelineId).Distinct());
            var caseList = string.Join(",", allocationBetweenDateRange.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).Select(y => y.OldCaseCode).Distinct());

            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineIdList);
            var caseDataTask = _ccmApiClient.GetCasesWithTaxonomiesByCaseCodes(caseList);
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var resourcesDataTask = _resourceApiClient.GetEmployees();
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(pipelineIdList);
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();

            await Task.WhenAll(opportunityDataTask, caseDataTask, officeListDataTask, resourcesDataTask, pipelineChangesTask);

            var opportunities = opportunityDataTask.Result;
            var cases = caseDataTask.Result;
            var offices = officeListDataTask.Result;
            var resources = resourcesDataTask.Result;
            var pipelineChanges = pipelineChangesTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;

            pipelineChanges.Join(opportunities, (pipeline) => pipeline.PipelineId, (opportunity) => opportunity.PipelineId, (pipeline, opportunity) =>
            {
                opportunity.StartDate = pipeline.StartDate ?? opportunity.StartDate;
                opportunity.EndDate = pipeline.EndDate ?? opportunity.EndDate;
                opportunity.ProbabilityPercent = pipeline.ProbabilityPercent ?? opportunity.ProbabilityPercent;

                return opportunity;
            }).ToList();

            var resourceHistoricalAllocations = ConvertToResourceAllocationModelByResourceWithCaseManagerDetails(allocationBetweenDateRange, cases, opportunities,
                resources.FirstOrDefault(r => r.EmployeeCode == employeeCode), offices, caseRoleTypeList, resources);
            return resourceHistoricalAllocations;


        }

        public async Task<IEnumerable<ResourceView>> GetResourcesStaffingAndCommitments(SupplyFilterCriteria supplyFilterCriteria, string loggedInUser)
        {
            var resourcesStaffingAndCommitmentDataReferences = new ResourceView();
            IEnumerable<EmployeePracticeArea> practiceAreaAffiliations = new List<EmployeePracticeArea>();  
            // TODO: Create a better process to form OData Query
            const string oDataQuery = $"$select=employeeCode,levelGrade&$expand=Position($select=PositionCode,positionName)&$expand=serviceLine($select=serviceLineCode,serviceLineName)";
            
            var resourcesDataTask = _resourceApiClient.GetActiveEmployeesFilteredBySelectedValues(supplyFilterCriteria.OfficeCodes, supplyFilterCriteria.StartDate,
                supplyFilterCriteria.EndDate,"" , "", oDataQuery);

            var commitmentsTask =
                _staffingApiClient.GetCommitmentsWithinDateRange(supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate);

            await Task.WhenAll(resourcesDataTask, commitmentsTask);

            var resources = resourcesDataTask.Result;
            var commitments = commitmentsTask.Result;
            
            var unfilteredEmployeeCodes = string.Join(',', resources.Select(r => r.EmployeeCode).Distinct());

            var employeesWithStaffableAsRoles = !string.IsNullOrEmpty(supplyFilterCriteria.StaffableAsTypeCodes)
                ? await _staffingApiClient.GetResourceActiveStaffableAsByEmployeeCodes(unfilteredEmployeeCodes)
                : new List<StaffableAs>();

            resourcesStaffingAndCommitmentDataReferences.StaffableAsRoles = employeesWithStaffableAsRoles;

            var resourcesFilteredByLevelGradesAndPositions = _commonResourcesService.FilterResourcesByLevelGradePositionAndStaffableAs(resources,employeesWithStaffableAsRoles,supplyFilterCriteria);

            //resourcesStaffingAndCommitmentDataReferences.Commitments = commitments;

            var filteredResources = _commonResourcesService.GetResourcesFilteredByStaffingTags(supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate,
                resourcesFilteredByLevelGradesAndPositions, commitments, supplyFilterCriteria.StaffingTags);

            (filteredResources, practiceAreaAffiliations) = await _commonResourcesService.GetEmployeesFilteredByAdditionalFilters(supplyFilterCriteria,
                filteredResources, resourcesStaffingAndCommitmentDataReferences);

            return await GetFilteredResourcesWithAllocationsAndCommitments(filteredResources, resourcesStaffingAndCommitmentDataReferences, practiceAreaAffiliations,
                loggedInUser, supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate);


        }



        public async Task<IEnumerable<ResourceView>> GetFilteredResourcesGroupWithAllocations(SupplyGroupFilterCriteria supplyGroupFilterCriteria, string loggedInUser)
        {
            // TODO: Create a better process to form OData Query
            const string oDataQuery = $"$select=employeeCode,levelGrade,fullName&$expand=serviceLine($select=serviceLineCode,serviceLineName),office";

            var resourcesTask = _resourceApiClient.GetActiveEmployeesFilteredBySelectedGroupValues(supplyGroupFilterCriteria.EmployeeCodes,supplyGroupFilterCriteria.StartDate,supplyGroupFilterCriteria.EndDate,oDataQuery);

            var employeeWithPracticeAreaAffiliationsTask = GetEmployeesWithPracticeAreaAffiliationsDataTask(supplyGroupFilterCriteria.EmployeeCodes);

            await Task.WhenAll(resourcesTask, employeeWithPracticeAreaAffiliationsTask);

   
            var resources = await resourcesTask;
            var employeeWithPracticeAreaAffiliations = await employeeWithPracticeAreaAffiliationsTask;

            return await GetFilteredResourcesWithAllocationsAndCommitments(resources, null, employeeWithPracticeAreaAffiliations,
                loggedInUser, supplyGroupFilterCriteria.StartDate, supplyGroupFilterCriteria.EndDate);
        }
        public async Task<IEnumerable<NotesAlertViewModel>> GetNotesAlert(string employeeCode)
        {
            var notesAlertTask = _staffingApiClient.GetNotesAlert(employeeCode);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(notesAlertTask, resourcesTask);

            var resources = resourcesTask.Result;
            var noteList = notesAlertTask.Result;
            var pipelineIdList = string.Join(",", noteList.Where(x => x.PipelineId.HasValue && string.IsNullOrEmpty(x.OldCaseCode)).Select(y => y.PipelineId).Distinct());
            var caseList = string.Join(",", noteList.Where(x => !string.IsNullOrEmpty(x.OldCaseCode)).Select(y => y.OldCaseCode).Distinct());
            var opportunitiesDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineIdList);
            var casesDataTask = _ccmApiClient.GetCaseDataBasicByCaseCodes(caseList);

            await Task.WhenAll(opportunitiesDataTask, casesDataTask);

            var opportunities = opportunitiesDataTask.Result;
            var cases = casesDataTask.Result;

            var notesAlert = ConvertToNotesAlertViewModel(noteList, cases, opportunities, resources);

            return notesAlert;

        }

        public async Task<IEnumerable<NotesSharedWithGroupViewModel>> GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(string employeeCode)
        {
            var recentNoteSharedWithGroupsTask = _staffingApiClient.GetMostRecentSharedWithEmployeeGroupsForNotesByEmployeeCode(employeeCode);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            await Task.WhenAll(recentNoteSharedWithGroupsTask, resourcesTask);
            var resources = resourcesTask.Result;
            var recentNoteSharedWithGroupsList = recentNoteSharedWithGroupsTask.Result;

            IEnumerable<NotesSharedWithGroupViewModel> recentNoteSharedWithGroups = recentNoteSharedWithGroupsList.Select(SharedWithGroup => new NotesSharedWithGroupViewModel
            {
                Id = SharedWithGroup.Id,
                SharedWithEmployees = resources?
                    .Where(x => SharedWithGroup.SharedWithEmployeeCode.Split(',')
                    .Contains(x.EmployeeCode))
                    .Select(x => new Employee{ EmployeeCode = x.EmployeeCode, FullName = x.FullName })
                    .ToList()
            });

            return recentNoteSharedWithGroups;
        }

        private async Task<IEnumerable<ResourceView>> GetFilteredResourcesWithAllocationsAndCommitments(
            IEnumerable<Resource> resourcesWithSelectedFields, ResourceView resourcesStaffingAndCommitmentDataReferences, IEnumerable<EmployeePracticeArea> practiceAreaAffiliations,
            string loggedInUser = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var filteredEmployeeCodes = string.Join(',', resourcesWithSelectedFields.Select(r => r.EmployeeCode).Distinct());
            var officesDataTask = _ccmApiClient.GetOfficeList();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var resourcesCommitmentsTask = _commonResourcesService.GetResourcesAllocationsAndCommitmentsForResourcesTab(
                resourcesWithSelectedFields, startDate, endDate, null, resourcesStaffingAndCommitmentDataReferences, loggedInUser);
            var employeesWithCertificatesTask = _commonResourcesService.GetEmployeesWithCertificatesByEmployeeCodes(filteredEmployeeCodes);
            var employeesWithLanguagesTask = _commonResourcesService.GetEmployeesWithLanguagesByEmployeeCodes(filteredEmployeeCodes);
            var preferencesTask = _staffingApiClient.GetEmployeeStaffingPreferences(filteredEmployeeCodes);
            await Task.WhenAll(officesDataTask, caseRoleTypeListTask, resourcesCommitmentsTask, employeesWithCertificatesTask, employeesWithLanguagesTask, preferencesTask);

            var offices = officesDataTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var resourcesCommitments = resourcesCommitmentsTask.Result.resourcesStaffingAndCommitment;
            var resources = resourcesCommitmentsTask.Result.resources;
            var certificatesData = employeesWithCertificatesTask.Result;
            var languagesData = employeesWithLanguagesTask.Result;
            var preferencesData = preferencesTask.Result;

            var allocations = resourcesCommitments.Allocations.GroupBy(x => x.EmployeeCode).ToList();
            var placeholderAllocations = resourcesCommitments.PlaceholderAllocations.GroupBy(x => x.EmployeeCode).ToList();
            var placeholderAndPlanningCardAllocations = resourcesCommitments.PlaceholderAllocations.Where(x => x.IsPlanningCardShared.Equals(true)).GroupBy(y => y.EmployeeCode).ToList();
            var loAs = resourcesCommitments.LoAs.GroupBy(x => x.EmployeeCode).ToList();
            var commitments = resourcesCommitments.Commitments.GroupBy(x => x.EmployeeCode).ToList();
            var vacations = resourcesCommitments.Vacations.GroupBy(x => x.EmployeeCode).ToList();
            var trainings = resourcesCommitments.Trainings.GroupBy(x => x.EmployeeCode).ToList();
            var timeOffs = resourcesCommitments.TimeOffs.GroupBy(x => x.EmployeeCode).ToList();
            var holidays = resourcesCommitments.Holidays.GroupBy(x => x.EmployeeCode).ToList();
            var transfers = resourcesCommitments.Transfers.GroupBy(x => x.EmployeeCode).ToList();
            var transitions = resourcesCommitments.Transitions.GroupBy(x => x.EmployeeCode).ToList();
            var terminations = resourcesCommitments.Terminations.GroupBy(x => x.EmployeeCode).ToList();
            var staffableAsRoles = resourcesCommitments.StaffableAsRoles.GroupBy(x => x.EmployeeCode).ToList();
            var resourceViewNotes = resourcesCommitments.ResourceViewNotes.GroupBy(x => x.EmployeeCode).ToList();
            var resourceCD = resourcesCommitments.ResourceCD.GroupBy(x => x.EmployeeCode).ToList();
            var resourceCommercialModel = resourcesCommitments.ResourceCommercialModel.GroupBy(x => x.EmployeeCode).ToList();

            var preferences = preferencesData.GroupBy(x => x.EmployeeCode).ToList();

            string listPipelineId =
                string.Join(",", resourcesCommitments.Allocations.Where(x => x.PipelineId != null && x.PipelineId != Guid.Empty).Select(x => x.PipelineId.ToString())
                .Concat(resourcesCommitments.PlaceholderAllocations.Where(x => x.PipelineId != null && x.PipelineId != Guid.Empty).Select(x => x.PipelineId.ToString())).Distinct());
            string listOldCaseCode =
                string.Join(",", resourcesCommitments.Allocations.Where(x => x.OldCaseCode != null).Select(x => x.OldCaseCode)
                .Concat(resourcesCommitments.PlaceholderAllocations.Where(x => x.OldCaseCode != null).Select(x => x.OldCaseCode)).Distinct());

            var opportunitiesDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(listPipelineId);
            var casesDataTask = _ccmApiClient.GetCaseDataBasicByCaseCodes(listOldCaseCode);
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(listPipelineId);

            await Task.WhenAll(opportunitiesDataTask, casesDataTask, pipelineChangesTask);

            var opportunities = opportunitiesDataTask.Result;
            var cases = casesDataTask.Result;
            var pipelineChanges = pipelineChangesTask.Result;

            pipelineChanges.Join(opportunities, (pipeline) => pipeline.PipelineId, (opportunity) => opportunity.PipelineId, (pipeline, opportunity) =>
            {
                opportunity.StartDate = pipeline.StartDate ?? opportunity.StartDate;
                opportunity.EndDate = pipeline.EndDate ?? opportunity.EndDate;
                opportunity.ProbabilityPercent = pipeline.ProbabilityPercent ?? opportunity.ProbabilityPercent;

                return opportunity;
            }).ToList();

            var resourcesWithAllocations = new List<ResourceView>();

            foreach (var resource in resources)
            {
                resource.LastBillable = resourcesCommitments.LastBillableDates?.FirstOrDefault(x => x.EmployeeCode == resource.EmployeeCode);

                var resourceAllocation = new ResourceView
                {
                    Resource = resource,
                    Allocations = ConvertToResourceAllocationModelByResource(
                                        allocations.Where(rs => rs.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                                        cases, opportunities, resource, offices, caseRoleTypeList),
                    PlaceholderAllocations = ConvertToResourceAllocationModelByResource(
                                        placeholderAllocations.Where(rs => rs.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                                        cases, opportunities, resource, offices, caseRoleTypeList),
                    LoAs = loAs.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Commitments = commitments.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Vacations = vacations.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Trainings = trainings.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    TimeOffs = timeOffs.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Holidays = holidays.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Transfers = transfers.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Transitions = transitions.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Terminations = terminations.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    StaffableAsRoles = staffableAsRoles.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    ResourceViewNotes = resourceViewNotes.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    ResourceCD = resourceCD.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    ResourceCommercialModel = resourceCommercialModel.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Certificates = certificatesData.ToList().Where(x => x.EmployeeCode == resource.EmployeeCode).Select(x => x.Certifications).FirstOrDefault(),
                    Languages = languagesData.ToList().Where(x => x.EmployeeCode == resource.EmployeeCode).Select(x => x.Languages).FirstOrDefault(),
                    Preferences = preferences.Where(x => x.Key.Equals(resource.EmployeeCode)).SelectMany(grp => grp),
                    Affiliations = practiceAreaAffiliations.Where(e => e.EmployeeCode == resource.EmployeeCode && (e.RoleCode == "9" || e.RoleCode == "8")) // Retrieving only L1 (RoleCode 8) and L2 (RoleCode 9) affiliations
                };

                resourcesWithAllocations.Add(resourceAllocation);
            }
            return resourcesWithAllocations;
        }

        public async Task<ResourceViewNoteViewModel> UpsertResourceViewNote(ResourceViewNote resourceViewNote)
        {
            var notesTask = _staffingApiClient.UpsertResourceViewNote(resourceViewNote);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(notesTask, resourcesTask);

            var resources = resourcesTask.Result;
            var noteList = new List<ResourceViewNote>
            {
                notesTask.Result
            };

            var upsertedNote = _commonResourcesService.ConvertToResourceViewNotesViewModel(noteList, resources).FirstOrDefault();

            var response = _signalRHubClient.GetUpdateOnSharedNotes(resourceViewNote.SharedWith);
            
            return upsertedNote;
        }

        public async Task<ResourceViewCDViewModel> UpsertResourceRecentCD(ResourceCD resourceViewCD)
        {
            var cdsTask = _staffingApiClient.UpsertResourceCD(resourceViewCD);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(cdsTask, resourcesTask);

            var resources = resourcesTask.Result;
            var cdList = new List<ResourceCD>
            {
                cdsTask.Result
            };

            var upsertedCD = _commonResourcesService.ConvertToResourceCDViewModel(cdList, resources).FirstOrDefault();


            return upsertedCD;
        }

        public Task<IEnumerable<EmployeePracticeArea>> GetEmployeesWithPracticeAreaAffiliationsDataTask(string employeeCodes)
        {
            return _basisApiClient.GetPracticeAreaAffiliationsByEmployeeCodes(employeeCodes, null, null);
        }

        public async Task<ResourceViewCommercialModelViewModel> UpsertResourceCommercialModel(ResourceCommercialModel resourceViewCommercialModel)
        {
            var commercialModelTask = _staffingApiClient.UpsertResourceCommercialModel(resourceViewCommercialModel);
            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            await Task.WhenAll(commercialModelTask, resourcesTask);

            var resources = resourcesTask.Result;
            var commercialModelList = new List<ResourceCommercialModel>
            {
                commercialModelTask.Result
            };

            var upsertedCommercialModel = _commonResourcesService.ConvertToResourceCommercialModelViewModel(commercialModelList, resources).FirstOrDefault();


            return upsertedCommercialModel;
        }

        public async Task<StaffingResponsible> GetResourceStaffingResponsibeDataByEmployeeCode(string employeeCode)
        {
            if (string.IsNullOrEmpty(employeeCode))
                throw new ArgumentException("EmployeeCode cannot be null or empty");

            var staffingResponsibleData =
                await _staffingApiClient.GetResourceStaffingResponsibleData(employeeCode);

            StaffingResponsible staffingRespnsible = staffingResponsibleData.FirstOrDefault();

            if (staffingRespnsible == null)
            {
                return new StaffingResponsible();
            }

            string staffingResponsibleEmployeeCodes = staffingRespnsible.ResponsibleForStaffingCodes;
            var staffingResponsibleResourcesData = await _resourceApiClient.GetEmployeesByEmployeeCodes(staffingResponsibleEmployeeCodes);

            string pdLeadCodes = staffingRespnsible.pdLeadCodes;
            var pdLeadResourcesData = await _resourceApiClient.GetEmployeesByEmployeeCodes(pdLeadCodes);

            string notifyUponStaffingEmployeeCodes = staffingRespnsible.notifyUponStaffingCodes;
            var notifyUponStaffingResourcesData = await _resourceApiClient.GetEmployeesByEmployeeCodes(notifyUponStaffingEmployeeCodes);

            return new StaffingResponsible {
                Id = staffingRespnsible.Id,
                EmployeeCode = staffingRespnsible.EmployeeCode,
                ResponsibleForStaffingCodes = staffingResponsibleEmployeeCodes,
                pdLeadCodes = pdLeadCodes,
                notifyUponStaffingCodes = notifyUponStaffingEmployeeCodes,
                responsibleForStaffingDetails = ConvertToStaffingResponsibleMembersViewModel(staffingResponsibleResourcesData),
                pdLeadDetails = ConvertToStaffingResponsibleMembersViewModel(pdLeadResourcesData),
                notifyUponStaffingDetails = ConvertToStaffingResponsibleMembersViewModel(notifyUponStaffingResourcesData),
                LastUpdatedBy = staffingRespnsible.LastUpdatedBy
            };

        }

        public async Task<PlanningCardViewModel> UpsertPlanningCard(PlanningCard planningCard, string loggedInUser)
        {
            var upsertedPlanningCardTask = _staffingApiClient.UpsertPlanningCardData(planningCard);
            var investmentCategoriesTask = _staffingApiClient.GetInvestmentCategoryList();
            var commitmentTypesTask = _staffingApiClient.GetCommitmentTypeList();
            var employeesIncludingTerminatedTask = _resourceApiClient.GetEmployeesIncludingTerminated(); 
            var officesTask = _ccmApiClient.GetOfficeList();


            await Task.WhenAll(upsertedPlanningCardTask, investmentCategoriesTask, commitmentTypesTask, employeesIncludingTerminatedTask, officesTask);

            var upsertedPlanningCard = upsertedPlanningCardTask.Result;
            var investmentCategories = investmentCategoriesTask.Result;
            var commitmentTypes = commitmentTypesTask.Result;
            var employeesIncludingTerminated = employeesIncludingTerminatedTask.Result;
            var offices = officesTask.Result;
            var casePlanningViewNotes = Enumerable.Empty<CaseViewNote>();

            if (!string.IsNullOrEmpty(loggedInUser))
            {
                casePlanningViewNotes = await _staffingApiClient.GetCaseViewNotesByPlanningCardIds(upsertedPlanningCard.Id.ToString(), loggedInUser);

            }

            //now get the allocations and get the employeeCodes from here.
            List<string> resourceAllocationsEmployeeCodes = upsertedPlanningCard.allocations.Select(allocation => allocation.EmployeeCode).ToList();


            string employeeCodes = string.Join(", ", resourceAllocationsEmployeeCodes);

            var resourcesData = await _resourceApiClient.GetEmployeesByEmployeeCodes(employeeCodes);

            return new PlanningCardViewModel
            {
                Id = upsertedPlanningCard.Id,
                Name = upsertedPlanningCard.Name,
                StartDate = upsertedPlanningCard.StartDate,
                EndDate = upsertedPlanningCard.EndDate,
                Office = upsertedPlanningCard.Office,
                IsShared = upsertedPlanningCard.IsShared,
                SharedOfficeCodes = upsertedPlanningCard.SharedOfficeCodes,
                SharedOfficeAbbreviations = !string.IsNullOrEmpty(upsertedPlanningCard.SharedOfficeCodes) ? GetOfficeAbbreviationListForPlanningCards(upsertedPlanningCard.SharedOfficeCodes, offices) : null,
                SharedStaffingTags = upsertedPlanningCard.SharedStaffingTags,
                IncludeInCapacityReporting = upsertedPlanningCard.IncludeInCapacityReporting,
                PegOpportunityId = upsertedPlanningCard.PegOpportunityId,
                ProbabilityPercent = upsertedPlanningCard.ProbabilityPercent,
                CreatedBy = upsertedPlanningCard.CreatedBy,
                MergedCaseCode = upsertedPlanningCard.MergedCaseCode,
                isMerged = upsertedPlanningCard.IsMerged,
                allocations = ConvertToResourceAssignmentViewModel(upsertedPlanningCard.allocations, resourcesData, investmentCategories, commitmentTypes, upsertedPlanningCard).ToList(),
                CasePlanningViewNotes = ConvertToCaseViewNotesViewModel(casePlanningViewNotes.Where(x => x.PlanningCardId.Equals(upsertedPlanningCard.Id)).ToList(), employeesIncludingTerminated)
            };
        }

        public async Task<IEnumerable<Commitment>> UpsertResourcesCommitments(IList<Commitment> commitments)
        {
            var savedCommitments = await _staffingApiClient.UpsertResourceCommitment(commitments);

            TriggerSendNotifications(savedCommitments);

            return savedCommitments;

        }

        private async void TriggerSendNotifications(IEnumerable<Commitment> commitments)
        {
            try
            {
                await SendNotificationsToUsersAsync(commitments);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                Console.WriteLine($"Error in SendNotificationsToUsersAsync: {ex.Message}");
            }
        }

        private async Task SendNotificationsToUsersAsync(IEnumerable<Commitment> commitments)
        {
            var employeeCodes = string.Join(',', commitments.Select(c => c.EmployeeCode).Distinct());
            var resources = await _resourceApiClient.GetEmployeesByEmployeeCodes(employeeCodes);

            var commitmentData = 
                new CommitmentEnrichment
                {
                    commitments = commitments.ToList(),
                    resources = resources.ToList()
                };

            var upsertedCommitments = await _staffingApiClient.UpsertRingfenceCommitmentAlerts(commitmentData);

            if (upsertedCommitments.Any())
            {
                var employeeCodesToNotify = String.Join(',', upsertedCommitments.Select(c => c.EmployeeCode).Distinct());
                await _signalRHubClient.GetUpdateOnRingfenceCommitmentsAlert(employeeCodesToNotify);
            }

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

        private string GetOfficeAbbreviationListForPlanningCards(string officeCodes, IEnumerable<Office> offices)
        {
            return string.Join(",", offices.Where(x => officeCodes.Contains(x.OfficeCode.ToString())).OrderBy(z => z.OfficeAbbreviation).Select(y => y.OfficeAbbreviation));
        }


        private static IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations,
            IEnumerable<Resource> resources, IEnumerable<InvestmentCategory> investmentCategories, IEnumerable<CommitmentType> commitmentTypes, PlanningCard updatedPlanningCard)
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
                                                    PlanningCardTitle = updatedPlanningCard.Name,
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
                                                    PositionGroupCode = placeholderAllocation?.PositionGroupCode,
                                                    IsPlanningCardShared = placeholderAllocation.IsPlanningCardShared,
                                                    IncludeInCapacityReporting = placeholderAllocation.IncludeInCapacityReporting
                                                }).ToList();

            return resourceAllocationsViewModel;
        }

        private List<EmployeeDetailsViewModel> ConvertToStaffingResponsibleMembersViewModel(List<Resource> resources)
        {
            if (resources == null || !resources.Any())
            {
                return new List<EmployeeDetailsViewModel>();
            }

            var staffingResponsibleGroupMembersViewModel = resources
                .Select(item => new EmployeeDetailsViewModel
                {
                    EmployeeCode = item.EmployeeCode,
                    FullName = item.FullName,
                }).ToList();

            return staffingResponsibleGroupMembersViewModel;
        }

        public async Task<List<SecurityUserDetailsViewModel>> GetAllSecurityUsersDetails()
        {
            var allSecurityUsersTask = _staffingApiClient.GetAllSecurityUsers();
            var allEmployeesTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            await Task.WhenAll(allSecurityUsersTask, allEmployeesTask);
            var allSecurityUsers = allSecurityUsersTask.Result;
            var allEmployees = allEmployeesTask.Result;
            var securityUsersDetails = new List<SecurityUserDetailsViewModel>();
            foreach (var securityUser in allSecurityUsers)
            {
                var employee = allEmployees.FirstOrDefault(emp => emp.EmployeeCode.Equals(securityUser.EmployeeCode, StringComparison.OrdinalIgnoreCase));
                var lastUpdatedBy = allEmployees.FirstOrDefault(emp => emp.EmployeeCode.Equals(securityUser.LastUpdatedBy, StringComparison.OrdinalIgnoreCase));
                securityUsersDetails.Add(new SecurityUserDetailsViewModel
                {
                    EmployeeCode = securityUser.EmployeeCode,
                    RoleCodes = securityUser.RoleCodes,
                    IsAdmin = securityUser.IsAdmin,
                    Override = securityUser.Override,
                    LastUpdated = securityUser.LastUpdated,
                    LastUpdatedBy = lastUpdatedBy?.FullName ?? securityUser.LastUpdatedBy,
                    FullName = employee?.FullName,
                    ManagingOfficeAbbreviation = employee?.SchedulingOffice?.OfficeAbbreviation,
                    ManagingOfficeName = employee?.SchedulingOffice?.OfficeName,
                    JobTitle = employee?.LevelName,
                    ServiceLine = employee?.ServiceLine?.ServiceLineName,
                    IsTerminated = string.Equals(employee?.ActiveStatus, "Terminated", StringComparison.OrdinalIgnoreCase) ? true : false,
                    Notes = securityUser.Notes,
                    EndDate = securityUser.EndDate,
                    UserTypeCode = securityUser.UserTypeCode,
                    GeoType = securityUser.GeoType,
                    OfficeCodes = securityUser.OfficeCodes,
                    ServiceLineCodes = securityUser.ServiceLineCodes,
                    PositionGroupCodes = securityUser.PositionGroupCodes,
                    LevelGrades = securityUser.LevelGrades,
                    PracticeAreaCodes = securityUser.PracticeAreaCodes,
                    RingfenceCodes = securityUser.RingfenceCodes,
                    HasAccessToAISearch = securityUser.HasAccessToAISearch,
                    HasAccessToStaffingInsightsTool = securityUser.HasAccessToStaffingInsightsTool,
                    HasAccessToRetiredStaffingTab = securityUser.hasAccessToRetiredStaffingTab
                });

            }
            return securityUsersDetails;
        }

        // Below Code is commented as currently we have removed sort by allocation % for the Sprint 41. We will work on it in future sprints
        //public async Task<IEnumerable<ResourceView>> GetFilteredResourcesWithAllocations(SupplyFilterCriteria supplyFilterCriteria, int? pageNumber, int? resourcesPerPage)
        //{
        //    var paginatedResourcesWithAllocations = Enumerable.Empty<AllocationSumViewModel>();

        //    var resourcesDataTask = _resourceApiClient.
        //        GetActiveEmployeesFilteredBySelectedValues(supplyFilterCriteria.OfficeCodes, supplyFilterCriteria.StartDate,
        //        supplyFilterCriteria.EndDate, supplyFilterCriteria.LevelGrades);
        //    var allocationsByOfficesTask = _staffingApiClient.
        //        GetResourceAllocationsByOfficeCodes(supplyFilterCriteria.OfficeCodes, supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate);
        //    var officesDataTask = _staffingApiClient.GetOfficeList();
        //    var investmentCategoriesDataTask = _staffingApiClient.GetInvestmentCategoryList();
        //    var caseRoleTypesDataTask = _staffingApiClient.GetCaseRoleTypeList();
        //    var commitmentsTask =
        //        _staffingApiClient.GetCommitmentsWithinDateRange(supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate);
        //    await Task.WhenAll(resourcesDataTask, allocationsByOfficesTask, officesDataTask, investmentCategoriesDataTask,
        //        caseRoleTypesDataTask, commitmentsTask);

        //    var resources = resourcesDataTask.Result;
        //    var allocationsByOffices = allocationsByOfficesTask.Result;
        //    var offices = officesDataTask.Result;
        //    var commitments = commitmentsTask.Result;

        //    var filteredResources = GetResourcesFilteredByStaffingTags(supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate,
        //        resources, commitments, supplyFilterCriteria.StaffingTags);

        //    var filteredAllocations = new List<AllocationSumViewModel>();

        //    foreach (var resource in filteredResources)
        //    {
        //        var filteredAllocation = new AllocationSumViewModel();
        //        var resourceAllocation = allocationsByOffices.Where(allocation => allocation.EmployeeCode == resource.EmployeeCode).ToList();

        //        filteredAllocation.Resource = resource;
        //        filteredAllocation.Allocations = resourceAllocation;

        //        filteredAllocation.AllocationSum = resourceAllocation.Where(x =>
        //        x.StartDate <= supplyFilterCriteria.StartDate && x.EndDate >= supplyFilterCriteria.StartDate).Select(allocation => allocation.Allocation).Sum();

        //        filteredAllocations.Add(filteredAllocation);
        //    }

        //    var sortedAllocation = SortResourcesAndAllocations(filteredAllocations, supplyFilterCriteria);

        //    paginatedResourcesWithAllocations = pageNumber == null && resourcesPerPage == null
        //        ? sortedAllocation
        //        : sortedAllocation
        //                    .Skip((pageNumber.Value - 1) * resourcesPerPage.Value)
        //                    .Take(resourcesPerPage.Value)
        //                    .ToList();

        //    var paginatedAllocations = sortedAllocation.SelectMany(i => i.Allocations).Distinct();

        //    string listPipelineId =
        //        string.Join(",", paginatedAllocations.Where(x =>
        //        x.PipelineId != null && x.PipelineId != Guid.Empty).Select(x => x.PipelineId.ToString()).Distinct());
        //    string listOldCaseCode =
        //        string.Join(",", paginatedAllocations.Where(x => x.OldCaseCode != null).Select(x => x.OldCaseCode).Distinct());

        //    var opportunitiesDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(listPipelineId);
        //    var casesDataTask = _ccmApiClient.GetCaseDataByCaseCodes(listOldCaseCode);
        //    var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineIds(listPipelineId.TrimEnd(','));

        //    await Task.WhenAll(opportunitiesDataTask, casesDataTask, pipelineChangesTask);

        //    var opportunities = opportunitiesDataTask.Result;
        //    var cases = casesDataTask.Result;
        //    var pipelineChanges = pipelineChangesTask.Result;

        //    pipelineChanges.Join(opportunities, (pipeline) => pipeline.PipelineId, (opportunity) => opportunity.PipelineId, (pipeline, opportunity) =>
        //    {
        //        if (pipeline.StartDate != null)
        //        {
        //            opportunity.StartDate = pipeline.StartDate;
        //        }
        //        if (pipeline.EndDate != null)
        //        {
        //            opportunity.EndDate = pipeline.EndDate;
        //        }
        //        return opportunity;
        //    }).ToList();

        //    var resourcesWithAllocations = new List<ResourceView>();

        //    foreach (var resourceWithAllocation in paginatedResourcesWithAllocations)
        //    {
        //        var resourceAllocation = new ResourceView
        //        {
        //            Resource = resourceWithAllocation.Resource,
        //            Allocations = ConvertToResourceAllocationModelByResource(resourceWithAllocation.Allocations, cases, opportunities,
        //            resourceWithAllocation.Resource, offices)
        //        };

        //        resourcesWithAllocations.Add(resourceAllocation);
        //    }

        //    return resourcesWithAllocations;

        //}

        public async Task<IEnumerable<ResourceView>> GetResourcesIncludingTerminatedWithAllocationsBySearchString(string searchString, string loggedInUser = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (string.IsNullOrEmpty(searchString) || searchString.Length <= 2)
                return Enumerable.Empty<ResourceView>();

            var searchedResources = await _resourceApiClient.GetEmployeesIncludingTerminatedBySearchString(searchString);

            var employeeCodes = string.Join(",", searchedResources.Select(r => r.EmployeeCode));

            var employeeWithPracticeAreaAffiliationsTask = await _basisApiClient.GetPracticeAreaAffiliationsByEmployeeCodes(employeeCodes, null, null);

            var resourcesStaffingAndCommitments = await GetFilteredResourcesWithAllocationsAndCommitments(
                searchedResources, null, employeeWithPracticeAreaAffiliationsTask, loggedInUser, startDate ?? DateTime.Now, endDate);

            return resourcesStaffingAndCommitments;

        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByCaseCode(string oldCaseCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            //If from date is null, then get all active (end date >= today) allocations
            if (!effectiveFromDate.HasValue)
                effectiveFromDate = DateTime.Now.Date;
            if (effectiveToDate.HasValue && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("effectiveFromDate cannot be greater than effectiveToDate");

            var caseDataTask = _ccmApiClient.GetCaseDataByCaseCodes(oldCaseCode);
            var resourceAllocationsDataTask = _staffingApiClient.GetResourceAllocationsByCaseCode(oldCaseCode);
            var placeholderAllocationsDataTask = _staffingApiClient.GetPlaceholderAllocationsByCaseCodes(oldCaseCode);
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var serviceLinesDataTask = _resourceApiClient.GetServiceLines();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            await Task.WhenAll(caseDataTask, resourceAllocationsDataTask, placeholderAllocationsDataTask,
                resourcesDataTask, officeListDataTask, serviceLinesDataTask, commitmentTypeListTask);

            var caseData = caseDataTask.Result.FirstOrDefault();
            var resourceAllocations = resourceAllocationsDataTask.Result;
            var placeholderAllocations = placeholderAllocationsDataTask.Result;
            var resources = resourcesDataTask.Result;
            var offices = officeListDataTask.Result;
            var serviceLines = serviceLinesDataTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            var placeholderAndResourceAllocations = new List<ResourceAssignmentViewModel>();

            var resourceAllocationsWithinDateRange = resourceAllocations.Where(allocation =>
            {
                if (effectiveToDate == null)
                    return allocation.EndDate >= effectiveFromDate;
                else
                    return allocation.StartDate <= effectiveToDate && allocation.EndDate >= effectiveFromDate;
            });

            var placeholderAllocationsWithinDateRange = placeholderAllocations?.Where(allocation =>
            {
                if (effectiveToDate == null)
                    return allocation.EndDate >= effectiveFromDate;
                else
                    return allocation.StartDate <= effectiveToDate && allocation.EndDate >= effectiveFromDate;
            });

            placeholderAndResourceAllocations.AddRange(resourceAllocationsWithinDateRange);
            placeholderAndResourceAllocations.AddRange(placeholderAllocationsWithinDateRange);

            return ConvertToResourceAllocationModelByProject(placeholderAndResourceAllocations, caseData, null, resources, offices, serviceLines, caseRoleTypeList, commitmentTypeList);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByPipelineId(string pipelineId, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (!effectiveFromDate.HasValue)
                effectiveFromDate = DateTime.Now.Date;
            if (effectiveToDate.HasValue && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("effectiveFromDate cannot be greater than effectiveToDate");

            var opportunityDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineId);
            var resourceAllocationsDataTask = _staffingApiClient.GetResourceAllocationsByPipelineId(pipelineId);
            var placeholderAllocationsDataTask = _staffingApiClient.GetPlaceholderAllocationsByPipelineIds(pipelineId);
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var investmentCategoriesDataTask = _staffingApiClient.GetInvestmentCategoryList();
            var caseRoleTypeListTask = _staffingApiClient.GetCaseRoleTypeList();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var serviceLinesDataTask = _resourceApiClient.GetServiceLines();
            var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineId(new Guid(pipelineId));
            var commitmentTypeListTask = _staffingApiClient.GetCommitmentTypeList();

            await Task.WhenAll(opportunityDataTask, resourceAllocationsDataTask, placeholderAllocationsDataTask,
                resourcesDataTask, investmentCategoriesDataTask, caseRoleTypeListTask,
                officeListDataTask, serviceLinesDataTask, pipelineChangesTask, commitmentTypeListTask);

            var opportunityData = opportunityDataTask.Result.FirstOrDefault();
            var pipelineChangesData = pipelineChangesTask.Result;
            var resourceAllocations = resourceAllocationsDataTask.Result;
            var placeholderAllocations = placeholderAllocationsDataTask.Result;
            var resources = resourcesDataTask.Result;
            var offices = officeListDataTask.Result;
            var serviceLines = serviceLinesDataTask.Result;
            var caseRoleTypeList = caseRoleTypeListTask.Result;
            var commitmentTypeList = commitmentTypeListTask.Result;

            if (pipelineChangesData != null && pipelineChangesData.PipelineId.Equals(new Guid(pipelineId)))
            {
                opportunityData.StartDate = pipelineChangesData.StartDate ?? opportunityData.StartDate;
                opportunityData.EndDate = pipelineChangesData.EndDate ?? opportunityData.EndDate;
                opportunityData.ProbabilityPercent = pipelineChangesData.ProbabilityPercent ?? opportunityData.ProbabilityPercent;
            }

            var placeholderAndResourceAllocations = new List<ResourceAssignmentViewModel>();

            var resourceAllocationsWithinDateRange = resourceAllocations.Where(allocation =>
            {
                if (effectiveToDate == null)
                    return allocation.EndDate >= effectiveFromDate;
                else
                    return allocation.StartDate <= effectiveToDate && allocation.EndDate >= effectiveFromDate;
            });

            var placeholderAllocationsWithinDateRange = placeholderAllocations?.Where(allocation =>
            {
                if (effectiveToDate == null)
                    return allocation.EndDate >= effectiveFromDate;
                else
                    return allocation.StartDate <= effectiveToDate && allocation.EndDate >= effectiveFromDate;
            });

            placeholderAndResourceAllocations.AddRange(resourceAllocationsWithinDateRange);
            placeholderAndResourceAllocations.AddRange(placeholderAllocationsWithinDateRange);

            return ConvertToResourceAllocationModelByProject(placeholderAndResourceAllocations, null, opportunityData, resources, offices, serviceLines, caseRoleTypeList, commitmentTypeList);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> GetHistoricalStaffingAllocationsForProject(string oldCaseCode, string pipelineId)
        {
            if (string.IsNullOrEmpty(oldCaseCode) && string.IsNullOrEmpty(pipelineId))
                return Enumerable.Empty<ResourceAssignmentViewModel>();
            var resourcesDataTask = _resourceApiClient.GetEmployeesIncludingTerminated();
            var officeListDataTask = _ccmApiClient.GetOfficeList();
            var serviceLinesDataTask = _resourceApiClient.GetServiceLines();

            if (!string.IsNullOrEmpty(pipelineId))
            {
                var opportunityDataTask = _pipelineApiClient.GetOpportunitiesWithTaxonomiesByPipelineIds(pipelineId);
                var resourceAllocationsDataTask = _staffingApiClient.GetResourceAllocationsByPipelineId(pipelineId);
                var pipelineChangesTask = _staffingApiClient.GetPipelineChangesByPipelineId(new Guid(pipelineId));

                await Task.WhenAll(opportunityDataTask, resourceAllocationsDataTask, resourcesDataTask, officeListDataTask, serviceLinesDataTask, pipelineChangesTask);

                var opportunityData = opportunityDataTask.Result.FirstOrDefault();
                var resourceAllocations = resourceAllocationsDataTask.Result;
                var resources = resourcesDataTask.Result;
                var offices = officeListDataTask.Result;
                var serviceLines = serviceLinesDataTask.Result;
                var pipelineChangesData = pipelineChangesTask.Result;

                if (pipelineChangesData != null)
                {
                    opportunityData.StartDate = pipelineChangesData.StartDate ?? opportunityData.StartDate;
                    opportunityData.EndDate = pipelineChangesData.EndDate ?? opportunityData.EndDate;
                    opportunityData.ProbabilityPercent = pipelineChangesData.ProbabilityPercent ?? opportunityData.ProbabilityPercent;
                }

                return ConvertToResourceAllocationModelByProject(resourceAllocations, null, opportunityData, resources, offices, serviceLines);
            }
            else
            {
                var caseDataTask = _ccmApiClient.GetCaseDataByCaseCodes(oldCaseCode);
                var resourceAllocationsDataTask = _staffingApiClient.GetResourceAllocationsByCaseCode(oldCaseCode);

                await Task.WhenAll(caseDataTask, resourceAllocationsDataTask, resourcesDataTask, officeListDataTask, serviceLinesDataTask);

                var caseData = caseDataTask.Result.FirstOrDefault();
                var resourceAllocations = resourceAllocationsDataTask.Result;
                var resources = resourcesDataTask.Result;
                var offices = officeListDataTask.Result;
                var serviceLines = serviceLinesDataTask.Result;

                return ConvertToResourceAllocationModelByProject(resourceAllocations, caseData, null, resources, offices, serviceLines);
            }
        }

        private IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAllocationModelByProject(IEnumerable<ResourceAssignmentViewModel> placeholderAndResourceallocations,
            CaseData caseItem, OpportunityData opportunityItem, IEnumerable<Resource> resources, IEnumerable<Office> offices, IEnumerable<ServiceLine> serviceLines,
            IEnumerable<CaseRoleType> caseRoleTypeList = null, IEnumerable<CommitmentType> commitmentTypeList = null)
        {
            var allocations = (from allocation in placeholderAndResourceallocations
                               let resource = resources.FirstOrDefault(r => r.EmployeeCode == allocation.EmployeeCode)
                               let office = offices.FirstOrDefault(o => o.OfficeCode == allocation.OperatingOfficeCode)
                               let serviceLine = serviceLines.FirstOrDefault(sl => sl.ServiceLineCode == allocation.ServiceLineCode)
                               let caseRoleType = caseRoleTypeList?.FirstOrDefault(crt => crt.CaseRoleCode == allocation.CaseRoleCode)
                               let commitmentType = commitmentTypeList?.FirstOrDefault(ct => ct.CommitmentTypeCode == allocation.CommitmentTypeCode)

                               select new ResourceAssignmentViewModel()
                               {
                                   Id = allocation.Id,
                                   OldCaseCode = allocation.OldCaseCode,
                                   CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                   CaseName = caseItem?.CaseName,
                                   ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                   PipelineId = allocation.PipelineId,
                                   OpportunityName = opportunityItem?.OpportunityName,
                                   EmployeeCode = allocation.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   InternetAddress = resource?.InternetAddress,
                                   CurrentLevelGrade = allocation.CurrentLevelGrade,
                                   OperatingOfficeCode = allocation.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   ServiceLineCode = allocation.ServiceLineCode,
                                   ServiceLineName = serviceLine?.ServiceLineName,
                                   Allocation = allocation.Allocation,
                                   StartDate = allocation.StartDate,
                                   EndDate = allocation.EndDate,
                                   InvestmentCode = allocation.InvestmentCode,
                                   CaseRoleCode = allocation.CaseRoleCode,
                                   LastUpdatedBy = allocation.LastUpdatedBy,
                                   CaseStartDate = caseItem?.StartDate,
                                   CaseEndDate = caseItem?.EndDate,
                                   OpportunityStartDate = opportunityItem?.StartDate,
                                   OpportunityEndDate = opportunityItem?.EndDate,
                                   Notes = allocation.Notes ?? "",
                                   CaseRoleName = caseRoleType?.CaseRoleName,
                                   TerminationDate = resource?.TerminationDate,
                                   JoiningDate = resource?.StartDate,
                                   CommitmentTypeCode = allocation.CommitmentTypeCode,
                                   CommitmentTypeName = commitmentType?.CommitmentTypeName,
                                   IsPlaceholderAllocation = allocation.IsPlaceholderAllocation,
                                   PositionGroupCode = allocation?.PositionGroupCode
                               }).ToList();

            return allocations;
        }

        private List<ResourceAssignmentViewModel> ConvertToResourceAllocationModel(IEnumerable<ResourceAssignmentViewModel> resourceAllocations,
            IEnumerable<CaseData> casesData, IEnumerable<OpportunityData> opportunitiesData, IEnumerable<Office> officesData,
            IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<Resource> resources = null)
        {
            var allocations = (from resourceAllocation in resourceAllocations
                               join resourceData in resources on resourceAllocation.EmployeeCode equals resourceData.EmployeeCode into resourcesData
                               from resource in resourcesData.DefaultIfEmpty()
                               join officeData in officesData on resourceAllocation.OperatingOfficeCode equals officeData.OfficeCode into offices
                               from office in offices.DefaultIfEmpty()
                               join caseData in casesData on resourceAllocation.OldCaseCode equals caseData.OldCaseCode into resAllocGroups
                               from caseItem in resAllocGroups.DefaultIfEmpty()
                               join opp in opportunitiesData on resourceAllocation.PipelineId equals opp?.PipelineId into resOppsGroups
                               from opportunityItem in resOppsGroups.DefaultIfEmpty()
                               join crt in caseRoleTypeList on resourceAllocation.CaseRoleCode equals crt.CaseRoleCode into resAllocCaseRoleType
                               from caseRoleTypeName in resAllocCaseRoleType.DefaultIfEmpty()
                                   //join res in resources on caseItem?.CaseManagerCode equals res.EmployeeCode into resourcesList
                                   //from caseManager in resourcesList.DefaultIfEmpty()
                               select new ResourceAssignmentViewModel()
                               {
                                   Id = resourceAllocation.Id,
                                   OldCaseCode = resourceAllocation.OldCaseCode,
                                   CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                   CaseName = caseItem?.CaseName,
                                   ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                   PipelineId = resourceAllocation.PipelineId,
                                   OpportunityName = opportunityItem?.OpportunityName,
                                   EmployeeCode = resourceAllocation.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   InternetAddress = resource?.InternetAddress,
                                   CurrentLevelGrade = resourceAllocation.CurrentLevelGrade,
                                   OperatingOfficeCode = resourceAllocation.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   Allocation = resourceAllocation.Allocation,
                                   StartDate = resourceAllocation.StartDate,
                                   EndDate = resourceAllocation.EndDate,
                                   InvestmentCode = resourceAllocation.InvestmentCode,
                                   CaseRoleCode = resourceAllocation.CaseRoleCode,
                                   PrimaryIndustry = caseItem?.PrimaryIndustry ?? opportunityItem?.PrimaryIndustry,
                                   PrimaryCapability = caseItem?.PrimaryCapability ?? opportunityItem?.PrimaryCapability,
                                   LastUpdatedBy = resourceAllocation.LastUpdatedBy,
                                   CaseStartDate = caseItem?.StartDate,
                                   CaseEndDate = caseItem?.EndDate,
                                   OpportunityStartDate = opportunityItem?.StartDate,
                                   OpportunityEndDate = opportunityItem?.EndDate,
                                   ProbabilityPercent = opportunityItem?.ProbabilityPercent,
                                   Notes = resourceAllocation.Notes ?? "",
                                   CaseRoleName = caseRoleTypeName?.CaseRoleName,
                                   CaseManagerCode = caseItem?.CaseManagerCode//,
                                   // CaseManagerName = caseManager?.FullName
                               }).ToList();

            return allocations;
        }

        private List<ResourceAssignmentViewModel> ConvertToResourceAllocationModelByResource(IEnumerable<ResourceAssignmentViewModel> resourceAllocations,
            IEnumerable<CaseData> casesData, IEnumerable<OpportunityData> opportunitiesData, Resource resource, IEnumerable<Office> officesData,
            IEnumerable<CaseRoleType> caseRoleTypeList)
        {
            var allocations = (from resourceAllocation in resourceAllocations
                               join officeData in officesData on resourceAllocation.OperatingOfficeCode equals officeData.OfficeCode into offices
                               from office in offices.DefaultIfEmpty()
                               join caseData in casesData on resourceAllocation.OldCaseCode equals caseData.OldCaseCode into resAllocGroups
                               from caseItem in resAllocGroups.DefaultIfEmpty()
                               join opp in opportunitiesData on resourceAllocation.PipelineId equals opp?.PipelineId into resOppsGroups
                               from opportunityItem in resOppsGroups.DefaultIfEmpty()
                               join crt in caseRoleTypeList on resourceAllocation.CaseRoleCode equals crt.CaseRoleCode into resAllocCaseRoleType
                               from caseRoleTypeName in resAllocCaseRoleType.DefaultIfEmpty()
                               select new ResourceAssignmentViewModel()
                               {
                                   Id = resourceAllocation.Id,
                                   OldCaseCode = resourceAllocation.OldCaseCode,
                                   CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                   CaseName = caseItem?.CaseName,
                                   ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                   PipelineId = resourceAllocation.PipelineId,
                                   OpportunityName = opportunityItem?.OpportunityName,
                                   EmployeeCode = resourceAllocation.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   InternetAddress = resource?.InternetAddress,
                                   CurrentLevelGrade = resourceAllocation.CurrentLevelGrade,
                                   OperatingOfficeCode = resourceAllocation.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   Allocation = resourceAllocation.Allocation,
                                   StartDate = resourceAllocation.StartDate,
                                   EndDate = resourceAllocation.EndDate,
                                   InvestmentCode = resourceAllocation.InvestmentCode,
                                   CaseRoleCode = resourceAllocation.CaseRoleCode,
                                   PrimaryIndustry = caseItem?.PrimaryIndustry ?? opportunityItem?.PrimaryIndustry,
                                   PrimaryCapability = caseItem?.PrimaryCapability ?? opportunityItem?.PrimaryCapability,
                                   LastUpdatedBy = resourceAllocation.LastUpdatedBy,
                                   CaseStartDate = caseItem?.StartDate,
                                   CaseEndDate = caseItem?.EndDate,
                                   OpportunityStartDate = opportunityItem?.StartDate,
                                   OpportunityEndDate = opportunityItem?.EndDate,
                                   ProbabilityPercent = opportunityItem?.ProbabilityPercent,
                                   Notes = resourceAllocation.Notes ?? "",
                                   CaseRoleName = caseRoleTypeName?.CaseRoleName,
                                   CaseManagerCode = caseItem?.CaseManagerCode,
                                   IsPlaceholderAllocation = resourceAllocation.IsPlaceholderAllocation,
                                   PlanningCardId = resourceAllocation.PlanningCardId,
                                   PlanningCardTitle = resourceAllocation.PlanningCardTitle,
                                   IsPlanningCardShared = resourceAllocation.IsPlanningCardShared,
                                   IncludeInCapacityReporting = resourceAllocation?.IncludeInCapacityReporting
                                   //CaseManagerName = caseManager?.FullName
                               }).ToList();

            return allocations;
        }

        private List<ResourceAssignmentViewModel> ConvertToResourceAllocationModelByResourceWithCaseManagerDetails(IEnumerable<ResourceAssignmentViewModel> resourceAllocations,
            IEnumerable<CaseData> casesData, IEnumerable<OpportunityData> opportunitiesData, Resource resource, IEnumerable<Office> officesData,
            IEnumerable<CaseRoleType> caseRoleTypeList, IEnumerable<Resource> resources = null)
        {
            var allocations = (from resourceAllocation in resourceAllocations
                               join officeData in officesData on resourceAllocation.OperatingOfficeCode equals officeData.OfficeCode into offices
                               from office in offices.DefaultIfEmpty()
                               join caseData in casesData on resourceAllocation.OldCaseCode equals caseData.OldCaseCode into resAllocGroups
                               from caseItem in resAllocGroups.DefaultIfEmpty()
                               join opp in opportunitiesData on resourceAllocation.PipelineId equals opp?.PipelineId into resOppsGroups
                               from opportunityItem in resOppsGroups.DefaultIfEmpty()
                               join crt in caseRoleTypeList on resourceAllocation.CaseRoleCode equals crt.CaseRoleCode into resAllocCaseRoleType
                               from caseRoleTypeName in resAllocCaseRoleType.DefaultIfEmpty()
                               join res in resources on caseItem?.CaseManagerCode equals res.EmployeeCode into resourcesList
                               from caseManager in resourcesList.DefaultIfEmpty()
                               select new ResourceAssignmentViewModel()
                               {
                                   Id = resourceAllocation.Id,
                                   OldCaseCode = resourceAllocation.OldCaseCode,
                                   CaseTypeCode = (Constants.CaseType?)caseItem?.CaseTypeCode,
                                   CaseName = caseItem?.CaseName,
                                   ClientName = caseItem?.ClientName ?? opportunityItem?.ClientName,
                                   PipelineId = resourceAllocation.PipelineId,
                                   OpportunityName = opportunityItem?.OpportunityName,
                                   EmployeeCode = resourceAllocation.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   InternetAddress = resource?.InternetAddress,
                                   CurrentLevelGrade = resourceAllocation.CurrentLevelGrade,
                                   OperatingOfficeCode = resourceAllocation.OperatingOfficeCode,
                                   OperatingOfficeAbbreviation = office?.OfficeAbbreviation,
                                   Allocation = resourceAllocation.Allocation,
                                   StartDate = resourceAllocation.StartDate,
                                   EndDate = resourceAllocation.EndDate,
                                   InvestmentCode = resourceAllocation.InvestmentCode,
                                   CaseRoleCode = resourceAllocation.CaseRoleCode,
                                   PrimaryIndustry = caseItem?.PrimaryIndustry ?? opportunityItem?.PrimaryIndustry,
                                   PrimaryCapability = caseItem?.PrimaryCapability ?? opportunityItem?.PrimaryCapability,
                                   LastUpdatedBy = resourceAllocation.LastUpdatedBy,
                                   CaseStartDate = caseItem?.StartDate,
                                   CaseEndDate = caseItem?.EndDate,
                                   OpportunityStartDate = opportunityItem?.StartDate,
                                   OpportunityEndDate = opportunityItem?.EndDate,
                                   ProbabilityPercent = opportunityItem?.ProbabilityPercent,
                                   Notes = resourceAllocation.Notes ?? "",
                                   CaseRoleName = caseRoleTypeName?.CaseRoleName,
                                   CaseManagerCode = caseItem?.CaseManagerCode,
                                   CaseManagerName = caseManager?.FullName,
                                   PegIndustryTerm = caseItem?.PegIndustryTerm ?? ""
                               }).ToList();

            return allocations;
        }
        private List<NotesAlertViewModel> ConvertToNotesAlertViewModel(
            IEnumerable<NoteAlert> notesAlertData,
            IEnumerable<CaseData> casesData,
            IEnumerable<OpportunityData> opportunitiesData,
            IEnumerable<Resource> resources = null)
        {
            var notesAlertList = (from notesAlert in notesAlertData
                               join caseData in casesData on notesAlert.OldCaseCode equals caseData.OldCaseCode into resAllocGroups
                               from caseItem in resAllocGroups.DefaultIfEmpty()
                               join opp in opportunitiesData on notesAlert.PipelineId equals opp?.PipelineId into resOppsGroups
                               from opportunityItem in resOppsGroups.DefaultIfEmpty()
                               join resourceData in resources on notesAlert.EmployeeCode equals resourceData.EmployeeCode into resourcesData
                               from resource in resourcesData.DefaultIfEmpty()
                               join createdByResource in resources on notesAlert.CreatedBy equals createdByResource.EmployeeCode into createdByResources
                               from createdBy in createdByResources.DefaultIfEmpty()
                               join noteForEmployeeResource in resources on notesAlert.NoteForEmployeeCode equals noteForEmployeeResource.EmployeeCode into noteForEmployeeResources
                               from noteForEmployee in noteForEmployeeResources.DefaultIfEmpty()
                               select new NotesAlertViewModel()
                               {
                                   Id = notesAlert.Id,
                                   NoteID = notesAlert.NoteID,
                                   OldCaseCode = notesAlert.OldCaseCode,
                                   PipelineId = notesAlert.PipelineId,
                                   PlanningCardId = notesAlert.PlanningCardId,
                                   PlanningCardName = notesAlert.PlanningCardName,
                                   CaseName = caseItem?.CaseName,
                                   OppName = opportunityItem?.OpportunityName,
                                   EmployeeCode = notesAlert.EmployeeCode,
                                   EmployeeName = resource?.FullName,
                                   NoteForEmployeeCode = notesAlert.NoteForEmployeeCode,
                                   NoteForEmployeeName = noteForEmployee?.FullName,
                                   AlertStatus = notesAlert.AlertStatus,
                                   LastUpdated = notesAlert.LastUpdated,
                                   LastUpdatedBy = notesAlert.LastUpdatedBy,
                                   CreatedBy = notesAlert.CreatedBy,
                                   CreatedByEmployeeName = createdBy?.FullName,
                                   Note = notesAlert.Note,
                                   NoteTypeCode = notesAlert.NoteTypeCode
                               }).ToList();

            return notesAlertList;
        }


    }
}
