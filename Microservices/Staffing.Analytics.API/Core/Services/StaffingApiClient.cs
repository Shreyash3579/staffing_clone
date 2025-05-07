using Newtonsoft.Json;
using Staffing.Analytics.API.Contracts.Services;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Services
{
    public class StaffingApiClient : IStaffingApiClient
    {
        private readonly HttpClient _apiClient;
        public StaffingApiClient(HttpClient httpClient)
        {
            _apiClient = httpClient;
            var endpointAddress = ConfigurationUtility.GetValue("StaffingApiBaseUrl");
            _apiClient.BaseAddress = new Uri(endpointAddress);
            _apiClient.Timeout = TimeSpan.FromMinutes(20);
        }

        public async Task<IEnumerable<CaseRoleType>> GetCaseRoleTypeLookupList()
        {
            var responseMessage = await _apiClient.GetAsync($"api/lookup/caseRoleTypes");
            var caseRoleTypes = JsonConvert.DeserializeObject<IEnumerable<CaseRoleType>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CaseRoleType>();
            return caseRoleTypes.ToList();
        }
        public async Task<IEnumerable<InvestmentCategory>> GetInvestmentCategoryLookupList()
        {
            var responseMessage = await _apiClient.GetAsync($"api/lookup/investmentTypes");
            var investmentCategories = JsonConvert.DeserializeObject<IEnumerable<InvestmentCategory>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<InvestmentCategory>();
            return investmentCategories.ToList();
        }

        public async Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByScheduleIds(string scheduleIds)
        {
            if (string.IsNullOrEmpty(scheduleIds))
                return new List<ResourceAllocationViewModel>();

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/resourceAllocation/allocationsByScheduleIds", scheduleIds);
            var allocations = JsonConvert.DeserializeObject<IEnumerable<ResourceAllocationViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<ResourceAllocationViewModel>();
            return allocations.ToList();
        }
        public async Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByCommitmentIds(string commitmentIds)
        {
            if (string.IsNullOrEmpty(commitmentIds))
                return new List<CommitmentViewModel>();

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/commitment/commitmentByIds", commitmentIds);
            var allocations = JsonConvert.DeserializeObject<IEnumerable<CommitmentViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CommitmentViewModel>();
            return allocations.ToList();
        }
        public async Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsByDeletedCommitmentIds(string commitmentIds)
        {
            if (string.IsNullOrEmpty(commitmentIds))
                return new List<CommitmentViewModel>();

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/commitment/commitmentByDeletedIds", commitmentIds);
            var allocations = JsonConvert.DeserializeObject<IEnumerable<CommitmentViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CommitmentViewModel>();
            return allocations.ToList();
        }

        public async Task<IEnumerable<CommitmentViewModel>> GetResourceCommitmentsWithinDateRangeByEmployees(string employeeCodes, DateTime? startDate,
                DateTime? endDate, string commitmentTypeCode = null)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<CommitmentViewModel>();

            var payload = new { employeeCodes, startDate, endDate, commitmentTypeCode };

            var responseMessage =
               await _apiClient.PostAsJsonAsync("api/commitment/commitmentsWithinDateRangeByEmployees", payload);

            var commitments =
                JsonConvert.DeserializeObject<IEnumerable<CommitmentViewModel>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? Enumerable.Empty<CommitmentViewModel>();

            return commitments.ToList();
        }

        public async Task<IEnumerable<CommitmentType>> GetCommitmentTypeList(bool? showHidden)
        {
            var responseMessage = await _apiClient.GetAsync($"api/commitment/lookup?showHidden={showHidden}");

            var commitmentTypes =
                JsonConvert.DeserializeObject<IEnumerable<CommitmentType>>(responseMessage.Content?.ReadAsStringAsync()
                    .Result);

            return commitmentTypes;
        }

        public async Task<IEnumerable<PlaceholderAllocationViewModel>> GetPlaceholderAllocationsByScheduleIds(string placeholderScheduleIds)
        {
            if (string.IsNullOrEmpty(placeholderScheduleIds))
                return new List<PlaceholderAllocationViewModel>();

            var responseMessage = await _apiClient.PostAsJsonAsync($"api/scheduleMasterPlaceholder/allocationsByPlaceholderScheduleIds", placeholderScheduleIds);
            var allocations = JsonConvert.DeserializeObject<IEnumerable<PlaceholderAllocationViewModel>>(responseMessage.Content?.ReadAsStringAsync().Result) ?? Enumerable.Empty<PlaceholderAllocationViewModel>();
            return allocations.ToList();
        }

        public async Task<IEnumerable<PlanningCard>> GetPlanningCardByPlanningCardIds(string planningCardIds)
        {
            if (string.IsNullOrEmpty(planningCardIds))
                return new List<PlanningCard>();

            var responseMessage =
                await _apiClient.PostAsJsonAsync("api/planningCard/planningCardsByPlanningCardIds", planningCardIds);

            var planningCards =
                JsonConvert.DeserializeObject<IEnumerable<PlanningCard>>(responseMessage.Content
                    ?.ReadAsStringAsync().Result) ?? new List<PlanningCard>();

            return planningCards;
        }

    }
}
