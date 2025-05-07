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
    public class ResourceAllocationRepository : IResourceAllocationRepository
    {
        private readonly IBaseRepository<ResourceAllocation> _baseRepository;

        public ResourceAllocationRepository(IBaseRepository<ResourceAllocation> baseRepository)
        {
            _baseRepository = baseRepository;
        }
        public async Task<IEnumerable<ResourceAllocation>> UpsertAnalyticsReportData(DataTable resourceAllocationDataTable)
        {
            var allocationDateRangeBeforeUpsert = await _baseRepository.Context.Connection.QueryAsync<ResourceAllocation>(
                StoredProcedureMap.UpsertAnalyticsReportData,
                new
                {
                    resourceAllocations =
                        resourceAllocationDataTable.AsTableValuedParameter(
                            "[dbo].[ResourceAllocationTableType]")
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return allocationDateRangeBeforeUpsert;
        }

        public async Task<string> DeleteAnalyticsDataByScheduleId(Guid scheduleId)
        {
            var deletedEmployeeCode = await _baseRepository.Context.Connection.QueryAsync<string>(
                StoredProcedureMap.DeleteAnalyticsDataByScheduleId,
                new
                {
                    scheduleId
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return deletedEmployeeCode.FirstOrDefault();
        }

        public async Task<IEnumerable<string>> DeleteAnalyticsDataByScheduleIds(string scheduleIds)
        {
            var deletedEmployeeCode = await _baseRepository.Context.Connection.QueryAsync<string>(
                StoredProcedureMap.DeleteAnalyticsDataByScheduleIds,
                new
                {
                    scheduleIds
                },
                commandType: CommandType.StoredProcedure,
                commandTimeout: _baseRepository.Context.TimeoutPeriod);

            return deletedEmployeeCode;
        }

        public async Task<IEnumerable<string>> GetResourcesWithNoAvailabilityRecords(string listEmployeeCodes)
        {
            var employeeCodes = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<string>(
                StoredProcedureMap.GetResourcesWithNoAvailabilityRecords,
                new { listEmployeeCodes },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return employeeCodes;
        }

        //TODO remove after 2019 data population or keep it for future data populations between date ranges
        public async Task<IEnumerable<string>> GetResourcesWithNoAvailabilityRecordsBetweenDateRange(string listEmployeeCodes)
        {
            var employeeCodes = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<string>(
                StoredProcedureMap.GetResourcesWithNoAvailabilityRecordsBetweenDateRange,
                new { listEmployeeCodes },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return employeeCodes;
        }

        public async Task<IEnumerable<string>> GetECodesWithPartialAvailabilityOnDate(DateTime lastDayForAvailability)
        {
            var eCodes = await Task.Run(() => _baseRepository.Context.Connection.QueryAsync<string>(
                StoredProcedureMap.GetECodesWithPartialAvailabilityOnDate,
                new { lastDayForAvailability },
                commandType: CommandType.StoredProcedure,
                commandTimeout: 180));

            return eCodes;
        }

        public async Task InsertAvailabilityTillNextYear(DataTable resourceTransactionsTable)
        {
            await _baseRepository.Context.Connection.QueryAsync(
               StoredProcedureMap.InsertAvailabilityTillNextYear,
               new
               {
                   availabilityRecords =
                       resourceTransactionsTable.AsTableValuedParameter(
                           "[dbo].[analyticsResourceAvailabilityTableType]")
               },
               commandType: CommandType.StoredProcedure,
               commandTimeout: _baseRepository.Context.TimeoutPeriod);
        }

        public async Task<IEnumerable<ResourceAllocation>> GetResourcesFullAvailabilityDateRange(string employeeCodes)
        {
            var result = await
                _baseRepository.GetAllAsync(new { employeeCodes },
                    StoredProcedureMap.GetResourcesFullAvailabilityDateRange);

            return result;
        }


    }
}
