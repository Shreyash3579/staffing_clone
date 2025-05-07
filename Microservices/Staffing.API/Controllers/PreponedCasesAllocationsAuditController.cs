using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreponedCasesAllocationsAuditController : ControllerBase
    {
        private readonly IPreponedCasesAllocationsAuditService _preponedCasesAllocationsAuditService;

        public PreponedCasesAllocationsAuditController(IPreponedCasesAllocationsAuditService preponedCasesAllocationsAuditService)
        {
            _preponedCasesAllocationsAuditService = preponedCasesAllocationsAuditService;
        }

        /// <summary>
        ///      Upsert Preponed Cases Allocations for audit purposes when CCM prepones a case
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertPreponedCaseAllocationsAudit")]
        public async Task<IActionResult> UpsertPreponedCaseAllocationsAudit(dynamic payload)
        {
            IEnumerable<PreponedCasesAllocationsAudit> auditData = JsonConvert.DeserializeObject<PreponedCasesAllocationsAudit[]>(payload.ToString());

            var data = await _preponedCasesAllocationsAuditService.UpsertPreponedCaseAllocationsAudit(auditData);
            return Ok(data);
        }

        /// <summary>
        ///      Gets all cases that have been preponed from CCM for offices and selected service lines 
        ///      Return past two weeks by default or can use date range
        /// </summary>
        /// <remarks>
        /// Sample Request:
        /// {
        ///     "startDate": "2023-05-25",
        ///     "endDate": "2023-06-15",
        ///     "serviceLineCodes": "SL0001",
        ///     "officeCodes": "110"
        /// }
        /// </remarks>
        /// <param name="payload"></param>
        /// <returns>Case audit details like which case was updated, who made the change and when and the impacted employees</returns>
        [HttpPost]
        public async Task<IActionResult> GetPreponedCaseAllocationsAudit(dynamic payload)
        {
            var startDate = !string.IsNullOrEmpty(payload["startDate"]?.ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
            var endDate = !string.IsNullOrEmpty(payload["endDate"]?.ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;
            var serviceLineCodes = !string.IsNullOrEmpty(payload["serviceLineCodes"]?.ToString()) ? payload["serviceLineCodes"].ToString() : null;
            var officeCodes = !string.IsNullOrEmpty(payload["officeCodes"]?.ToString()) ? payload["officeCodes"].ToString() : null;

            var data = await _preponedCasesAllocationsAuditService.GetPreponedCaseAllocationsAudit(serviceLineCodes, officeCodes, startDate, endDate);
            return Ok(data);
        }
    }
}
