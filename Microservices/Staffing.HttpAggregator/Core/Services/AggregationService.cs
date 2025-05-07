using Microsoft.AspNetCore.Http;
using Staffing.HttpAggregator.Contracts.Services;
using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Core.Services
{
    public class AggregationService : IAggregationService
    {
        private readonly IStaffingService _staffingSerive;
        private readonly IVacationApiClient _vacationApiClient;
        private readonly IBasisApiClient _basisApiClient;
        private readonly IBvuCDApiClient _bvuCDApiClient;
        private readonly IResourceApiClient _resourceApiClient;
        private readonly IStaffingApiClient _staffingApiClient;
        private readonly IResourcePlaceholderAllocationService _resourcePlaceholderAllocationService;
        private readonly IResourceService _resourceService;
        private readonly IRevenueApiClient _revenueApiClient;

        public AggregationService(IStaffingService staffingSerive, IVacationApiClient vacationApiClient,IBasisApiClient basisApiClient,
            IBvuCDApiClient bvuCDApiClient, IResourceApiClient resourceApiClient,IStaffingApiClient staffingApiClient, 
            IResourcePlaceholderAllocationService resourcePlaceholderAllocationService, IResourceService resourceService, IRevenueApiClient revenueApiClient)
        {
            _staffingSerive = staffingSerive;
            _vacationApiClient = vacationApiClient;
            _basisApiClient = basisApiClient;
            _bvuCDApiClient = bvuCDApiClient;
            _resourceApiClient = resourceApiClient;
            _staffingApiClient = staffingApiClient;
            _resourcePlaceholderAllocationService = resourcePlaceholderAllocationService;
            _resourceService = resourceService;
            _revenueApiClient = revenueApiClient;
        }

        public async Task<ResourceCommitmentViewModel> GetAllCommitmentsForEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var staffingAllocations = await _staffingSerive.GetStaffingAllocationsForEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            var vacationRequests = await _vacationApiClient.GetVacationRequestsByEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            var officeHolidays = await _basisApiClient.GetOfficeHolidaysByEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            var trainings = await _bvuCDApiClient.GetTrainingsByEmployee(employeeCode, effectiveFromDate, effectiveToDate);
            var loas = await _resourceApiClient.GetLOAsByEmployeeCode(employeeCode, effectiveFromDate, effectiveToDate);
            var commitmentsSavedInStaffing = await _staffingApiClient.GetResourceCommitments(employeeCode, effectiveFromDate, effectiveToDate);
            var employeeTransition = await _resourceApiClient.GetTransitionByEmployeeCode(employeeCode, effectiveFromDate, effectiveToDate);
            var employeeTransfers = await _resourceApiClient.GetEmployeeTransfersWithinDateRange(employeeCode, effectiveFromDate, effectiveToDate);
            var employeeTermination = await _resourceApiClient.GetTerminationByEmployeeCode(employeeCode, effectiveFromDate, effectiveToDate);
            var employeeTimeOffs = await _resourceApiClient.GetEmployeeTimeoffs(employeeCode, effectiveFromDate, effectiveToDate);
            var placeholderAllocations = await _resourcePlaceholderAllocationService.GetPlaceholderAllocationsByEmployeeCode(employeeCode, effectiveFromDate, effectiveToDate);

            var resourceCommitments = ConvertToResourceCommitmentViewModel(staffingAllocations, vacationRequests, officeHolidays,
                trainings, loas, commitmentsSavedInStaffing, employeeTransition, employeeTransfers, employeeTermination,
                employeeTimeOffs,placeholderAllocations);
            return resourceCommitments;
        }

        public async Task<EmployeeInfoWithGxcAffiliationsViewModel> GetEmployeeInfoWithGxcAffiliations(string employeeCode)
        {
            var employee = await _resourceApiClient.GetEmployeeByEmployeeCode(employeeCode);
            var affiliationsByEmployeeCodesAndPracticeAreaCodes = await _basisApiClient.GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(employeeCode);
            var employeeAdvisor = await _resourceService.GetAdvisorNameByEmployeeCode(employeeCode);
            var employeeMentees = await _resourceService.GetMenteeNamesByEmployeeCode(employeeCode);
            var employeeTimeInLevel = await _resourceApiClient.GetTimeInLevelByEmployeeCode(employeeCode);

            var employeeInfoWithGxcAffiliations = ConvertToEmployeeInfoWithGxcAffiliations(
                employee, affiliationsByEmployeeCodesAndPracticeAreaCodes,
                employeeAdvisor, employeeMentees,
                employeeTimeInLevel);

            return employeeInfoWithGxcAffiliations;
        }

        private ResourceCommitmentViewModel ConvertToResourceCommitmentViewModel(IEnumerable<ResourceAssignmentViewModel> staffingAllocations,
             IEnumerable<VacationRequestViewModel> vacationRequests, IEnumerable<HolidayViewModel> officeHolidays, IEnumerable<TrainingViewModel> trainings,
             IEnumerable<LOA> loas, IEnumerable<Commitment> commitmentsSavedInStaffing, ResourceTransition employeeTransition,
             IEnumerable<ResourceTransfer> employeeTransfers, ResourceTermination employeeTermination,
             IEnumerable<ResourceTimeOff> employeeTimeOffs, IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations) 
        {
            var resourceCommitments = new ResourceCommitmentViewModel
            {
                staffingAllocations = staffingAllocations,
                vacationRequests = vacationRequests,
                officeHolidays = officeHolidays,
                trainings = trainings,
                loa = loas,
                commitmentsSavedInStaffing = commitmentsSavedInStaffing,
                employeeTransition = employeeTransition,
                employeeTransfers = employeeTransfers,
                employeeTermination = employeeTermination,
                employeeTimeOffs = employeeTimeOffs,
                placeholderAllocations = placeholderAllocations
            };

            return resourceCommitments;
        }

        private EmployeeInfoWithGxcAffiliationsViewModel ConvertToEmployeeInfoWithGxcAffiliations(
            Resource employee, IEnumerable<EmployeePracticeAreaViewModel> affiliationsByEmployeeCodesAndPracticeAreaCodes,
            AdvisorViewModel employeeAdvisor, IEnumerable<MenteeViewModel> employeeMentees,
            IEnumerable<ResourceTimeInLevel> employeeTimeInLevel)
        {
            var employeeInfoWithGxcAffiliations = new EmployeeInfoWithGxcAffiliationsViewModel
            {
                employee = employee,
                affiliationsByEmployeeCodesAndPracticeAreaCodes = affiliationsByEmployeeCodesAndPracticeAreaCodes,
                employeeAdvisor = employeeAdvisor,
                employeeMentees = employeeMentees,
                employeeTimeInLevel = employeeTimeInLevel
            };

            return employeeInfoWithGxcAffiliations;
        }
    }
}