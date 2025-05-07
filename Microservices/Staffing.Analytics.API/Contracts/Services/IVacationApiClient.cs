using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IVacationApiClient
    {
        Task<IEnumerable<VacationRequestViewModel>> GetVacationsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
    }
}
