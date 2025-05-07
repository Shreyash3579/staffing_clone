using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vacation.API.Models;

namespace Vacation.API.Contracts.RepositoryInterfaces
{
    public interface IVacationRequestRepository
    {
        Task<IEnumerable<VacationRequest>> GetVacationRequestsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<VacationRequest>> GetVacationRequests(DateTime? lastPolledDateTime);
        Task<IEnumerable<VacationRequest>> GetVacationsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
    }
}
