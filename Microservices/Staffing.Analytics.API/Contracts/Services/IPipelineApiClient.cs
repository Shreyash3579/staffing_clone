using Staffing.Analytics.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.Analytics.API.Contracts.Services
{
    public interface IPipelineApiClient
    {
        Task<IEnumerable<OpportunityDetailsViewModel>> GetOpportunityDetailsByPipelineIds(string pipelineIds);
    }
}
