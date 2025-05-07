using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IResourceService
    {
        Task<ResourceCommitment> GetResourcesFilteredBySelectedValues(SupplyFilterCriteria supplyFilterCriteria, string loggedInUser);
        Task<ResourceCommitment> GetResourcesFilteredBySelectedGroupValues(SupplyGroupFilterCriteria supplyGroupFilterCriteria, string loggedInUser);
        Task<ResourceCommitment> GetResourcesAllocationsAndCommitmentsBySearchString(string searchString, bool? addTransfers = false);
        Task<ResourceCommitment> GetResourcesIncludingTerminatedAllocationsAndCommitmentsBySearchString(string searchString, bool? addTransfers = false);
        Task<ResourceCommitment> GetResourcesAllocationsAndCommitmentsWithinDateRange(string employeeCodes, DateTime? startDate, DateTime? endDate, string commitmentTypes);
        Task<AdvisorViewModel> GetAdvisorNameByEmployeeCode(string employeeCode);
        Task<IEnumerable<MenteeViewModel>> GetMenteeNamesByEmployeeCode(string employeeCode);
    }
}
