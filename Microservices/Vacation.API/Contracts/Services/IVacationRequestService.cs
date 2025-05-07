using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vacation.API.ViewModels;

namespace Vacation.API.Contracts.Services
{
    public interface IVacationRequestService
    {
        Task<IEnumerable<VacationRequestViewModel>> GetVacationRequestsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<VacationRequestViewModel>> GetVacationRequests(DateTime? lastPolledDateTime);
        Task<IEnumerable<VacationRequestViewModel>> GetVacationsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
    }
}
