using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsAuditController : ControllerBase
    {
        private readonly IAnalyticsAuditService _analyticsAuditService;
        public AnalyticsAuditController(IAnalyticsAuditService analyticsAuditService)
        {
            _analyticsAuditService = analyticsAuditService;
        }

        /// <summary>
        /// Gets all timestamps from SMD and RA that are not synced with CAD
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("analyticsRecordsNotSyncedWithCAD")]
        public async Task<IActionResult> GetAnalyticsRecordsNotSyncedWithCAD()
        {
            var securityUsers = await _analyticsAuditService.GetAnalyticsRecordsNotSyncedWithCAD();
            return Ok(securityUsers);
        }
    }
}
