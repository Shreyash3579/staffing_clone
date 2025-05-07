using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class SkuController : ControllerBase
    {
        private readonly ISkuService _skuService;
        public SkuController(ISkuService skuService)
        {
            _skuService = skuService;
        }

        /// <summary>
        /// Get saved sku term for a case or opp or planning card
        /// </summary>
        /// <param name="payload">Pass comma separated list of oldCaseCodes, pipelineIds and planningCardsIds</param>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "oldCaseCodes":"Z4RJ,S3XF,G8MD",
        ///       "pipelineIds":"646E2606-FF10-492D-BE0A-1187AAC90122,98F69747-6902-4F7A-A944-7A30C8B4C7B0,AEF80B09-B72E-4BED-BB5D-2C61B123AB61",
        ///       "planningCardIds": ""
        ///    }
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [Route("getSkuForProjects")]
        public async Task<IActionResult> GetSkuForProject(dynamic payload)
        {
            var oldCaseCodes = payload["oldCaseCodes"].ToString();
            var pipelineIds = string.IsNullOrEmpty(payload["pipelineIds"].ToString())
                ? null
                : payload["pipelineIds"].ToString();
            var planningCardIds = string.IsNullOrEmpty(payload["planningCardIds"].ToString())
                ? null
                : payload["planningCardIds"].ToString();
            
            var skuTermList = await _skuService.GetSkuForProjects(oldCaseCodes, pipelineIds, planningCardIds);
            return Ok(skuTermList);
        }

        /// <summary>
        /// Upsert saved sku term for a case or opp or planning card
        /// </summary>
        /// <param name="payload">Sku model object</param>
        /// <remarks>
        /// Sample Request:
        /// {
        ///    "id": null,
        ///    "skuTerm": "1M+2C+2AC",
        ///    "oldCaseCode": "G8MD"
        ///    "pipelineId": null,
        ///    "lastUpdatedBy": "45088"
        /// }
        /// </remarks>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertSkuForProjects")]
        public async Task<IActionResult> UpsertSkuForProject(dynamic payload)
        {
            Sku skus = JsonConvert.DeserializeObject<Sku[]>(payload.ToString());
            
            var skuTermList = await _skuService.UpsertSkuForProject(skus);
            return Ok(skuTermList);
        }
    }
}
