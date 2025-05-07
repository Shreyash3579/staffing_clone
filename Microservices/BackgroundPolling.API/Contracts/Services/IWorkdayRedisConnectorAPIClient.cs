using BackgroundPolling.API.Models;
using BackgroundPolling.API.Models.Workday;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IWorkdayRedisConnectorAPIClient
    {
        Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactionsByModifiedDate(DateTime date);
        Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactionsByEfectiveDate(DateTime date);
        Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactions(string employeeCodes);
        Task<IEnumerable<LOATransaction>> GetEmployeesLOATransactionsPendingFromRedis();
        Task<IEnumerable<EmployeeTransaction>> GetEmployeesStaffingTransactionsPendingFromRedis();
    }
}
