using BvuCD.API.Contracts.Repository;
using BvuCD.API.Core.Helpers;
using BvuCD.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BvuCD.API.Core.Repository
{
    public class TrainingRepository : ITrainingRepository
    {
        private readonly IBaseRepository<Training> _baseRepository;

        public TrainingRepository(IBaseRepository<Training> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<Training>> GetTrainingsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var trainings = await
                _baseRepository.GetAllAsync(new { employeeCode, effectiveFromDate, effectiveToDate },
                    StoredProcedureMap.GetTrainingsByEmployee);

            return trainings;
        }

        public async Task<IEnumerable<Training>> GetTrainings(DateTime? lastPolledDateTime)
        {
            var trainings = await
                _baseRepository.GetAllAsync(new { lastPolledDateTime },
                    StoredProcedureMap.GetTrainings);

            return trainings;
        }

        public async Task<IEnumerable<Training>> GetTrainingsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            var trainings = await
                _baseRepository.GetAllAsync(
                    new { employeeCodes, startDate, endDate },
                    StoredProcedureMap.GetTrainingsWithinDateRangeByEmployees
                 );

            return trainings;
        }
    }
}
