using Basis.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basis.API.Contracts.Services
{
    public interface IHolidayService
    {
        Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<HolidayViewModel>> GetHolidays();
        Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysWithinDateRangeByEmployees(string employeeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysWithinDateRangeByOffices(string officeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);

    }
}
