using Microsoft.AspNetCore.Mvc;
using Staffing.AzureServiceBus.Contracts.Services;
using Staffing.AzureServiceBus.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.AzureServiceBus.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PegQueueController : ControllerBase
    {
        private readonly IPegQueueService _pegQueueService;

        public PegQueueController(IPegQueueService pegQueueService)
        {
            _pegQueueService = pegQueueService;
        }

        /// <summary>
        /// START the subsciption on the Azure Service Bus PEG Queue
        /// </summary>
        /// <returns></returns>
        [HttpGet("subscribeToPegQueue")]
        public async Task<IActionResult> SubscribeToPegQueue()
        {
            await _pegQueueService.SubscribeToPegQueue();

            return Ok("Peg Queue Subscription Started");
        }

        /// <summary>
        /// Sned the case code on which Planning Cards are merged to PEG Queue
        /// </summary>
        /// <returns>true or false if addition to queue fails or succeeds</returns>
        [HttpPost("sendToPegQueue")]
        public async Task<IActionResult> SendToPegQueue(IEnumerable<PegOpportunityMap> convertedPlanningCards)
        {
            var isSuccessfullySentToQueue = await _pegQueueService.SendToPegQueue(convertedPlanningCards);

            return Ok(isSuccessfullySentToQueue);
        }

        /// <summary>
        /// Stop the subsciption on the Azure Service Bus PEG Queue
        /// </summary>
        /// <returns></returns>
        [HttpGet("stopPegQueue")]
        public async Task<IActionResult> StopPegQueue()
        {
            await _pegQueueService.StopPegQueue();

            return Ok("Peg Queue Subscription Stopped");
        }
    }
}