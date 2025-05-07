using BvuCD.API.Contracts.Repository;
using BvuCD.API.Contracts.Services;
using BvuCD.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BvuCD.API.Core.Services
{
    public class TrainingService : ITrainingService
    {
        private readonly ITrainingRepository _trainingRepository;

        public TrainingService(ITrainingRepository trainingRepository)
        {
            _trainingRepository = trainingRepository;
        }

        public async Task<IEnumerable<TrainingViewModel>> GetTrainingsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            if (string.IsNullOrEmpty(employeeCode))
                Enumerable.Empty<TrainingViewModel>();
            if (effectiveFromDate != null && effectiveToDate != null && effectiveFromDate > effectiveToDate)
                throw new ArgumentException("EffectiveToDate should be greater than EffectiveFromDate");

            var trainingsData = await
                _trainingRepository.GetTrainingsByEmployee(employeeCode, effectiveFromDate, effectiveToDate);

            var trainings = trainingsData.Select(item => new TrainingViewModel()
            {
                EmployeeCode = item.EmployeeCode,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Role = item.AttendeeRole,
                TrainingName = item.TrainingName,
                Type = "Training"
            }).OrderBy(r => r.StartDate);

            return trainings ?? Enumerable.Empty<TrainingViewModel>();
        }

        public async Task<IEnumerable<TrainingViewModel>> GetTrainings(DateTime? lastPolledDateTime)
        {
            var trainingsData = await
                _trainingRepository.GetTrainings(lastPolledDateTime);

            var trainings = trainingsData.Select(item => new TrainingViewModel()
            {
                Id = item.Id,
                EmployeeCode = item.EmployeeCode,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Role = item.AttendeeRole,
                TrainingName = item.TrainingName,
                Type = "Training",
                LastUpdated = item.LastUpdated,
                LastUpdatedBy = item.LastUpdatedBy
            }).OrderBy(r => r.StartDate);

            return trainings ?? Enumerable.Empty<TrainingViewModel>();
        }

        public async Task<IEnumerable<TrainingViewModel>> GetTrainingsWithinDateRangeByEmployeeCodes(string employeeCodes,
            DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(employeeCodes))
                return Enumerable.Empty<TrainingViewModel>();

            var trainingsData = await
                _trainingRepository.GetTrainingsWithinDateRangeByEmployeeCodes(employeeCodes, startDate, endDate);

            var trainings = trainingsData.Select(item => new TrainingViewModel()
            {
                EmployeeCode = item.EmployeeCode,
                StartDate = item.StartDate,
                EndDate = item.EndDate,
                Role = item.AttendeeRole,
                TrainingName = item.TrainingName,
                Type = "Training"

            }).OrderBy(r => r.StartDate);

            return trainings ?? Enumerable.Empty<TrainingViewModel>();
        }
    }
}
