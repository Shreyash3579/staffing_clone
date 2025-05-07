using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Core.Helpers;
using Dapper;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Repository
{
    public class IrisPollingRepository: IIrisPollingRepository
    {
        private readonly IBaseRepository<string> _baseRepository;
        public IrisPollingRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task UpsertWorkAndSchoolHistory(DataTable workHistoryDataTable, DataTable schoolHistoryDataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.UpsertWorkAndSchoolHistory,
                new
                {
                    workHistory =
                        workHistoryDataTable.AsTableValuedParameter(
                            "[iris].[employeeWorkHistoryTableType]"),
                    schoolHistory = schoolHistoryDataTable.AsTableValuedParameter(
                            "[iris].[employeeSchoolHistoryTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
    }
}
