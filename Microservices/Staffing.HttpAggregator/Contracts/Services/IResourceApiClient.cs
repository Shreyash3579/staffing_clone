using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IResourceApiClient
    {
        Task<List<Resource>> GetEmployees();
        Task<List<Resource>> GetEmployeesIncludingTerminated();
        Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedValues(string officeCodes, DateTime startDate, DateTime endDate,
            string levelGrades, string positionCodes);

        Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedValues(string officeCodes, DateTime startDate, DateTime endDate,
            string levelGrades, string positionCodes, string oDataQuery);
        Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedGroupValues(string employeeCodes, 
            DateTime startDate, DateTime endDate);
        Task<IEnumerable<Resource>> GetActiveEmployeesFilteredBySelectedGroupValues(string employeeCodes,
          DateTime startDate, DateTime endDate, string oDataQuery);

        Task<IEnumerable<Resource>> GetEmployeesBySearchString(string searchString, bool? addTrasfers = false);
        Task<IEnumerable<Resource>> GetEmployeesIncludingTerminatedBySearchString(string searchString, bool? addTrasfers = false);
        Task<IEnumerable<ResourceLoA>> GetLOAsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<LOA>> GetLOAsByEmployeeCode(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<Resource> GetEmployeeByEmployeeCode(string employeeCode);
        Task<List<Resource>> GetEmployeesByEmployeeCodes(string employeeCode);
        Task<IEnumerable<ResourceTimeInLevel>> GetTimeInLevelByEmployeeCode(string employeeCode);
        Task<IEnumerable<ResourceTransfer>> GetEmployeesPendingTransfersByEndDate(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ResourceTransfer>> GetEmployeeTransfersWithinDateRange(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<ResourceTransition>> GetTransitionsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<ResourceTransition> GetTransitionByEmployeeCode(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<ResourceTermination>> GetPendingTerminationsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<ResourceTermination> GetTerminationByEmployeeCode(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<ResourceTimeOff>> GetEmployeesTimeoffs(string employeeCodes, DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<ResourceTimeOff>> GetEmployeeTimeoffs(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);

        Task<IEnumerable<ServiceLine>> GetServiceLines();
        Task<IEnumerable<Office>> GetOffices();
        Task<IEnumerable<EmployeeCertificates>> GetCertificatesByEmployeeCodes(string employeeCodes);
        Task<IEnumerable<EmployeeLanguages>> GetLanguagesByEmployeeCodes(string employeeCodes);
    }
}
