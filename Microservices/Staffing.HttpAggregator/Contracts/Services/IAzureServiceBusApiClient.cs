using Staffing.HttpAggregator.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Contracts.Services
{
    public interface IAzureServiceBusApiClient
    {
        Task<bool> SendToPegQueue(IEnumerable<PegOpportunityMap> pegOpportunityMap);
    }
}
