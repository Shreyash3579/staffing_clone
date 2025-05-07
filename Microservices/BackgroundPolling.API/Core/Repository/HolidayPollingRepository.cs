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
    public class HolidayPollingRepository: IHolidayPollingRepository
    {
        private readonly IBaseRepository<Holiday> _baseRepository;
        public HolidayPollingRepository(IBaseRepository<Holiday> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task InsertHolidays(DataTable holidayDataTable)
        {
            await _baseRepository.Context.AnalyticsConnection.QueryAsync(
                StoredProcedureMap.InsertOfficeHolidays,
                new
                {
                    holidays =
                        holidayDataTable.AsTableValuedParameter(
                            "[basis].[holidayTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }
    }
}
