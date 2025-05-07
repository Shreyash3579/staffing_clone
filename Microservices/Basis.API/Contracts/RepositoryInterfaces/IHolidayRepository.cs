using Basis.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basis.API.Contracts.RepositoryInterfaces
{
    public interface IHolidayRepository
    {
        Task<IEnumerable<Holiday>> GetOfficeHolidaysByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<HolidayViewModel>> GetHolidays();
        Task<IEnumerable<Holiday>> GetOfficeHolidaysWithinDateRangeByEmployees(string employeeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<Holiday>> GetOfficeHolidaysWithinDateRangeByOffices(string officeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate);
    }
}
