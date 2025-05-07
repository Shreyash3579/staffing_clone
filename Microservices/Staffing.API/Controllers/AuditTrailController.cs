using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class AuditTrailController : ControllerBase
    {
        private readonly IAuditTrailService _service;

        public AuditTrailController(IAuditTrailService service)
        {
            _service = service;
        }


        /// <summary>
        ///     Get Audit trail for case or opportunity
        /// </summary>
        /// <param name="oldCaseCode">Case code to get audit trail for case</param>
        /// <param name="pipelineId">Id to get audit trail for opportunity</param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns>Audit history</returns>
        [HttpGet]
        [Route("auditCase")]
        public async Task<IActionResult> GetAuditTrailForCaseOrOpportunity(string oldCaseCode, string pipelineId, int? limit, int? offset)
        {
            var auditTrails = await _service.GetAuditTrailForCaseOrOpportunity(oldCaseCode, pipelineId, limit, offset);
            return Ok(auditTrails);
        }


        /// <summary>
        ///     Get Audit trail for an employee
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns>Audit history</returns>
        [HttpGet]
        [Route("auditEmployee")]
        public async Task<IActionResult> GetAuditTrailForEmployee(string employeeCode, int? limit, int? offset)
        {
            var auditTrails = await _service.GetAuditTrailForEmployee(employeeCode, limit, offset);
            return Ok(auditTrails);
        }
    }
}