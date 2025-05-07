using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IResourceAllocationService
    {
        Task<IEnumerable<ResourceAssignmentViewModel>> UpsertResourceAllocations(
           IEnumerable<ResourceAssignmentViewModel> resourceAllocation);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetResourceAllocationsByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate, List<Resource> resources = null);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetPlaceholderAllocationsByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate, List<Resource> resources = null);
        Task<IEnumerable<ResourceAssignmentViewModel>> UpsertCaseRollsAndAllocations(IEnumerable<CaseRoll> caseRoll, IEnumerable<ResourceAssignmentViewModel> resourceAllocation);
        Task<IEnumerable<ResourceAssignmentViewModel>> RevertCaseRollAndAllocations(CaseRoll caseRoll, IEnumerable<ResourceAssignmentViewModel> resourceAllocation);
        Task<IEnumerable<CaseRoleAllocationViewModel>> GetCaseRoleAllocationsByOldCaseCodes(string oldCaseCodes);
        Task<IEnumerable<CaseRoleAllocationViewModel>> GetCaseRoleAllocationsByPipelineIds(string pipelineIds);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date);
        Task<IEnumerable<ResourceAssignmentViewModel>> GetAllocationsWithinDateRangeForOfficeClosure(string officeCodes, string caseTypeCodes, DateTime startDate, DateTime endDate, string staffingTags);
    }
}