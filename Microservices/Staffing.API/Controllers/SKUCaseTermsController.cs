using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class SKUCaseTermsController : ControllerBase
    {
        private readonly ISKUCaseTermsService _skuTermService;

        public SKUCaseTermsController(ISKUCaseTermsService skuTermService)
        {
            _skuTermService = skuTermService;
        }

        /// <summary>
        /// Get all SKU Terms
        /// </summary>
        /// <returns>Saved Master Data for SKU Terms</returns>
        [HttpGet]
        [Route("getskutermlist")]
        public async Task<IActionResult> GetSKUTermList()
        {
            var skuTermList = await _skuTermService.GetSKUTermList();
            return Ok(skuTermList);
        }

        /// <summary>
        /// Get saved SKU Terms for a specific opportunity
        /// </summary>
        /// <param name="pipelineId"></param>
        /// <returns>Saved SKU Terms for a specific opportunity</returns>
        [HttpGet]
        [Route("getskutermsforopportunity")]
        public async Task<IActionResult> GetSKUTermsForOppotunity(Guid pipelineId)
        {
            var skuCaseTerms = await _skuTermService.GetSKUTermsForOpportunity(pipelineId);
            return Ok(skuCaseTerms);
        }

        /// <summary>
        /// Get saved SKU Terms for a specific case
        /// </summary>
        /// <param name="oldCaseCode"></param>
        /// <returns>Saved SKU Terms for a specific case</returns>
        [HttpGet]
        [Route("getskutermsforcase")]
        public async Task<IActionResult> GetSKUTermsForCase(string oldCaseCode)
        {
            var skuCaseTerms = await _skuTermService.GetSKUTermsForCase(oldCaseCode);
            return Ok(skuCaseTerms);
        }

        /// <summary>
        /// Used to save the SKU Terms for a specfic case
        /// </summary>
        /// <param name="payload">Json representing SKU Terms for a specic case</param>
        /// <sample>
        /// {
        ///   "skuTermsCodes": "1,2,3,4",
        ///   "oldCaseCode": "K7FC",
        ///   "effectiveDate": "2019-09-11T06:49:00",
        ///   "lastUpdatedBy": 45088
        /// }
        /// </sample>
        /// <returns>Return saved SKU Terms for the case</returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Route("insertskutermsforcase")]
        public async Task<IActionResult> InsertSKUCaseTerms(dynamic payload)
        {
            SKUCaseTerms skuCaseTerms = JsonConvert.DeserializeObject<SKUCaseTerms>(payload.ToString());
            var results = await _skuTermService.InsertSKUCaseTerms(skuCaseTerms);
            return Ok(results);
        }

        /// <summary>
        /// Used to update the SKU Terms for a specfic case
        /// </summary>
        /// <param name="payload"></param>
        /// <returns>Updated SKU Terms for the specific case</returns>
        [HttpPut]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [Route("updateskutermsforcase")]
        public async Task<IActionResult> UpdateSKUCaseTerms(dynamic payload)
        {
            SKUCaseTerms skuCaseTerms = JsonConvert.DeserializeObject<SKUCaseTerms>(payload.ToString());
            var results = await _skuTermService.UpdateSKUCaseTerms(skuCaseTerms);
            return Ok(results);
        }

        [HttpDelete]
        [Route("deleteskutermsforcase")]
        public async Task<IActionResult> DeleteSKUCaseTermsById(Guid Id)
        {
            await _skuTermService.DeleteSKUCaseTermsById(Id);
            return Ok();
        }

        /// <summary>
        /// This api end point is used to get the upcoming SKU Size Terms for any opportunity or case
        /// on the basis of its pipelineId or oldCaseCode and the startDate and endDate between which 
        /// this sku size must lie.
        /// </summary>
        /// <sample>
        /// {
        ///   "oldCaseCodes": "K7FC",
        ///   "pipelineIds": null,
        ///   "startDate": "2022-09-11T06:49:00",
        ///   "endDate": "2022-10-11T06:49:00"
        /// }
        /// </sample>
        /// <returns></returns>
        [HttpPost]
        [Route("getskutermsforcasesoropportunitiesforduration")]
        public async Task<IActionResult> GetSKUTermsForCasesOrOpportunitiesForDuration(dynamic payload)
        {
            var oldCaseCodes = string.IsNullOrEmpty($"{payload["oldCaseCodes"]}") ? null : $"{payload["oldCaseCodes"]}";
            var pipelineIds = string.IsNullOrEmpty($"{payload["pipelineIds"]}") ? null : $"{payload["pipelineIds"]}";
            var startDate = Convert.ToDateTime(payload["startDate"]);
            var endDate = Convert.ToDateTime(payload["endDate"]);

            var skuCaseTerms = await _skuTermService.GetSKUTermsForCaseOrOpportunityForDuration(oldCaseCodes, pipelineIds, startDate, endDate);
            return Ok(skuCaseTerms);
        }


    }
}