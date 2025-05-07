using BackgroundPolling.API.Models;
using BackgroundPolling.API.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Contracts.Services
{
    public interface IPipelineApiClient
    {
        Task<IEnumerable<OpportunityViewModel>> GetOpportunitiesByPipelineIds(string pipelineIdList);
        Task<IEnumerable<OpportunityFlatViewModel>> GetOpportunitiesFlatData(DateTime? lastUpdated);
    }
}
