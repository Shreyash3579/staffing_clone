using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.Services
{
    public interface IResourceAllocationService
    {
        Task<IEnumerable<ResourceAllocation>> UpsertResourceAllocations(IEnumerable<ResourceAllocation> resourceAllocations);
        Task DeleteResourceAllocationById(Guid id, string lastUpdatedBy);
        Task DeleteResourceAllocationByIds(string ids, string lastUpdatedBy);
        Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourcesCount>> GetResourceAllocationsCountByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByPipelineIds(string pipelineIds);
        Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByScheduleIds(string scheduleIds);
        Task<(string, IEnumerable<ResourceAllocationViewModel>)> GetResourceAllocationsBySelectedValues(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes = "", string clientId = "");
        Task<(string, IEnumerable<ResourceAllocationViewModel>)> GetResourceAllocationsBySelectedValuesV2(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes = "", string clientId = "", string action = "");
        Task<(string, IEnumerable<ResourceAllocationViewModel>)> GetResourceAllocationsBySelectedValues(string action, string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes = "", string clientId = "");
        Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByOfficeCodes(string officeCodes, DateTime startDate, DateTime endDate);
        Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsOnCaseRollByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourceAllocation>> GetAllocationsForEmployeesOnPrePost();
        Task<IEnumerable<ResourceAllocationViewModel>> GetResourceAllocationsBySelectedSupplyValues(string officeCodes, DateTime startDate, DateTime endDate,
            string staffingTags, string currentLevelGrades);
        Task<IEnumerable<ResourceAllocationViewModel>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date);
        Task<IEnumerable<EmployeeLastBillableDateViewModel>> GetLastBillableDateByEmployeeCodes(string employeeCodes, DateTime? date);
    }
}
