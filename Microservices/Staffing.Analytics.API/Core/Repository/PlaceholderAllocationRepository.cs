using Dapper;
using Staffing.Analytics.API.Contracts.RepositoryInterfaces;
using Staffing.Analytics.API.Core.Helpers;
using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Core.Repository
{
    public class PlaceholderAllocationRepository : IPlaceholderAllocationRepository
    {
        private readonly IBaseRepository<string> _baseRepository;

        public PlaceholderAllocationRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task<IEnumerable<ResourceAllocation>> UpsertPlaceholderAnalyticsReportData(DataTable placeholderAllocations)
        {
            var allocationDateRangeBeforeUpsert = await _baseRepository.Context.Connection.QueryAsync<ResourceAllocation>(
                StoredProcedureMap.UpsertScheduleMasterPlaceholderDetail,
                new
                {
                    placeholderAllocations =
                        placeholderAllocations.AsTableValuedParameter(
                            "[dbo].[placeholderAllocationTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return allocationDateRangeBeforeUpsert;
        }

        public async Task<IEnumerable<string>> DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds(string scheduleMasterPlaceholderIds)
        {
            var deletedEmployeeCode =  await _baseRepository.Context.Connection.QueryAsync<string>(
                StoredProcedureMap.DeletePlaceholderAnalyticsDataByScheduleMasterPlaceholderIds,
                new
                {
                    scheduleMasterPlaceholderIds
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
            
            return deletedEmployeeCode;
        }


        public async Task<PlaceholderScheduleIdsViewModel> GetPlaceholderScheduleIdsIncorrectlyProcessedInAnalytics()
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
                StoredProcedureMap.GetPlaceholderScheduleIdsIncorrectlyProcessedForAnalytics,
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var scheduleIdsToDelete = result.Read<Guid>().ToList();
            var scheduleIdsToUpsert = result.Read<Guid>().ToList();

            var scheduleIds = ConvertToPlaceholderScheduleIdsModel(scheduleIdsToDelete, scheduleIdsToUpsert);
            return scheduleIds;
        }

        private PlaceholderScheduleIdsViewModel ConvertToPlaceholderScheduleIdsModel(List<Guid> scheduleIdsToDelete, List<Guid> scheduleIdsToUpsert)
        {
            var scheduleIds = new PlaceholderScheduleIdsViewModel
            {
                ScheduleIdsToDelete = scheduleIdsToDelete,
                ScheduleIdsToUpsert = scheduleIdsToUpsert
            };

            return scheduleIds;
        }

    }
}
