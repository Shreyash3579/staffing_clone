using Microsoft.Extensions.Hosting;
using Staffing.AzureServiceBus.Contracts.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Core.Services
{
    public class ServiceBusHostedService : BackgroundService
    {
        private readonly IPegQueueService _pegQueueService;
        private readonly IPricingConfiguratorQueueService _pricingConfiguratorQueueService;

        public ServiceBusHostedService(IPegQueueService pegQueueService, IPricingConfiguratorQueueService pricingConfiguratorQueueService)
        {
            _pegQueueService = pegQueueService;
            _pricingConfiguratorQueueService = pricingConfiguratorQueueService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //start the Service Bus on Peg
            await _pegQueueService.SubscribeToPegQueue();
            await _pricingConfiguratorQueueService.SubscribeToPricingConfiguratorQueue();
        }

        public override  async Task StopAsync(CancellationToken cancellationToken)
        {
            //Stop the Service Bus on Peg
            await _pegQueueService.StopPegQueue();
            await _pricingConfiguratorQueueService.StopPricingConfiguratorQueue();
            
        }
    }
}
