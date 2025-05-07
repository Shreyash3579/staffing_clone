using Staffing.Analytics.API.Models;
using Staffing.Analytics.API.Models.Workday;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IResourceApiClient
    {
        Task<IEnumerable<ResourceTransaction>> GetEmployeePendingPromotions(string employeeCode);
        Task<IEnumerable<ResourceTransaction>> GetEmployeePendingTransfers(string employeeCode);
        Task<IEnumerable<ResourceTransaction>> GetFutureTransitionByEmployeeCode(string employeeCode);
        Task<IEnumerable<ResourceTransaction>> GetFutureLOAsByEmployeeCode(string employeeCode);
        Task<IEnumerable<ResourceLoA>> GetLOAsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime startDate, DateTime endDate);
        Task<IEnumerable<ResourceLoA>> GetEmployeesLoATransactions(string listEmployeeCodes);
        Task<IEnumerable<ResourceTransaction>> GetPendingTransactionsByEmployeeCodes(string employeeCodes);
        Task<IEnumerable<Resource>> GetEmployees();
        Task<IEnumerable<Resource>> GetEmployeesIncludingTerminated();
        Task<IEnumerable<EmployeeTransaction>> GetEmployeesStaffingTransactions(string employeeCodes);
        Task<IEnumerable<ServiceLine>> GetServiceLineList();
        Task<IEnumerable<ResourceTimeOff>> GetEmployeesTimeoffs(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<JobProfile>> GetJobProfileList();
    }
}
