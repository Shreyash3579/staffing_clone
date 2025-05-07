using Staffing.HttpAggregator.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IBvuCDApiClient
    {
        Task<IEnumerable<TrainingViewModel>> GetTrainingsWithinDateRangeByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate);
        Task<IEnumerable<TrainingViewModel>> GetTrainingsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
    }
}
