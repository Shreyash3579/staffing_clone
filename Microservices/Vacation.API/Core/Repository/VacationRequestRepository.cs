using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vacation.API.Contracts.RepositoryInterfaces;
using Vacation.API.Core.Helpers;
using Vacation.API.Models;

namespace Vacation.API.Core.Repository
{
    public class VacationRequestRepository : IVacationRequestRepository
    {
        private readonly IBaseRepository<VacationRequest> _baseRepository;

        public VacationRequestRepository(IBaseRepository<VacationRequest> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<VacationRequest>> GetVacationRequestsByEmployee(string employeeCode, DateTime? effectiveFromDate, DateTime? effectiveToDate)
        {
            var vacations = await
                _baseRepository.GetAllAsync(new { employeeCode, effectiveFromDate, effectiveToDate },
                    StoredProcedureMap.GetVacationRequestsByEmployee);

            return vacations;
        }

        public async Task<IEnumerable<VacationRequest>> GetVacationRequests(DateTime? lastPolledDateTime)
        {
            var vacations = await
                _baseRepository.GetAllAsync(new { lastPolledDateTime },
                    StoredProcedureMap.GetVacationRequests);

            return vacations;
        }

        public async Task<IEnumerable<VacationRequest>> GetVacationsWithinDateRangeByEmployeeCodes(string employeeCodes, DateTime? startDate, DateTime? endDate)
        {
            var vacations = await
                _baseRepository.GetAllAsync(
                    new { employeeCodes, startDate, endDate },
                    StoredProcedureMap.GetVacationRequestsWithinDateRangeByEmployees);

            return vacations;
        }
    }
}
