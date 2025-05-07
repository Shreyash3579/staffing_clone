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
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private readonly IBaseRepository<string> _baseRepository;
        public AnalyticsRepository(IBaseRepository<string> baseRepository)
        {
            _baseRepository = baseRepository;
        }

        public async Task UpdateCostAndAvailabilityDataByScheduleId(string scheduleIds)
        {
            await _baseRepository.Context.Connection.QueryAsync(
               StoredProcedureMap.UpdateCostAndAvailabilityDataByScheduleId,
               new
               {
                   listScheduleIds = scheduleIds
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        //public async Task UpsertAvailabilityData(string employeeCodes)
        //{
        //    await _baseRepository.Context.Connection.QueryAsync(
        //        StoredProcedureMap.UpsertResourceAvailability,
        //        new
        //        {
        //            listEmployeeCodes = employeeCodes
        //        },
        //        commandType: CommandType.StoredProcedure,
        //        commandTimeout: _baseRepository.Context.TimeoutPeriod);
        //}

        public async Task UpsertAvailabilityDataBetweenDateRange(DataTable availabilityDateRangeForEmployeesTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpsertResourceAvailabilityBetweenDateRange,
                new
                {
                    tbAvailabilityEmployees =
                        availabilityDateRangeForEmployeesTable.AsTableValuedParameter(
                            "[dbo].[ResourceAvailabilityTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task UpdateAvailabilityDataForResourcesWithNoAvailabilityRecords(DataTable resourceAvailabilityTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpdateAvailabilityForResourcesWithNoAvailabilityRecords,
                new
                {
                    resourcesWithNoAvailabilityRecords =
                        resourceAvailabilityTable.AsTableValuedParameter(
                            "[dbo].[analyticsResourceAvailabilityTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        //TODO: Delete after 2019 data population or keep for future data population between date ranges
        public async Task UpdateAvailabilityDataForResourcesWithNoAvailabilityRecordsBetweenDateRange(DataTable resourceAvailabilityTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.UpdateAvailabilityForResourcesWithNoAvailabilityRecordsBetweenDateRange,
                new
                {
                    resourcesWithNoAvailabilityRecords =
                        resourceAvailabilityTable.AsTableValuedParameter(
                            "[dbo].[analyticsResourceAvailabilityTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<AnalyticsViewModel> GetResourcesAllocationAndAvailabilityByDateRange(DateTime? startDate, DateTime? endDate, DateTime? lastUpdatedFrom, DateTime? lastUpdatedTo, string action, string sourceTable, short pageNumber, int pageSize)
        {
            var result = await _baseRepository.Context.Connection.QueryMultipleAsync(
               StoredProcedureMap.GetResourcesAllocationAndAvailability,
               new
               {
                   startDate,
                   endDate,
                   lastUpdatedFrom,
                   lastUpdatedTo,
                   action,
                   sourceTable,
                   pageNumber,
                   pageSize
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);

            var analyticsData = result.Read<AnalyticsData>().ToList();
            var count = result.Read<int>().ToList().FirstOrDefault();

            var analyticsViewModel = new AnalyticsViewModel
            {
                totalCount = count,
                result = analyticsData
            };

            return analyticsViewModel;
        }

        public async Task UpdateAnalyticsDataForCommitments(DataTable resourcesCommitmentsDataTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
                StoredProcedureMap.updateAnalyticsDataForCommitments,
                new
                {
                    resourcesCommitments =
                        resourcesCommitmentsDataTable.AsTableValuedParameter(
                            "[dbo].[ResourceCommitmentTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 600);
        }

        public async Task<IEnumerable<Holiday>> GetHolidayWithinDateRangeByEmployees(string employeeCodes, DateTime? fromDate, DateTime? toDate)
        {
            var holidays = await _baseRepository.Context.Connection.QueryAsync<Holiday>(
                StoredProcedureMap.GetHolidayWithinDateRangeByEmployees,
                new
                {
                    employeeCodes = employeeCodes,
                    startDate = fromDate,
                    endDate = toDate
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return holidays;
        }

        public async Task UpsertCapacityAnalysisDaily(bool? fullLoad, DateTime? loadAfterLastUpdated)
        {
            await Task.Run(() => _baseRepository.Context.Connection.Query(
                 StoredProcedureMap.UpsertCapacityAnalysisDaily,
                 new { fullLoad, loadAfterLastUpdated },
                 commandType: CommandType.StoredProcedure,
                 commandTimeout: 144000));
        }

        public async Task UpdateCapacityAnalysisDailyForChangeInCaseAttribute(DateTime? updateAfterTimeStamp)
        {
            await Task.Run(() => _baseRepository.Context.Connection.Query(
                 StoredProcedureMap.UpdateCapacityAnalysisDailyForChangeInCaseAttribute,
                 new { updateAfterTimeStamp = updateAfterTimeStamp },
                 commandType: CommandType.StoredProcedure,
                 commandTimeout: 144000));
        }

        public async Task UpsertCapacityAnalysisMonthly(bool? fullLoad)
        {
            await Task.Run(() => _baseRepository.Context.Connection.Query(
                 StoredProcedureMap.UpsertCapacityAnalysisMonthly,
                 new { fullLoad },
                 commandType: CommandType.StoredProcedure,
                 commandTimeout: 144000));
        }

        public async Task<IEnumerable<CommitmentViewModel>> GetExternalCommitments(DateTime? updatedAfter)
        {
            var commitments = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<CommitmentViewModel>(
                 StoredProcedureMap.GetExternalCommitmentsMinStartMaxEndDate,
                 new { updatedAfter },
                 commandType: CommandType.StoredProcedure,
                 commandTimeout: _baseRepository.Context.TimeoutPeriod));

            return commitments;
        }

        public async Task<IEnumerable<Guid>> GetScheduleIdsIncorrectlyProcessedInAnalytics()
        {
            var scheduleIds = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<Guid>(
                 StoredProcedureMap.GetScheduleIdsIncorrectlyProcessedForAnalytics,
                 commandType: CommandType.StoredProcedure,
                 commandTimeout: _baseRepository.Context.TimeoutPeriod));

            return scheduleIds;
        }
    }
}
