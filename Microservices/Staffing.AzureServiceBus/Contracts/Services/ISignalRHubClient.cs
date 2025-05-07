using Staffing.AzureServiceBus.Models;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Contracts.Services
{
    public interface ISignalRHubClient
    {
        Task<string> UpdateSignalRHub(PegOpportunity pegData);
    }
}
