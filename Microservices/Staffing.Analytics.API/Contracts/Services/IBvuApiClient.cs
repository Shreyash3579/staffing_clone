using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IBvuApiClient
    {
        Task<IEnumerable<TrainingViewModel>> GetTrainingsWithinDateRangeByEmployeeCodes(string employeeCodes, 
            DateTime? startDate, DateTime? endDate);
    }
}
