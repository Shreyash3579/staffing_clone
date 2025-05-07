using Staffing.AzureServiceBus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Contracts.Services
{
    public interface IPipelineApiClient
    {
        Task<Opportunity> GetOpportunityByCortexId(string cortexOpportunityId);
    }
}
