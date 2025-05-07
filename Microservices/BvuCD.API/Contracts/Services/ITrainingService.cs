using BvuCD.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BvuCD.API.Contracts.Services
{
    public interface ITrainingService
    {
        Task<IEnumerable<TrainingViewModel>> GetTrainingsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate);
        Task<IEnumerable<TrainingViewModel>> GetTrainings(DateTime? lastPolledDateTime);
        Task<IEnumerable<TrainingViewModel>> GetTrainingsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate);
    }
}
