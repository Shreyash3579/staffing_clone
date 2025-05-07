using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class ResourceService : IResourceService
    {
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IHcpdApiClient _hcpdApiClient;
        private readonly ICommonResourcesService _commonResourcesService;


        public ResourceService(IResourceApiClient resourceApiClient,
            IStaffingApiClient staffingApiClient,
            IHcpdApiClient hcpdApiClient,
            ICommonResourcesService commonResourcesService)
        {
            _resourceApiClient = resourceApiClient;
            _staffingApiClient = staffingApiClient;
            _hcpdApiClient = hcpdApiClient;
            _commonResourcesService = commonResourcesService;
        }

        public async Task<ResourceCommitment> GetResourcesFilteredBySelectedValues(SupplyFilterCriteria supplyFilterCriteria, string loggedInUser)
        {
            var officeCodes = supplyFilterCriteria.OfficeCodes;
            var startDate = supplyFilterCriteria.StartDate;
            var endDate = supplyFilterCriteria.EndDate;
            var levelGrades = supplyFilterCriteria.LevelGrades;
            var staffingTags = supplyFilterCriteria.StaffingTags;
            var positionCodes = supplyFilterCriteria.PositionCodes;
            var staffableAsTypeCodes = supplyFilterCriteria.StaffableAsTypeCodes;

            if (string.IsNullOrEmpty(officeCodes) || string.IsNullOrEmpty(staffingTags))
                return new ResourceCommitment();
            if (startDate == DateTime.MinValue)
                throw new ArgumentException("Start Date can not be null");
            if (endDate == DateTime.MinValue)
                throw new ArgumentException("End Date can not be null");
            if (endDate < startDate)
                throw new ArgumentException("End date should be greater than start date");

            var resourcesDataTask =
                _resourceApiClient.GetActiveEmployeesFilteredBySelectedValues(officeCodes, startDate, endDate, null, null);

            var commitmentsTask =
                _staffingApiClient.GetCommitmentsWithinDateRange(startDate, endDate);

            await Task.WhenAll(resourcesDataTask, commitmentsTask);

            var resourcesData = resourcesDataTask.Result;
            var commitments = commitmentsTask.Result;

            var distinctEmployeeCodes = string.Join(',', resourcesData.Select(r => r.EmployeeCode).Distinct());

            var employeesWithStaffableAsRoles = !string.IsNullOrEmpty(staffableAsTypeCodes) ? await _staffingApiClient.GetResourceActiveStaffableAsByEmployeeCodes(distinctEmployeeCodes) : new List<StaffableAs>();

            var noteTypeCode = "RA";

            var resourceViewNotesTask =
                _staffingApiClient.GetResourceViewNotes(distinctEmployeeCodes, loggedInUser, noteTypeCode);
            var resourcesTask = _resourceApiClient.GetEmployees();

            await Task.WhenAll(resourceViewNotesTask, resourcesTask);

            var resourceViewNotes = resourceViewNotesTask.Result;
            var resources = resourcesTask.Result;

            // filtering based on LevelGrade, PositionCode, and StaffableAsTypeCodes
            var filteredResources = _commonResourcesService.FilterResourcesByLevelGradePositionAndStaffableAs(resourcesData, employeesWithStaffableAsRoles, supplyFilterCriteria);

            filteredResources = GetEmployeesFilteredByFilterValues(filteredResources, supplyFilterCriteria, commitments);

            foreach (var resource in filteredResources)
            {
                resource.ResourceViewNotes = ConvertToResourceViewNotesViewModel(resourceViewNotes.Where(x => x.EmployeeCode == resource.EmployeeCode), resources);
            }

            var resourcesAllocationsAndCommitments = await GetResourcesAllocationsAndCommitments(filteredResources, startDate, endDate, null);

            resourcesAllocationsAndCommitments.PlaceholderAllocations = GetPlaceholdersExcludingUnsharedPlanningCards(
                resourcesAllocationsAndCommitments.PlaceholderAllocations);

            return resourcesAllocationsAndCommitments;
        }

        public IEnumerable<ResourceViewNoteViewModel> ConvertToResourceViewNotesViewModel(IEnumerable<ResourceViewNote> resourceViewNotes, List<Resource> resources)
        {
            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            IEnumerable<ResourceViewNoteViewModel> resourceViewNotesModel = resourceViewNotes.Select(note => new ResourceViewNoteViewModel
            {
                Id = note.Id,
                EmployeeCode = note.EmployeeCode,
                Note = note.Note ?? "",
                IsPrivate = note.IsPrivate,
                SharedWith = note.SharedWith,
                SharedWithDetails = !string.IsNullOrEmpty(note.SharedWith) ? resources?.Where(x => note.SharedWith.Split(',').Contains(x.EmployeeCode)).ToList() : null,
                CreatedBy = note.CreatedBy,
                CreatedByName = resources.FirstOrDefault(x => x.EmployeeCode == note.CreatedBy).FullName,
                NoteTypeCode = note.NoteTypeCode,
                LastUpdatedBy = note.LastUpdatedBy,
                LastUpdated = TimeZoneInfo.ConvertTimeToUtc(note.LastUpdated, est)
            });

            return resourceViewNotesModel;
        }

        public async Task<ResourceCommitment> GetResourcesFilteredBySelectedGroupValues(SupplyGroupFilterCriteria supplyGroupFilterCriteria, string loggedInUser)
        {
            var employeeCodes = supplyGroupFilterCriteria.EmployeeCodes;
            var startDate = supplyGroupFilterCriteria.StartDate;
            var endDate = supplyGroupFilterCriteria.EndDate;

            if (string.IsNullOrEmpty(employeeCodes))
                throw new ArgumentException("Employee Codes can not be null");
            if (startDate == DateTime.MinValue)
                throw new ArgumentException("Start Date can not be null");
            if (endDate == DateTime.MinValue)
                throw new ArgumentException("End Date can not be null");
            if (endDate < startDate)
                throw new ArgumentException("End date should be greater than start date");

            var resourcesDataTask =
                _resourceApiClient.GetActiveEmployeesFilteredBySelectedGroupValues(employeeCodes, startDate, endDate);

            await Task.WhenAll(resourcesDataTask);

            var resourcesData = resourcesDataTask.Result;

            var distinctEmployeeCodes = string.Join(',', resourcesData.Select(r => r.EmployeeCode).Distinct());
            var noteTypeCode = "RA";

            var resourceViewNotesTask =
                _staffingApiClient.GetResourceViewNotes(distinctEmployeeCodes, loggedInUser, noteTypeCode);
            var resourcesTask = _resourceApiClient.GetEmployees();

            await Task.WhenAll(resourceViewNotesTask, resourcesTask);

            var resourceViewNotes = resourceViewNotesTask.Result;
            var resources = resourcesTask.Result;

            foreach (var resource in resourcesData)
            {
                resource.ResourceViewNotes = ConvertToResourceViewNotesViewModel(resourceViewNotes.Where(x => x.EmployeeCode == resource.EmployeeCode), resources);
            }

            var resourcesAllocationsAndCommitments = await GetResourcesAllocationsAndCommitments(resourcesData,
                startDate, endDate, null);

            return resourcesAllocationsAndCommitments;
        }

        public async Task<ResourceCommitment> GetResourcesAllocationsAndCommitmentsBySearchString(string searchString, bool? addTransfers = false)
        {
            var resources = await _resourceApiClient.GetEmployeesBySearchString(searchString, (bool)addTransfers);
            var resourcesAllocationsAndCommtments = await GetResourcesAllocationsAndCommitments(resources, null, null, null);
            return resourcesAllocationsAndCommtments;
        }

        public async Task<ResourceCommitment> GetResourcesIncludingTerminatedAllocationsAndCommitmentsBySearchString(string searchString, bool? addTransfers = false)
        {
            var resources = await _resourceApiClient.GetEmployeesIncludingTerminatedBySearchString(searchString, (bool)addTransfers);
            var resourcesAllocationsAndCommtments = await GetResourcesAllocationsAndCommitments(resources, DateTime.Now.Date, null, null);
            return resourcesAllocationsAndCommtments;
        }

        public async Task<ResourceCommitment> GetResourcesAllocationsAndCommitmentsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate,
            string commitmentTypes)
        {
            var resourcesAllocationsAndCommtments = await _commonResourcesService.GetResourcesCommitmentsForStaffingTab(employeeCodes, startDate, endDate, commitmentTypes);
            return resourcesAllocationsAndCommtments;
        }

        public async Task<AdvisorViewModel> GetAdvisorNameByEmployeeCode(string employeeCode)
        {
            var advisor = new AdvisorViewModel();
            var employeeWithAdvisor = await _hcpdApiClient.GetAdvisorByEmployeeCode(employeeCode);

            if (!string.IsNullOrEmpty(employeeWithAdvisor?.AdvisorEmployeeCode))
            {
                var employees = await _resourceApiClient.GetEmployeesIncludingTerminated();

                var advisorInfo = employees.FirstOrDefault(emp => emp.EmployeeCode == employeeWithAdvisor.AdvisorEmployeeCode);
                advisor.FullName = advisorInfo?.FirstName + " " + advisorInfo?.LastName;
            }

            return advisor;
        }

        public async Task<IEnumerable<MenteeViewModel>> GetMenteeNamesByEmployeeCode(string employeeCode)
        {
            var mentees = Enumerable.Empty<MenteeViewModel>();

            var employeeWithMentees = await _hcpdApiClient.GetMenteesByEmployeeCode(employeeCode);
            if (employeeWithMentees.Any())
            {
                var employees = await _resourceApiClient.GetEmployeesIncludingTerminated();

                mentees = employeeWithMentees.Select(mentee =>
                {
                    var menteeInfo = employees.FirstOrDefault(emp => emp.EmployeeCode == mentee.MenteeEmployeeCode);
                    return new MenteeViewModel
                    {
                        FullName = menteeInfo?.FirstName + " " + menteeInfo?.LastName
                    };
                });
            }

            return mentees;
        }

        private async Task<ResourceCommitment> GetResourcesAllocationsAndCommitments(IEnumerable<Resource> resources,
            DateTime? startDate, DateTime? endDate, string commitmentTypeCode)
        {
            var distinctEmployeeCodes = string.Join(",", resources.Select(x => x.EmployeeCode).Distinct());
            var resourceAllocationsAndCommitments = await _commonResourcesService.GetResourcesCommitmentsForStaffingTab(distinctEmployeeCodes, startDate, endDate, null);
            resourceAllocationsAndCommitments.Resources = resources;
            return resourceAllocationsAndCommitments;

        }

        private IEnumerable<Resource> GetEmployeesFilteredByFilterValues(IEnumerable<Resource> resourcesData, SupplyFilterCriteria supplyFilterCriteria, IEnumerable<CommitmentViewModel> commitments)
        {
            var unfilteredEmployeeCodes = string.Join(',', resourcesData.Select(r => r.EmployeeCode).Distinct());

            var employeeWithPracticeAreaAffiliationsTask = _commonResourcesService.GetEmployeesWithPracticeAreaAffiliationsDataTask(unfilteredEmployeeCodes, supplyFilterCriteria.PracticeAreaCodes, supplyFilterCriteria.AffiliationRoleCodes);

            var employeesWithPracticeAreaAffiliations = employeeWithPracticeAreaAffiliationsTask.Result;

            var filteredResources = _commonResourcesService.FilterResourcesBySelectedPracticeAreaAffiliations(supplyFilterCriteria.PracticeAreaCodes, supplyFilterCriteria.AffiliationRoleCodes,
            employeesWithPracticeAreaAffiliations, resourcesData);

            filteredResources = _commonResourcesService.GetResourcesFilteredByStaffingTags(supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate, filteredResources, commitments,
                supplyFilterCriteria.StaffingTags);

            return filteredResources;
        }

        private IEnumerable<ScheduleMasterPlaceholder> GetPlaceholdersExcludingUnsharedPlanningCards(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations)
        {
            return placeholderAllocations.Where(x => !x.PlanningCardId.HasValue || (x.PlanningCardId.HasValue && x.IsPlanningCardShared.GetValueOrDefault()));
        }
    }
}