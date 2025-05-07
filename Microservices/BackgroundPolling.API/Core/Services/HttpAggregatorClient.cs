using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class HttpAggregatorClient : IHttpAggregatorClient
    {
        private readonly HttpClient _apiClient;

        public HttpAggregatorClient(HttpClient apiClient)
        {
            var endpointAddress = ConfigurationUtility.GetValue("AppSettings:HttpAggregatorBaseUrl");
            _apiClient = apiClient;
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(30);
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourceAllocations(IEnumerable<ScheduleMaster> allocationsToUpdate)
        {
            var allocations = ConvertToResourceAssignmentViewModel(allocationsToUpdate);

            var updatedPrePostAllocations = await UpsertResourceAllocations(allocations);
            return updatedPrePostAllocations;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourceAllocations(IEnumerable<ResourceAssignmentViewModel> allocations)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourceAllocationAggregator/upsertResourceAllocations", allocations);
            var updatedAllocations = JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<ResourceAssignmentViewModel>();
            return updatedAllocations;
        }

        public async Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourcePlaceholderAllocations(IEnumerable<ScheduleMasterPlaceholder> placeholdeAllocationsToUpdate)
        {
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourcePlaceholderAllocationAggregator", placeholdeAllocationsToUpdate);
            var updatedAllocations = JsonConvert.DeserializeObject<IEnumerable<ResourceAssignmentViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? Enumerable.Empty<ResourceAssignmentViewModel>();
            return updatedAllocations;
        }

        public async Task<OfficeHierarchy> GetOfficeHierarchy()
        {
            var responseMessage = await _apiClient.GetAsync("api/lookup/officeHierarchy");
            var officeHierarchies = JsonConvert.DeserializeObject<OfficeHierarchy>(responseMessage.Content?.ReadAsStringAsync().Result)
                                ?? new OfficeHierarchy();
            return officeHierarchies;
        }

        public async Task<string> SendMonthlyStaffingAllocationsEmailToExperts(string employeeCodes)
        {
            employeeCodes = employeeCodes ?? string.Empty;

            var responseMessgae = await _apiClient.PostAsJsonAsync("api/expertEmailUtility/sendMonthlyStaffingAllocationsEmailToExperts", employeeCodes);
            var status = responseMessgae.Content?.ReadAsStringAsync().Result;

            return status;
        }

        public async Task<string> SendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign(string employeeCodes)
        {
            employeeCodes = employeeCodes ?? string.Empty;

            var responseMessgae = await _apiClient.PostAsJsonAsync("api/expertEmailUtility/sendMonthlyStaffingAllocationsEmailToApacInnovationAndDesign", employeeCodes);
            var status = responseMessgae.Content?.ReadAsStringAsync().Result;

            return status;
        }

        private IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ScheduleMaster> resourceAllocations)
        {
            var allocations = resourceAllocations.Select(item => new ResourceAssignmentViewModel
            {
                Id = item.Id,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                EmployeeCode = item.EmployeeCode,
                CurrentLevelGrade = item.CurrentLevelGrade,
                OperatingOfficeCode = item.OperatingOfficeCode,
                Allocation = item.Allocation,
                StartDate = Convert.ToDateTime(item.StartDate),
                EndDate = Convert.ToDateTime(item.EndDate),
                ServiceLineCode = item.ServiceLineCode,
                ServiceLineName = item.ServiceLineName,
                InvestmentCode = item.InvestmentCode,
                CaseRoleCode = item.CaseRoleCode,
                LastUpdatedBy = item.LastUpdatedBy,
                Notes = item.Notes,
            });

            return allocations;
        }

        private IEnumerable<ResourceAssignmentViewModel> ConvertToResourceAssignmentViewModel(IEnumerable<ScheduleMasterPlaceholder> placeholderAllocations)
        {
            var allocations = placeholderAllocations.Select(item => new ResourceAssignmentViewModel
            {
                Id = item.Id,
                PlanningCardId = item.PlanningCardId,
                OldCaseCode = item.OldCaseCode,
                PipelineId = item.PipelineId,
                EmployeeCode = item.EmployeeCode,
                CurrentLevelGrade = item.CurrentLevelGrade,
                OperatingOfficeCode = item.OperatingOfficeCode,
                Allocation = item.Allocation,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                ServiceLineCode = item.ServiceLineCode,
                ServiceLineName = item.ServiceLineName,
                InvestmentCode = item.InvestmentCode,
                CaseRoleCode = item.CaseRoleCode,
                CommitmentTypeCode = item.CommitmentTypeCode,
                IsPlaceholderAllocation = item.IsPlaceholderAllocation,
                isConfirmed = item.IsConfirmed,
                LastUpdatedBy = item.LastUpdatedBy,
                Notes = item.Notes,
            });

            return allocations;
        }

        public async Task<string> UpsertCaseRollsAndAllocations(IEnumerable<CaseRoll> caseRolls, IEnumerable<ScheduleMaster> resourceAllocations)
        {
            var allocations = ConvertToResourceAssignmentViewModel(resourceAllocations);

            var payload = new { caseRolls, resourceAllocations = allocations };
            var responseMessage = await _apiClient.PostAsJsonAsync("api/resourceAllocationAggregator/upsertCaseRollsAndAllocations", payload);

            var rolledCases = JsonConvert.DeserializeObject<string>(responseMessage.Content?.ReadAsStringAsync().Result) ?? "";
            return rolledCases;
        }

        
    }
}
