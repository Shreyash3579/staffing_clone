using Staffing.HttpAggregator.Models;
using Staffing.HttpAggregator.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IBasisApiClient
    {
        Task<IEnumerable<EmployeePracticeArea>> GetPracticeAreaAffiliationsByEmployeeCodes(string employeeCodes, string practiceAreaCodes, string affiliationRoleCodes);

        //Holiday Controller
        Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysWithinDateRangeByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<HolidayViewModel>> GetOfficeHolidaysByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<EmployeePracticeAreaViewModel>> GetAffiliationsByEmployeeCodesAndPracticeAreaCodes(string employeeCode);
    }
}
