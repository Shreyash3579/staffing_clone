using BackgroundPolling.API.Contracts.Helpers;
using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using BackgroundPolling.API.Models;
using Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class VacationPollingRepository : IVacationPollingRepository
    {
        private readonly IBaseRepository<Vacation> _baseRepository;
        public VacationPollingRepository(IBaseRepository<Vacation> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task UpsertVacations(DataTable vacationDataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpsertVacations,
                new
                {
                    vacations =
                        vacationDataTable.AsTableValuedParameter(
                            "[vacation].[vacationRequestMasterTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
    }
}
