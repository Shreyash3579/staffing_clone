using Microsoft.AspNetCore.Mvc;
using Staffing.AzureServiceBus.Contracts.Services;
using Staffing.AzureServiceBus.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricingConfiguratorQueueController : ControllerBase
    {
        private readonly IPricingConfiguratorQueueService _pricingConfiguratorQueueService;

        public PricingConfiguratorQueueController(IPricingConfiguratorQueueService pricingConfiguratorQueueService)
        {
            _pricingConfiguratorQueueService = pricingConfiguratorQueueService;
        }

        /// <summary>
        /// START the subsciption on the Azure Service Bus Pricing Configurator Queue
        /// </summary>
        /// <returns></returns>
        [HttpGet("subscribeToPricingConfiguratorQueue")]
        public async Task<IActionResult> SubscribeToPricingConfiguratorQueue()
        {
            await _pricingConfiguratorQueueService.SubscribeToPricingConfiguratorQueue();

            return Ok("Pricing Configurator Queue Subscription Started");
        }

        /// <summary>
        /// Stop the subsciption on the Azure Service Bus Pricing Configurator Queue
        /// </summary>
        /// <returns></returns>
        [HttpGet("stopPricingConfiguratorQueue")]
        public async Task<IActionResult> StopPegQueue()
        {
            await _pricingConfiguratorQueueService.StopPricingConfiguratorQueue();

            return Ok("Pricing Configurator Queue Subscription Stopped");
        }

    }
}



