using Staffing.AzureServiceBus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Contracts.Services
{
    public interface IPegQueueService
    {
       Task SubscribeToPegQueue();
       Task StopPegQueue();
        Task<bool> SendToPegQueue(IEnumerable<PegOpportunityMap> convertedPlanningCards);
    }
}