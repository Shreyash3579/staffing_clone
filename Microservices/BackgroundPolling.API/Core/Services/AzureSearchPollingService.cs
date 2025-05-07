using BackgroundPolling.API.Contracts.Repository;
using BackgroundPolling.API.Contracts.Services;
using BackgroundPolling.API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Core.Services
{
    public class AzureSearchPollingService : IAzureSearchPollingService
    {
        public IStaffingAnalyticsRepository _staffingAnalyticsRepository;

        public AzureSearchPollingService(IStaffingAnalyticsRepository staffingAnalyticsRepository)
        {
            _staffingAnalyticsRepository = staffingAnalyticsRepository;
        }

        public async Task<string> UpsertEmployeeConsildatedDataForSearch(bool isFullLoad = false)
        {
           return await _staffingAnalyticsRepository.UpsertEmployeeConsildatedDataForSearch(isFullLoad);
        }

    }
}
