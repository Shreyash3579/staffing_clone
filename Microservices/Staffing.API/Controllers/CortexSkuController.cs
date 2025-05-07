using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CortexSkuController : ControllerBase
    {
        private readonly ICortexSkuService _cortexSKUService;

        public CortexSkuController(ICortexSkuService cortexSKUService)
        {
            _cortexSKUService = cortexSKUService;
        }

        /// <summary>
        ///     Get Cortex SKU Mappings with BOSS SKU
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCortexSkuMappings()
        {
            var commitmentTypes = await _cortexSKUService.GetCortexSkuMappings();
            return Ok(commitmentTypes);
        }

        /// <summary>
        ///     Update isPlaceholderCreatedFromCortex data
        /// </summary>
        [HttpPost]
        [Route("upsertPlaceholderCreatedForCortexPlaceholders")]
        public async Task<IActionResult> UpsertPlaceholderCreatedForCortexSKUs(dynamic payload)
        {
            var caseOppCortexTeamSize = payload != null
                ? JsonConvert.DeserializeObject<CaseOppCortexTeamSize>(payload.ToString())
                : new CaseOppCortexTeamSize();

            var result = await _cortexSKUService.UpsertPlaceholderCreatedForCortexSKUs(caseOppCortexTeamSize);
            return Ok(result);
        }

        /// <summary>
        ///      Get id placeholder created from cortex sku for an opp
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getOppCortexPlaceholderInfoByPipelineIds")]
        public async Task<IActionResult> GetOppCortexPlaceholderInfoByPipelineIds(dynamic payload)
        {
            var pipelineIds = !string.IsNullOrEmpty(payload["pipelineIds"]?.ToString()) ? payload["pipelineIds"].ToString() : null;
            var data = await _cortexSKUService.GetOppCortexPlaceholderInfoByPipelineIds(pipelineIds);
            return Ok(data);
        }

        /// <summary>
        ///     Update sku data from pricing configurator
        /// </summary>
        [HttpPost]
        [Route("upsertPricingSKU")]
        public async Task<IActionResult> UpsertPricingSKU(dynamic payload)
        {
            var caseOppTeamSize = payload != null
                ? JsonConvert.DeserializeObject<CaseOppCortexTeamSize>(payload.ToString())
                : null;

            var result = await _cortexSKUService.UpsertPricingSKU(caseOppTeamSize);
            return Ok(result);
        }

        /// <summary>
        ///     Update pricing sku data for logging purpose
        /// </summary>
        [HttpPost]
        [Route("upsertPricingSkuDataLog")]
        public async Task<IActionResult> UpsertPricingSkuDataLog(dynamic payload)
        {
            var pricingTeamSizeDataLogs = payload != null
                ? JsonConvert.DeserializeObject<IEnumerable<PricingSkuViewModel>>(payload.ToString())
                : null;

            var result = await _cortexSKUService.UpsertPricingSkuDataLog(pricingTeamSizeDataLogs);
            return Ok(result);
        }
    }
}
