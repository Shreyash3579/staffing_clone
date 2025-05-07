using Basis.API.Contracts.RepositoryInterfaces;
using Basis.API.Contracts.Services;
using Basis.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basis.API.Core.Services
{
    public class HolidayService : IHolidayService
    {
        private readonly IHolidayRepository _holidayRepository;

        public HolidayService(IHolidayRepository holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        public async Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                throw new ArgumentException("Employee Code can not be null");
            if (effectiveFromDate != null && effectiveToDate != null && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("EffectiveToDate should be greater than EffectiveFromDate");

            var officeHolidaysData = await
                _holidayRepository.GetOfficeHolidaysByEmployee(employeeCode, effectiveFromDate, effectiveToDate);

            var officeHolidays = officeHolidaysData?.Select(item => new HolidayViewModel
            {
                StartDate = item.HolidayDate,
                EndDate = item.HolidayDate,
                Description = item.Description,
                Type = "Holiday"
            });

            return officeHolidays ?? Enumerable.Empty<HolidayViewModel>();
        }

        public async Task<IEnumerable<HolidayViewModel>> GetHolidays()
        {
            var holidays = await
                _holidayRepository.GetHolidays();

            return holidays ?? Enumerable.Empty<HolidayViewModel>();
        }

        public async Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysWithinDateRangeByEmployees(string employeeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                throw new ArgumentException("Employee Codes can not be null");
            if (effectiveFromDate != null && effectiveToDate != null && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("EffectiveToDate should be greater than EffectiveFromDate");

            var officeHolidaysData = await
                _holidayRepository.GetOfficeHolidaysWithinDateRangeByEmployees(employeeCodes, effectiveFromDate, effectiveToDate);

            var officeHolidays = officeHolidaysData?.Select(item => new HolidayViewModel
            {
                EmployeeCode = item.EmployeeCode,
                StartDate = item.HolidayDate,
                EndDate = item.HolidayDate,
                Description = item.Description,
                Type = "Holiday"
            });

            return officeHolidays ?? Enumerable.Empty<HolidayViewModel>();
        }
        public async Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysWithinDateRangeByOffices(string officeCodes, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(officeCodes))
                throw new ArgumentException("Office Code(s) can not be null");
            if (effectiveFromDate != null && effectiveToDate != null && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("EffectiveToDate should be greater than EffectiveFromDate");

            var officeHolidaysData = await
                _holidayRepository.GetOfficeHolidaysWithinDateRangeByOffices(officeCodes, effectiveFromDate, effectiveToDate);

            var officeHolidays = officeHolidaysData?.Select(item => new HolidayViewModel
            {
                OfficeCode = item.OfficeCode,
                StartDate = item.HolidayDate,
                EndDate = item.HolidayDate,
                Description = item.Description,
                Type = "Holiday"
            });

            return officeHolidays ?? Enumerable.Empty<HolidayViewModel>();
        }
    }
}
