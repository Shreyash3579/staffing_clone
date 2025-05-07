using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using System;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class TaggedCaseController : ControllerBase
    {
        private readonly ITaggedCaseService _taggedCaseService;

        public TaggedCaseController(ITaggedCaseService taggedCaseService)
        {
            _taggedCaseService = taggedCaseService;
        }


        /// <summary>
        /// Get comma separated List of Old Cases codes signifies cases tagged due to resource having service line (AAG, ADAPT, FRWD) assigned on case
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        /// "oldCaseCodes": "H5AC,W8VZ,D6GR",
        /// "serviceLineNames": "AAG,ADAPT,FRWD"
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("casesByResourceServiceLines")]
        public async Task<IActionResult> GetCasesByResourceServiceLines(dynamic payload)
        {
            var oldCaseCodes = $"{payload["oldCaseCodes"]}";
            var serviceLineNames = $"{payload["serviceLineNames"]}";
            var taggedCases = await _taggedCaseService.GetCasesByResourceServiceLines(oldCaseCodes, serviceLineNames);
            return Ok(taggedCases);
        }

        /// <summary>
        /// Get comma separated List of pipeline Id signifies opportunities tagged due to resource having service line (AAG, ADAPT, FRWD) assigned on opportunity
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        /// "pipelineIds": "6792E643-4BAA-4BAB-9021-24370F206B6F,C1F0D5D5-34C4-4EC6-AC36-5A16A2C0C219",
        /// "serviceLineNames": "AAG,ADAPT,FRWD"
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("opportunitiesByResourceServiceLines")]
        public async Task<IActionResult> GetOpportunitiesByResourceServiceLines(dynamic payload)
        {
            var pipelineIds = $"{payload["pipelineIds"]}";
            var serviceLineNames = $"{payload["serviceLineNames"]}";
            var taggedOpportunities =
                await _taggedCaseService.GetOpportunitiesByResourceServiceLines(pipelineIds, serviceLineNames);
            return Ok(taggedOpportunities);
        }
    }
}