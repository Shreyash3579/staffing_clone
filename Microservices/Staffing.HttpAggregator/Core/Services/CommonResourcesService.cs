using Newtonsoft.Json;
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
    public class CommonResourcesService : ICommonResourcesService
    {
        private readonly IBasisApiClient _basisApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourceAllocationService _resourceAllocationService;
        private readonly IBvuCDApiClient _bvuCDApiClient;
        private readonly IVacationApiClient _vacationApiClient;

        public CommonResourcesService(IBasisApiClient basisApiClient, IResourceApiClient resourceApiClient, IStaffingApiClient staffingApiClient,
            IResourceAllocationService resourceAllocationService, IBvuCDApiClient bvuCDApiClient, IVacationApiClient vacationApiClient)
        {
            _basisApiClient = basisApiClient;
            _resourceApiClient = resourceApiClient;
            _staffingApiClient = staffingApiClient;
            _resourceAllocationService = resourceAllocationService;
            _bvuCDApiClient = bvuCDApiClient;
            _vacationApiClient = vacationApiClient;
        }

        public async Task<ResourceCommitment> GetResourcesCommitmentsForStaffingTab(string distinctEmployeeCodes, DateTime? startDate, DateTime? endDate, string commitmentTypes)
        {
            var getAllCommitments = string.IsNullOrEmpty(commitmentTypes);

            // Get Commitments saved in Staffing
            var resourcesCommitmentsTask = _staffingApiClient.GetResourceCommitmentsWithinDateRangeByEmployees(
                distinctEmployeeCodes, startDate, endDate, null);

            var allocations = GetAllocationsDetailedInfo(distinctEmployeeCodes, startDate, endDate, commitmentTypes);

            var externalCommitmentsTasks = GetCommitmentsFromExternalSystem(distinctEmployeeCodes, startDate, endDate,
                commitmentTypes, null);


            var resourcesWithStaffableAsRolesTask = _staffingApiClient.GetResourceActiveStaffableAsByEmployeeCodes(
                distinctEmployeeCodes);

            await Task.WhenAll(allocations.resourcesAllocationsTask,
                allocations.resourcesPlaceholderAndPlanningCardAllocationsTask,
                resourcesCommitmentsTask,
                externalCommitmentsTasks.resourcesLoAsTask,
                externalCommitmentsTasks.resourcesTransfersTask,
                externalCommitmentsTasks.resourcesTransitionsTask,
                externalCommitmentsTasks.resourcesVacationsTask,
                externalCommitmentsTasks.resourcesTrainingsTask,
                externalCommitmentsTasks.resourcesTerminationTask,
                externalCommitmentsTasks.resourcesOfficeHolidaysTask,
                externalCommitmentsTasks.resourcesTimeOffsTask,
                resourcesWithStaffableAsRolesTask);

            var resourcesAllocations = allocations.resourcesAllocationsTask.Result;
            var resourcesPlaceholderAndPlanningCardAllocations = allocations.resourcesPlaceholderAndPlanningCardAllocationsTask.Result;
            var resourcesLoAs = externalCommitmentsTasks.resourcesLoAsTask.Result;
            var resourcesTransfers = externalCommitmentsTasks.resourcesTransfersTask.Result;
            var resourcesTransitions = externalCommitmentsTasks.resourcesTransitionsTask.Result;
            var resourcesVacations = externalCommitmentsTasks.resourcesVacationsTask.Result;
            var resourcesTrainings = externalCommitmentsTasks.resourcesTrainingsTask.Result;
            var resourcesTerminations = externalCommitmentsTasks.resourcesTerminationTask.Result;
            var resourcesHolidays = externalCommitmentsTasks.resourcesOfficeHolidaysTask.Result;
            var resourcesTimeOffs = externalCommitmentsTasks.resourcesTimeOffsTask.Result;
            var resourcesCommitments = resourcesCommitmentsTask.Result;
            var resourcesWithStaffableAsRoles = resourcesWithStaffableAsRolesTask.Result;

            var resourceAllocationsAndCommitments = new ResourceCommitment
            {
                Allocations = resourcesAllocations,
                LoAs = resourcesLoAs,
                Commitments = resourcesCommitments,
                Transfers = resourcesTransfers,
                Transitions = resourcesTransitions,
                Trainings = resourcesTrainings,
                Vacations = resourcesVacations,
                Terminations = resourcesTerminations,
                TimeOffs = resourcesTimeOffs,
                PlaceholderAllocations = resourcesPlaceholderAndPlanningCardAllocations,
                StaffableAsRoles = resourcesWithStaffableAsRoles,
                Holidays = resourcesHolidays
            };
            return resourceAllocationsAndCommitments;
        }

        private (Task<IEnumerable<ResourceAssignmentViewModel>> resourcesAllocationsTask,
            Task<IEnumerable<ScheduleMasterPlaceholder>> resourcesPlaceholderAndPlanningCardAllocationsTask)
            GetAllocationsDetailedInfo(string distinctEmployeeCodes, DateTime? startDate, DateTime? endDate,
            string commitmentTypes)
        {
            var getAllCommitments = string.IsNullOrEmpty(commitmentTypes);
            // Get Allocations data from Staffing
            var resourcesAllocationsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Allocation)
                    ? _resourceAllocationService.GetResourceAllocationsByEmployeeCodes(distinctEmployeeCodes, startDate, endDate)
                    : Task.FromResult(Enumerable.Empty<ResourceAssignmentViewModel>());

            // Get placeholder planning card allocations
            var resourcesPlaceholderAndPlanningCardAllocationsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.PlaceholderAndPlanningCard)
                ? _staffingApiClient.GetPlacholderAndPlanningCardAllocationsWithinDateRange(distinctEmployeeCodes, startDate, endDate)
                : Task.FromResult(Enumerable.Empty<ScheduleMasterPlaceholder>());
            return
                (resourcesAllocationsTask,
                resourcesPlaceholderAndPlanningCardAllocationsTask);
        }


        private (Task<IEnumerable<ResourceAssignmentViewModel>> resourcesAllocationsTask,
            Task<IEnumerable<ScheduleMasterPlaceholder>> resourcesPlaceholderAndPlanningCardAllocationsTask)
            GetConfirmedAndPlaceholderAndPlanningCardAllocations(string distinctEmployeeCodes, DateTime? startDate, DateTime? endDate,
            string commitmentTypes)
        {
            var getAllCommitments = string.IsNullOrEmpty(commitmentTypes);
            // Get Allocations data from Staffing
            var resourcesAllocationsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Allocation)
                    ? _staffingApiClient.GetResourceAllocationsByEmployeeCodes(distinctEmployeeCodes, startDate, endDate)
                    : Task.FromResult(Enumerable.Empty<ResourceAssignmentViewModel>());

            // Get placeholder planning card allocations
            var resourcesPlaceholderAndPlanningCardAllocationsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.PlaceholderAndPlanningCard)
                ? _staffingApiClient.GetPlacholderAndPlanningCardAllocationsWithinDateRange(distinctEmployeeCodes, startDate, endDate)
                : Task.FromResult(Enumerable.Empty<ScheduleMasterPlaceholder>());
            return
                (resourcesAllocationsTask,
                resourcesPlaceholderAndPlanningCardAllocationsTask);
        }

        private (Task<IEnumerable<TrainingViewModel>> resourcesTrainingsTask,
            Task<IEnumerable<VacationRequestViewModel>> resourcesVacationsTask,
            Task<IEnumerable<ResourceTimeOff>> resourcesTimeOffsTask,
            Task<IEnumerable<ResourceLoA>> resourcesLoAsTask,
            Task<IEnumerable<ResourceTransfer>> resourcesTransfersTask,
            Task<IEnumerable<ResourceTermination>> resourcesTerminationTask,
            Task<IEnumerable<ResourceTransition>> resourcesTransitionsTask,
            Task<IEnumerable<HolidayViewModel>> resourcesOfficeHolidaysTask) GetCommitmentsFromExternalSystem(
            string distinctEmployeeCodes, DateTime? startDate, DateTime? endDate, string commitmentTypes,
            ResourceView resourcesStaffingAndCommitmentDataReferences)
        {
            var getAllCommitments = string.IsNullOrEmpty(commitmentTypes);
            // Get Training data from BVU
            var resourcesTrainingsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Training)
                ? _bvuCDApiClient.GetTrainingsWithinDateRangeByEmployeeCodes(distinctEmployeeCodes, startDate, endDate)
                : Task.FromResult(Enumerable.Empty<TrainingViewModel>());

            // TODO: Remove this as vacations are pulled from workday
            var resourcesVacationsTask = Task.FromResult(Enumerable.Empty<VacationRequestViewModel>());

            // Get TimeOff data from Workday
            var resourcesTimeOffsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Vacation)
                ? _resourceApiClient.GetEmployeesTimeoffs(distinctEmployeeCodes, startDate, endDate)
                : Task.FromResult(Enumerable.Empty<ResourceTimeOff>());

            // Get LOA data from Workday
            var resourcesLoAsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.LOA)
                ? LoadEmployeesLOAs(startDate, endDate, resourcesStaffingAndCommitmentDataReferences, distinctEmployeeCodes)
                : Task.FromResult(Enumerable.Empty<ResourceLoA>());

            // Get Transfer info from Workday
            var resourcesTransfersTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Transfer)
                ? _resourceApiClient.GetEmployeesPendingTransfersByEndDate(distinctEmployeeCodes, startDate, endDate)
                : Task.FromResult(Enumerable.Empty<ResourceTransfer>());

            // Get Termination data from Workday
            var resourcesTerminationTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Termination)
                ? _resourceApiClient.GetPendingTerminationsWithinDateRangeByEmployeeCodes(distinctEmployeeCodes, startDate, endDate)
                : Task.FromResult(Enumerable.Empty<ResourceTermination>());

            // Get Transition data from Workday
            var resourcesTransitionsTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Transition)
                ? LoadEmployeesTransitions(startDate, endDate, resourcesStaffingAndCommitmentDataReferences, distinctEmployeeCodes)
                : Task.FromResult(Enumerable.Empty<ResourceTransition>());

            // Get Holiday Data from Basis
            var resourcesOfficeHolidaysTask = getAllCommitments || commitmentTypes.Contains(Constants.CommitmentTypes.Holiday)
                ? _basisApiClient.GetOfficeHolidaysWithinDateRangeByEmployeeCodes(distinctEmployeeCodes, startDate, endDate)
                : Task.FromResult(Enumerable.Empty<HolidayViewModel>());


            Task<IEnumerable<ResourceTransition>> LoadEmployeesTransitions(DateTime? startDate, DateTime? endDate,
               ResourceView resourcesStaffingAndCommitmentDataReferences, string distinctEmployeeCodes)
            {
                if (resourcesStaffingAndCommitmentDataReferences != null && resourcesStaffingAndCommitmentDataReferences.Transitions.Any())
                {
                    return Task.FromResult(resourcesStaffingAndCommitmentDataReferences?.Transitions);

                }
                return _resourceApiClient.GetTransitionsWithinDateRangeByEmployeeCodes(distinctEmployeeCodes, startDate, endDate);


            }


            Task<IEnumerable<ResourceLoA>> LoadEmployeesLOAs(DateTime? startDate, DateTime? endDate,
                ResourceView resourcesStaffingAndCommitmentDataReferences, string distinctEmployeeCodes)
            {
                if (resourcesStaffingAndCommitmentDataReferences != null && resourcesStaffingAndCommitmentDataReferences.LoAs.Any())
                {
                    return Task.FromResult(resourcesStaffingAndCommitmentDataReferences?.LoAs);

                }
                return _resourceApiClient.GetLOAsWithinDateRangeByEmployeeCodes(distinctEmployeeCodes, startDate, endDate);

            }


            return
                (
                resourcesTrainingsTask,
                resourcesVacationsTask,
                resourcesTimeOffsTask,
                resourcesLoAsTask,
                resourcesTransfersTask,
                resourcesTerminationTask,
                resourcesTransitionsTask,
                resourcesOfficeHolidaysTask
                );
        }

        public async Task<(ResourceView resourcesStaffingAndCommitment, IList<Resource> resources)>
           GetResourcesAllocationsAndCommitmentsForResourcesTab(IEnumerable<Resource> filteredResources,
           DateTime? startDate, DateTime? endDate,
           string commitmentTypes, ResourceView resourcesStaffingAndCommitmentDataReferences)
        {
            return await GetResourcesAllocationsAndCommitmentsForResourcesTab(filteredResources, startDate,
                endDate, commitmentTypes, resourcesStaffingAndCommitmentDataReferences, null);
        }

        public async Task<(ResourceView resourcesStaffingAndCommitment, IList<Resource> resources)>
        GetResourcesAllocationsAndCommitmentsForResourcesTab(IEnumerable<Resource> filteredResources,
        DateTime? startDate, DateTime? endDate,
        string commitmentTypes, ResourceView resourcesStaffingAndCommitmentDataReferences, string loggedInuser)
        {
            var noteTypeCode = "RA"; 
            var distinctEmployeeCodes = string.Join(',', filteredResources?.Select(r => r.EmployeeCode).Distinct());

            var resourcesTask = _resourceApiClient.GetEmployeesIncludingTerminated();

            var getAllCommitments = string.IsNullOrEmpty(commitmentTypes);

            // Get Commitments saved in Staffing
            var resourcesCommitmentsTask = resourcesStaffingAndCommitmentDataReferences != null && resourcesStaffingAndCommitmentDataReferences.Commitments != null
                ? Task.FromResult(resourcesStaffingAndCommitmentDataReferences?.Commitments)
                : _staffingApiClient.GetResourceCommitmentsWithinDateRangeByEmployees(distinctEmployeeCodes, startDate, endDate, null);


            var allocations = GetConfirmedAndPlaceholderAndPlanningCardAllocations(distinctEmployeeCodes, startDate, endDate, commitmentTypes);

            var externalCommitmentsTasks = GetCommitmentsFromExternalSystem(distinctEmployeeCodes, startDate, endDate,
                commitmentTypes, resourcesStaffingAndCommitmentDataReferences);


            var resourcesWithStaffableAsRolesTask = resourcesStaffingAndCommitmentDataReferences != null && resourcesStaffingAndCommitmentDataReferences.StaffableAsRoles.Any()
                     ? Task.FromResult(resourcesStaffingAndCommitmentDataReferences.StaffableAsRoles)
                     : _staffingApiClient.GetResourceActiveStaffableAsByEmployeeCodes(distinctEmployeeCodes);

            // Get Resources Notes for Resource View from Staffing
            var resourcesViewNotesTask = !string.IsNullOrEmpty(loggedInuser)
                ? _staffingApiClient.GetResourceViewNotes(distinctEmployeeCodes, loggedInuser, noteTypeCode)
                : Task.FromResult(Enumerable.Empty<ResourceViewNote>());

            var resourceRecentCDTask = _staffingApiClient.GetResourceRecentCD(distinctEmployeeCodes);
            var resourceCommercialModelTask = _staffingApiClient.GetResourceCommercialModel(distinctEmployeeCodes);


            // Get Last Billable Date for Resource View from Staffing
            var lastBillableDatesTask = _staffingApiClient.GetLastBillableDateByEmployeeCodes(distinctEmployeeCodes);

            await Task.WhenAll(allocations.resourcesAllocationsTask,
                allocations.resourcesPlaceholderAndPlanningCardAllocationsTask,
                resourcesCommitmentsTask,
                externalCommitmentsTasks.resourcesLoAsTask,
                externalCommitmentsTasks.resourcesTransfersTask,
                externalCommitmentsTasks.resourcesTransitionsTask,
                externalCommitmentsTasks.resourcesVacationsTask,
                externalCommitmentsTasks.resourcesTrainingsTask,
                externalCommitmentsTasks.resourcesTerminationTask,
                externalCommitmentsTasks.resourcesTimeOffsTask,
                externalCommitmentsTasks.resourcesOfficeHolidaysTask,
                resourcesWithStaffableAsRolesTask,
                resourcesViewNotesTask,
                resourceRecentCDTask,
                resourceCommercialModelTask,
                lastBillableDatesTask,
                resourcesTask
                );

            var resourcesAllocations = allocations.resourcesAllocationsTask.Result;
            var placeholderAllocations = allocations.resourcesPlaceholderAndPlanningCardAllocationsTask.Result;
            var resourcesCommitments = resourcesCommitmentsTask.Result;
            var resourcesLoAs = externalCommitmentsTasks.resourcesLoAsTask.Result;
            var resourcesTransfers = externalCommitmentsTasks.resourcesTransfersTask.Result;
            var resourcesTransitions = externalCommitmentsTasks.resourcesTransitionsTask.Result;
            var resourcesVacations = externalCommitmentsTasks.resourcesVacationsTask.Result;
            var resourcesTrainings = externalCommitmentsTasks.resourcesTrainingsTask.Result;
            var resourcesTerminations = externalCommitmentsTasks.resourcesTerminationTask.Result;
            var resourcesTimeOffs = externalCommitmentsTasks.resourcesTimeOffsTask.Result;
            var resourcesWithStaffableAsRoles = resourcesWithStaffableAsRolesTask.Result;
            var resourcesOfficeHolidays = externalCommitmentsTasks.resourcesOfficeHolidaysTask.Result;
            var resourceViewNotes = resourcesViewNotesTask.Result;
            var resourceRecentCD = resourceRecentCDTask.Result;
            var resourceCommercialModel = resourceCommercialModelTask.Result;
            var lastBillableDates = lastBillableDatesTask.Result;
            var resources = resourcesTask.Result;


            var resourceAllocationsAndCommitments = new ResourceView
            {
                Allocations = resourcesAllocations.ToList(),
                PlaceholderAllocations = JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(JsonConvert.SerializeObject(placeholderAllocations)).ToList(),
                LoAs = resourcesLoAs,
                Commitments = resourcesCommitments,
                Transfers = resourcesTransfers,
                Trainings = resourcesTrainings,
                Transitions = resourcesTransitions,
                Vacations = resourcesVacations,
                Terminations = resourcesTerminations,
                TimeOffs = resourcesTimeOffs,
                Holidays = resourcesOfficeHolidays,
                StaffableAsRoles = resourcesWithStaffableAsRoles,
                ResourceViewNotes = ConvertToResourceViewNotesViewModel(resourceViewNotes, resources),
                ResourceCD = ConvertToResourceCDViewModel(resourceRecentCD, resources),
                ResourceCommercialModel = ConvertToResourceCommercialModelViewModel(resourceCommercialModel,resources),
                LastBillableDates = lastBillableDates,

            };

            filteredResources = resources.Join(
                   filteredResources,
                res => res.EmployeeCode,
                filRes => filRes.EmployeeCode,
                (res, sortedres) => res);


            return (resourceAllocationsAndCommitments, filteredResources.ToList());


        }

        public IEnumerable<Resource> GetResourcesFilteredByStaffingTags(DateTime startDate, DateTime endDate, IEnumerable<Resource> resources, IEnumerable<CommitmentViewModel> commitments,
           string staffingTags)
        {
            if (string.IsNullOrEmpty(staffingTags))
                return resources.Except(
                    resources.Where(r => commitments.Any(c => c.EmployeeCode.Equals(r.EmployeeCode) && c.EndDate >= endDate && c.StartDate <= startDate)));

            var staffingTagCodes = staffingTags.Split(',');

            var resourcesFilteredByServiceLines = resources.Where(r => r.ServiceLine != null && staffingTagCodes.Contains(r.ServiceLine.ServiceLineCode));
            var resourcesFilteredByCommitments = resources.Where(r => commitments.Any(c => c.EmployeeCode.Equals(r.EmployeeCode) && staffingTagCodes.Any(s => s.Equals(c.CommitmentTypeCode))));
            var resourcesOnOtherCommitmentWithinDateRangeSelected = resources.Where(r => commitments.Any(c => c.EmployeeCode.Equals(r.EmployeeCode) &&
                                                                                                      !staffingTagCodes.Any(s => s.Equals(c.CommitmentTypeCode)) && c.EndDate >= endDate && c.StartDate <= startDate));

            var resourcesFilteredByStaffingTags = resourcesFilteredByServiceLines.Union(resourcesFilteredByCommitments)
                .Except(resourcesOnOtherCommitmentWithinDateRangeSelected);

            return resourcesFilteredByStaffingTags;
        }

        public async Task<(IEnumerable<Resource> , IEnumerable<EmployeePracticeArea> )> GetEmployeesFilteredByAdditionalFilters(SupplyFilterCriteria supplyFilterCriteria,
            IEnumerable<Resource> filteredResources, ResourceView resourcesStaffingAndCommitmentDataReferences)
        {
            var unfilteredEmployeeCodes = string.Join(',', filteredResources.Select(r => r.EmployeeCode).Distinct());
            var employeeWithPracticeAreaAffiliationsTask = GetEmployeesWithPracticeAreaAffiliationsDataTask(unfilteredEmployeeCodes, null,null);
            var employeesWithLOAsDataTask = GetEmployeesWithLOAsWitihinDateRangeDataTask(supplyFilterCriteria, unfilteredEmployeeCodes);
            var employeesWithTransitionsDataTask = GetEmployeesWithTransitionsWithinDateRangeDataTask(supplyFilterCriteria, unfilteredEmployeeCodes);
            // Todo: remove the commented code for STaffable as, since logic moved
            //var employeesWithStaffableAsRolesDataTask = GetResourcesActiveStaffableAsByEmployeeCodesDataTask(supplyFilterCriteria, unfilteredEmployeeCodes);
            await Task.WhenAll(
                                employeeWithPracticeAreaAffiliationsTask,
                                employeesWithLOAsDataTask,
                                employeesWithTransitionsDataTask
                                //employeesWithStaffableAsRolesDataTask
                                );

            var employeesWithPracticeAreaAffiliations = employeeWithPracticeAreaAffiliationsTask.Result;
            var employeesWithLOAs = employeesWithLOAsDataTask.Result;
            var employeesWithTransitions = employeesWithTransitionsDataTask.Result;
            //var employeesWithStaffableAsRoles = employeesWithStaffableAsRolesDataTask.Result;

            resourcesStaffingAndCommitmentDataReferences.LoAs = employeesWithLOAs;
            resourcesStaffingAndCommitmentDataReferences.Transitions = employeesWithTransitions;
            //resourcesStaffingAndCommitmentDataReferences.StaffableAsRoles = employeesWithStaffableAsRoles;

            filteredResources = FilterResourcesBySelectedPracticeAreaAffiliations(supplyFilterCriteria.PracticeAreaCodes, supplyFilterCriteria.AffiliationRoleCodes, employeesWithPracticeAreaAffiliations, filteredResources);
            filteredResources = FilterResourcesBySelectedEmployeeStatuses(supplyFilterCriteria, filteredResources, employeesWithLOAs, employeesWithTransitions);

            //filteredResources = FilterResourcesBySelectedStaffableAsTypes(supplyFilterCriteria.StaffableAsTypeCodes, employeesWithStaffableAsRoles, filteredResources);

            return (filteredResources, employeesWithPracticeAreaAffiliations); 
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
                SharedWithDetails = !string.IsNullOrEmpty(note.SharedWith) ?  resources?.Where(x => note.SharedWith.Split(',').Contains(x.EmployeeCode)).ToList() : null,
                CreatedBy = note.CreatedBy,
                CreatedByName = resources.FirstOrDefault(x => x.EmployeeCode == note.CreatedBy).FullName,
                NoteTypeCode = note.NoteTypeCode,
                LastUpdatedBy = note.LastUpdatedBy,
                LastUpdated = TimeZoneInfo.ConvertTimeToUtc(note.LastUpdated, est)
            });

            return resourceViewNotesModel;
        }

        public IEnumerable<ResourceViewCDViewModel> ConvertToResourceCDViewModel(IEnumerable<ResourceCD> resourceCD, IEnumerable<Resource> resources)
        {
            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            IEnumerable<ResourceViewCDViewModel> resourceCDModel = resourceCD.Select(cd => new ResourceViewCDViewModel
            {
                Id = cd.Id,
                EmployeeCode = cd.EmployeeCode,
                RecentCD = cd.RecentCD,
                CreatedBy = cd.LastUpdatedBy,
                CreatedByName = resources.FirstOrDefault(x => x.EmployeeCode == cd.LastUpdatedBy).FullName,
                LastUpdatedBy = cd.LastUpdatedBy,
                LastUpdated = TimeZoneInfo.ConvertTimeToUtc(cd.LastUpdated, est)
            });

            return resourceCDModel;
        }

        public IEnumerable<ResourceViewCommercialModelViewModel> ConvertToResourceCommercialModelViewModel(IEnumerable<ResourceCommercialModel> resourceCommercialModel, IEnumerable<Resource> resources)
        {
            var est = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            IEnumerable<ResourceViewCommercialModelViewModel> resourceViewCommercialModel = resourceCommercialModel.Select(cm => new ResourceViewCommercialModelViewModel
            {
                Id = cm.Id,
                EmployeeCode = cm.EmployeeCode,
                CommercialModel = cm.CommercialModel,
                CreatedBy = cm.LastUpdatedBy,
                CreatedByName = resources.FirstOrDefault(x => x.EmployeeCode == cm.LastUpdatedBy).FullName,
                LastUpdatedBy = cm.LastUpdatedBy,
                LastUpdated = TimeZoneInfo.ConvertTimeToUtc(cm.LastUpdated, est)
            });

            return resourceViewCommercialModel;
        }



        public Task<IEnumerable<EmployeePracticeArea>> GetEmployeesWithPracticeAreaAffiliationsDataTask(string unfilteredEmployeeCodes, string practiceAreaCodes, string affiliationRoleCodes)
        {
            return  _basisApiClient.GetPracticeAreaAffiliationsByEmployeeCodes(unfilteredEmployeeCodes, practiceAreaCodes, affiliationRoleCodes);
        }

        public IEnumerable<Resource> FilterResourcesBySelectedPracticeAreaAffiliations(string supplyCriteriaPracticeAreas, string supplyCriteriaAffiliationRole, IEnumerable<EmployeePracticeArea> employeesWithPracticeAreaAffiliations, IEnumerable<Resource> filteredResources)
        {
            if (string.IsNullOrEmpty(supplyCriteriaPracticeAreas))
                return filteredResources;

            var practiceAreaCodes = supplyCriteriaPracticeAreas.Split(',')
                                                      .Select(code => code.Trim()).ToList();
            var practiceAreaRoles = supplyCriteriaAffiliationRole?.Split(',')
                                                      .Select(role => role.Trim()).ToList();

            // Get employee codes with selected practice area affiliations
            var filteredEmployeesWithSelectedPracticeArea = employeesWithPracticeAreaAffiliations
                .Where(e => practiceAreaCodes.Contains(e.PracticeAreaCode) && (string.IsNullOrEmpty(supplyCriteriaAffiliationRole) || practiceAreaRoles.Contains(e.RoleCode)));

            var employeesWithSelectedPracticeArea = string.Join(',', filteredEmployeesWithSelectedPracticeArea.Select(x => x.EmployeeCode).Distinct());
            filteredResources = filteredResources.Where(x => employeesWithSelectedPracticeArea.Contains(x.EmployeeCode));
            return filteredResources;
        }
        public async Task<IEnumerable<EmployeeCertificates>> GetEmployeesWithCertificatesByEmployeeCodes(string employeeCodes)
        {
            return await _resourceApiClient.GetCertificatesByEmployeeCodes(employeeCodes);
        }

        public async Task<IEnumerable<EmployeeLanguages>> GetEmployeesWithLanguagesByEmployeeCodes(string employeeCodes)
        {
            return  await _resourceApiClient.GetLanguagesByEmployeeCodes(employeeCodes);
        }

        #region Private Method

        private Task<IEnumerable<ResourceLoA>> GetEmployeesWithLOAsWitihinDateRangeDataTask(SupplyFilterCriteria supplyFilterCriteria, string unfilteredEmployeeCodes)
        {
            return !string.IsNullOrEmpty(supplyFilterCriteria.EmployeeStatuses) ? _resourceApiClient.GetLOAsWithinDateRangeByEmployeeCodes(unfilteredEmployeeCodes, supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate)
              : Task.FromResult<IEnumerable<ResourceLoA>>(new List<ResourceLoA>());
        }
        private Task<IEnumerable<ResourceTransition>> GetEmployeesWithTransitionsWithinDateRangeDataTask(SupplyFilterCriteria supplyFilterCriteria, string unfilteredEmployeeCodes)
        {
            return !string.IsNullOrEmpty(supplyFilterCriteria.EmployeeStatuses) ? _resourceApiClient.GetTransitionsWithinDateRangeByEmployeeCodes(unfilteredEmployeeCodes, supplyFilterCriteria.StartDate, supplyFilterCriteria.EndDate)
               : Task.FromResult<IEnumerable<ResourceTransition>>(new List<ResourceTransition>());

        }
        private Task<IEnumerable<StaffableAs>> GetResourcesActiveStaffableAsByEmployeeCodesDataTask(SupplyFilterCriteria supplyFilterCriteria, string unfilteredEmployeeCodes)
        {
            return !string.IsNullOrEmpty(supplyFilterCriteria.StaffableAsTypeCodes) ? _staffingApiClient.GetResourceActiveStaffableAsByEmployeeCodes(unfilteredEmployeeCodes)
               : Task.FromResult<IEnumerable<StaffableAs>>(new List<StaffableAs>());
        }

        private IEnumerable<Resource> FilterResourcesBySelectedStaffableAsTypes(string supplyCriteriaStaffableAsTypeCodes, IEnumerable<StaffableAs> employeesWithStaffableAsRoles, IEnumerable<Resource> filteredResources)
        {
            if (string.IsNullOrEmpty(supplyCriteriaStaffableAsTypeCodes))
                return filteredResources;

            var employeesWithSelectedStaffableAsRoles = employeesWithStaffableAsRoles.Where(x => supplyCriteriaStaffableAsTypeCodes.Split(',').Contains(x.StaffableAsTypeCode.ToString()));
            filteredResources = filteredResources.Where(x => employeesWithSelectedStaffableAsRoles.Any(y => y.EmployeeCode == x.EmployeeCode));
            return filteredResources;
        }


        private IEnumerable<Resource> FilterResourcesBySelectedLanguages(string supplyCriteriaLanguages, IEnumerable<EmployeeLanguages> employeesWithLanguages, IEnumerable<Resource> filteredResources)
        {
            if (string.IsNullOrEmpty(supplyCriteriaLanguages))
                return filteredResources;

            var selectedLanguages = supplyCriteriaLanguages.Split(',').Select(value => value.Trim().ToLower());
            var employeesHavingSelectedLanguages = new List<string>();
            var selectedEmployeesWithLanguages = employeesWithLanguages.Where(employeeLanguages =>
            {
                var employeeLanguagesNameList = employeeLanguages.Languages?.Select(x => x.Name.ToLower()).ToArray();
                return employeeLanguagesNameList != null ? selectedLanguages.All(langName => Array.IndexOf(employeeLanguagesNameList, langName) > -1) : false;
            }).Select(value => value.EmployeeCode);
            return filteredResources.Where(resource => selectedEmployeesWithLanguages.Contains(resource.EmployeeCode));
        }
        private IEnumerable<Resource> FilterResourcesBySelectedEmployeeStatuses(SupplyFilterCriteria supplyFilterCriteria, IEnumerable<Resource> unfilteredResources,
            IEnumerable<ResourceLoA> employeesWithLOAs, IEnumerable<ResourceTransition> employeesWithTransitions)
        {
            //If no status is selected then don't show any
            if (string.IsNullOrEmpty(supplyFilterCriteria.EmployeeStatuses))
                return Enumerable.Empty<Resource>();

            if (supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.Active) && supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.NotYetStarted)
                        && supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.LOA) && supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.Transition))
                return unfilteredResources;

            var filteredResources = new List<Resource>();
            var employeesOnLOAForFullDateRange = employeesWithLOAs.Where(x => x.StartDate.Value.Date <= supplyFilterCriteria.StartDate.Date && x.EndDate.Value.Date >= supplyFilterCriteria.EndDate.Date);
            var employeesWithNotYetStartedStatus = unfilteredResources.Where(x => string.Equals(x.ActiveStatus, "Not Yet Started", StringComparison.OrdinalIgnoreCase));

            if (supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.LOA) && employeesOnLOAForFullDateRange.Any())
            {
                string eCodes = string.Join(",", employeesOnLOAForFullDateRange.Select(x => x.EmployeeCode).Distinct());
                filteredResources.AddRange(unfilteredResources.Where(x => eCodes.Contains(x.EmployeeCode)));
            }
            if (supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.Transition) && employeesWithTransitions.Any())
            {
                string eCodes = string.Join(",", employeesWithTransitions.Select(x => x.EmployeeCode).Distinct());
                filteredResources.AddRange(unfilteredResources.Where(x => eCodes.Contains(x.EmployeeCode)));
            }
            if (supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.NotYetStarted) && employeesWithNotYetStartedStatus.Any())
            {
                filteredResources.AddRange(employeesWithNotYetStartedStatus);
            }
            if (supplyFilterCriteria.EmployeeStatuses.Contains(Constants.EmployeeStatus.Active))
            {
                string eCodeLoa = string.Join(",", employeesOnLOAForFullDateRange.Select(x => x.EmployeeCode).Distinct());
                string eCodeTransition = string.Join(",", employeesWithTransitions.Select(x => x.EmployeeCode).Distinct());
                string eCodeNYS = string.Join(",", employeesWithNotYetStartedStatus.Select(x => x.EmployeeCode).Distinct());

                filteredResources.AddRange(unfilteredResources.Where(x => !eCodeLoa.Contains(x.EmployeeCode) && !eCodeTransition.Contains(x.EmployeeCode) && !eCodeNYS.Contains(x.EmployeeCode)));
            }

            return filteredResources.GroupBy(x => x.EmployeeCode).Select(grp => grp.First());
        }

        private IEnumerable<Resource> FilterResourcesBySelectedCertifications(string supplyCriteriaCertifications, IEnumerable<EmployeeCertificates> employeesWithCertifications, IEnumerable<Resource> filteredResources)
        {
            if (string.IsNullOrEmpty(supplyCriteriaCertifications))
                return filteredResources;

            var selectedCertificates = supplyCriteriaCertifications.Split(',').Select(value => value.Trim().ToLower());
            var employeesHavingSelectedCertificates = new List<string>();
            var selecteEmployeesWithCertificates = employeesWithCertifications.Where(employeeCertificates =>
            {
                var employeeCertificateNameList = employeeCertificates.Certifications?.Select(x => x.Name.ToLower()).ToArray();
                return employeeCertificateNameList != null ? selectedCertificates.All(certName => Array.IndexOf(employeeCertificateNameList, certName) > -1) : false;
            }).Select(value => value.EmployeeCode);

            return filteredResources.Where(resource => selecteEmployeesWithCertificates.Contains(resource.EmployeeCode));
        }

        public IEnumerable<Resource> FilterResourcesByLevelGradePositionAndStaffableAs(
        IEnumerable<Resource> resourcesData,
        IEnumerable<StaffableAs> employeesWithStaffableAsRoles,
        SupplyFilterCriteria supplyFilterCriteria)
        {
            var filteredResources = resourcesData;

            // Create a set of EmployeeCodes with the selected StaffableAsTypeCodes
            var staffableEmployeeCodes = new List<string>();
            if (!string.IsNullOrEmpty(supplyFilterCriteria.StaffableAsTypeCodes))
            {
                var employeesWithSelectedStaffableAsRoles = employeesWithStaffableAsRoles
                    .Where(x => supplyFilterCriteria.StaffableAsTypeCodes.Split(',')
                    .Contains(x.StaffableAsTypeCode.ToString())).ToList();

                staffableEmployeeCodes = employeesWithSelectedStaffableAsRoles.Select(x => x.EmployeeCode).Distinct().ToList();
            }

            // if both are null then filter only on the basis of staffable as
            if (string.IsNullOrEmpty(supplyFilterCriteria.LevelGrades) && string.IsNullOrEmpty(supplyFilterCriteria.PositionCodes) && !string.IsNullOrEmpty(supplyFilterCriteria.StaffableAsTypeCodes))
            {
                filteredResources = filteredResources
                        .Where(r => staffableEmployeeCodes.Contains(r.EmployeeCode)).ToList();
            }

            // Filter by (LevelGrade OR StaffableAs), but only apply if LevelGrades are provided
            if (!string.IsNullOrEmpty(supplyFilterCriteria.LevelGrades))
            {
                var levels = supplyFilterCriteria.LevelGrades.Split(',');
                if (staffableEmployeeCodes.Any())
                {
                    // Apply (LevelGrade OR StaffableAs)
                    filteredResources = filteredResources
                        .Where(r => levels.Contains(r.LevelGrade) || staffableEmployeeCodes.Contains(r.EmployeeCode)).ToList();
                }
                else
                {
                    // Apply only LevelGrade filter if StaffableAs is empty
                    filteredResources = filteredResources
                        .Where(r => levels.Contains(r.LevelGrade)).ToList();
                }
            }

            // Filter by (PositionCode OR StaffableAs), but only apply if PositionCodes are provided
            if (!string.IsNullOrEmpty(supplyFilterCriteria.PositionCodes))
            {
                var positions = supplyFilterCriteria.PositionCodes.Split(',');
                if (staffableEmployeeCodes.Any())
                {
                    // Apply (PositionCode OR StaffableAs)
                    filteredResources = filteredResources
                        .Where(r => positions.Contains(r.Position?.PositionCode) || staffableEmployeeCodes.Contains(r.EmployeeCode)).ToList();
                }
                else
                {
                    // Apply only PositionCode filter if StaffableAs is empty
                    filteredResources = filteredResources
                        .Where(r => positions.Contains(r.Position?.PositionCode)).ToList();
                }
            }

            return filteredResources.DistinctBy(r => r.EmployeeCode).ToList();
        }
        #endregion
    }
}
