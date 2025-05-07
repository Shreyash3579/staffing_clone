using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IResourceApiClient
    {
        #region Employee
        Task<List<Resource>> GetEmployees();
        Task<List<Resource>> GetNotYetStartedEmployees();
        Task<List<Resource>> GetEmployeesIncludingTerminated();
        Task<List<Resource>> GetTerminatedEmployees();
        Task<Dictionary<string, string>> GetEmployeeIdTypeMaps();
        #endregion

        #region Employee Transactions
        Task<IEnumerable<ResourceTransaction>> GetFutureTransitions();
        Task<IEnumerable<ResourceTransaction>> GetFuturePromotions();
        Task<IEnumerable<ResourceTransaction>> GetFutureTransfers();
        Task<IEnumerable<EmployeeTransaction>> GetEmployeesStaffingTransactions(string employeeCodes);
        Task<IEnumerable<ResourceTransaction>> GetFutureTerminations();
        #endregion

        #region Employee LOA
        Task<IEnumerable<ResourceLOA>> GetFutureLOAs();
        Task<IEnumerable<ResourceLOA>> GetPendingLOATransactions();
        Task<IEnumerable<ResourceLOA>> GetLOAsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime startDate, DateTime endDate);
        Task<IEnumerable<ResourceLOA>> GetEmployeesLoATransactions(string listEmployeeCodes);
        Task<IEnumerable<EmployeeLoATransaction>> GetWDEmployeesLoATransactions(string listEmployeeCodes);
        #endregion

        #region Lookup

        Task<IEnumerable<ServiceLine>> GetServiceLines();
        Task<IEnumerable<PDGrade>> GetPDGrades();
        #endregion

        #region TimeOff
        Task<IEnumerable<ResourceTimeOff>> GetEmployeesTimeoffs(string employeeCodes, DateTime? startDate, DateTime? endDate);
        #endregion

        #region Employees Certifications
        Task<IEnumerable<EmployeeCertificates>> GetCertificatesByEmployeeCodes(string employeeCodes);
        #endregion

        #region Employees Languages
        Task<IEnumerable<EmployeeLanguages>> GetLanguagesByEmployeeCodes(string listEmployeeCodes);
        #endregion
    }
}
