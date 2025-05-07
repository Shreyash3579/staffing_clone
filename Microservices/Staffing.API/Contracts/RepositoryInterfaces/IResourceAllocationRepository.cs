using Staffing.API.Models;
using Staffing.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.API.Contracts.RepositoryInterfaces
{
    public interface IResourceAllocationRepository
    {Task<IEnumerable<ResourceAllocation>> UpsertResourceAllocations(DataTable resourceAllocationDataTable);
        Task DeleteResourceAllocationById(Guid id, string lastUpdatedBy);
        Task DeleteResourceAllocationByIds(string ids, string lastUpdatedBy);        
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourcesCount>> GetResourceAllocationsCountByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByPipelineIds(string pipelineIds);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByScheduleIds(string scheduleIds);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedValues(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedValuesV2(string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes, string action);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedValues(string action, string oldCaseCodes,
            string employeeCodes, DateTime? lastUpdated, DateTime? startDate, DateTime? endDate, string caseRoleCodes);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByOfficeCodes(string officeCodes, DateTime startDate, DateTime endDate);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
       Task<IEnumerable<ResourceAllocation>> GetRecordsUpdatedOnCaseRoll(string oldCaseCode, DateTime caseCurrentEndDate);
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsOnCaseRollByCaseCodes(string oldCaseCodes);
        Task<IEnumerable<ResourceAllocation>> GetAllocationsForEmployeesOnPrePost();
        Task<IEnumerable<ResourceAllocation>> GetResourceAllocationsBySelectedSupplyValues(string officeCodes, DateTime startDate, DateTime endDate,
            string staffingTags, string currentLevelGrades);
        Task<IEnumerable<ResourceAllocation>> GetLastTeamByEmployeeCode(string employeeCode, DateTime? date);
        Task<IEnumerable<EmployeeLastBillableDateViewModel>> GetLastBillableDateByEmployeeCodes(string employeeCodes, DateTime? date);

    }
}
