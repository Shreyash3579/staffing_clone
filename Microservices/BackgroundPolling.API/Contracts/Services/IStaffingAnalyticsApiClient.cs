using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IStaffingAnalyticsApiClient
    {
        Task<IEnumerable<ResourceAllocation>> UpdateCostForResourcesAvailableInFullCapacity(string listEmployeeCodes);
        Task<List<ResourceAvailabilityViewModel>> InsertDailyAvailabilityTillNextYearForAll(string employeeCodes);
        Task UpsertAvailabilityData(string listEmployeeCodes); 
        Task UpsertCapacityAnalysisDaily(bool? fullLoad, DateTime? loadAfterLastUpdated);
        Task UpsertCapacityAnalysisMonthly(bool? fullLoad);
        Task UpdateAnlayticsDataForUpsertedCommitment(string commitmentIds);
    }
}
