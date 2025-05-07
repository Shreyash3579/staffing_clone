using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Contracts.Services
{
    public interface IPricingConfiguratorQueueService
    {
        Task SubscribeToPricingConfiguratorQueue();
        Task StopPricingConfiguratorQueue();
    }
}
