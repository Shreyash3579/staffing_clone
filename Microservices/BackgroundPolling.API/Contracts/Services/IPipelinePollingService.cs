using BackgroundPolling.API.Core.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IPipelinePollingService
    {
        [SkipWhenPreviousJobIsRunning]
        public Task<IEnumerable<Guid?>> UpdateOpportunityEndDateFromPipeline();

        [SkipWhenPreviousJobIsRunning]
        Task UpsertOpportunitiesFlatDataFromPipeline(bool isFullLoad, DateTime? lastUpdated);
    }
}
