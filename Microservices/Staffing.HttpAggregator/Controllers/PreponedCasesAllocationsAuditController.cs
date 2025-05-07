using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.HttpAggregator.Contracts.Services;
using System.Threading.Tasks;
using System;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PreponedCasesAllocationsAuditController : ControllerBase
    {
        private readonly IPreponedCasesAllocationsAuditService _preponedCasesAllocationsAuditService;

        public PreponedCasesAllocationsAuditController(IPreponedCasesAllocationsAuditService preponedCasesAllocationsAuditService)
        {
            _preponedCasesAllocationsAuditService = preponedCasesAllocationsAuditService;
        }

        /// <summary>
        ///      Get all preponed cases and impacted allocations on them between date range and for selected staffing tags.
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        /// 
        /// {
        ///     "startDate":"2023-06-01T15:41:18.009Z",
        ///     "endDate":"2023-06-15T15:41:18.009Z",
        ///     "serviceLineCodes":"P",
        ///     "officeCodes":"210,512,215,536"
        /// }
        /// </remarks>
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
