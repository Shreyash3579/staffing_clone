using BvuCD.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BvuCD.API.Contracts.Repository
{
    public interface ITrainingRepository
    {
        Task<IEnumerable<Training>> GetTrainingsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<Training>> GetTrainings(DateTime? lastPolledDateTime);
        Task<IEnumerable<Training>> GetTrainingsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
    }
}
