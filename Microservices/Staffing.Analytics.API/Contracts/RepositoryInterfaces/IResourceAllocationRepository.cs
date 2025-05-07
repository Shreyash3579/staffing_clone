using Staffing.Analytics.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.RepositoryInterfaces
{
    public interface IResourceAllocationRepository
    {
        Task<IEnumerable<ResourceAllocation>> UpsertAnalyticsReportData(DataTable resourceAllocationDataTable);
        Task<string> DeleteAnalyticsDataByScheduleId(Guid id);
        Task<IEnumerable<string>> DeleteAnalyticsDataByScheduleIds(string ids);
        Task<IEnumerable<string>> GetResourcesWithNoAvailabilityRecords(string listEmployeeCodes);
        //TODO: delete after 2019 data population or keep it for future data populations between date ranges
        Task<IEnumerable<string>> GetResourcesWithNoAvailabilityRecordsBetweenDateRange(string listEmployeeCodes);
        Task<IEnumerable<string>> GetECodesWithPartialAvailabilityOnDate(DateTime lastDayForAvailability);
        Task<IEnumerable<ResourceAllocation>> GetResourcesFullAvailabilityDateRange(string employeeCodes);
        Task InsertAvailabilityTillNextYear(DataTable resourceAvailabilityTable);

    }
}
