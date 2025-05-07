using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Core.Helpers;
using Basis.API.Models;
using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Core.Repository
{
    public class HolidayRepository : IHolidayRepository
    {
        private readonly IBaseRepository<Holiday> _baseRepository;

        public HolidayRepository(IBaseRepository<Holiday> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<Holiday>> GetOfficeHolidaysByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var officeHolidays = await _baseRepository.GetAllAsync(new { employeeCode, effectiveFromDate, effectiveToDate },
                    StoredProcedureMap.GetOfficeHolidaysByEmployee);

            return officeHolidays;
        }

        public async Task<IEnumerable<HolidayViewModel>> GetHolidays()
        {
            var holidays = await Task.Run(() => _baseRepository.Context.Connection.Query<HolidayViewModel>(
                StoredProcedureMap.GetHolidays,
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180).ToList());

            return holidays;
        }

        public async Task<IEnumerable<Holiday>> GetOfficeHolidaysWithinDateRangeByEmployees(string employeeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var employeesOfficeHolidays = await _baseRepository.GetAllAsync(new { employeeCodes, effectiveFromDate, effectiveToDate },
                    StoredProcedureMap.GetOfficeHolidaysWithinDateRangeByEmployees);

            return employeesOfficeHolidays;
        }

        public async Task<IEnumerable<Holiday>> GetOfficeHolidaysWithinDateRangeByOffices(string officeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var officeHolidays = await _baseRepository.GetAllAsync(new { officeCodes, effectiveFromDate, effectiveToDate },
                    StoredProcedureMap.GetOfficeHolidaysWithinDateRangeByOffices);

            return officeHolidays;
        }
    }
}
