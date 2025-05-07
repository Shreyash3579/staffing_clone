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
    public class OfficeClosureCasesController : ControllerBase
    {
        private readonly IOfficeClosureCasesService _service;
        public OfficeClosureCasesController(IOfficeClosureCasesService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("upsertOfficeClosureCases")]
        public async Task<IActionResult> UpsertOfficeClosureCases(OfficeClosureCases officeClosureCases)
        {
            var response = await _service.UpsertOfficeClosureCases(officeClosureCases);
            return Ok(response);
        }

        //commented on 06-jun-23 as it is not being used anymore
        ///// <summary>
        ///// Get comma separated List of pipeline Id signifies opportunities tagged due to resource having service line (AAG, ADAPT, FRWD) assigned on opportunity
        ///// </summary>
        ///// <remarks>
        ///// Sample Request:
        ///// {
        ///// "officeCodes": "110",
        ///// "caseTypeCodes": "1",
        ///// "startDate": "2023-06-01",
        ///// "endDate": "2023-06-030",
        ///// }
        ///// </remarks>
        ///// <param name="payload"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[Route("getOfficeClosureChangesWithinDateRangeByOfficeAndCaseType")]
        //public async Task<IActionResult> GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(dynamic payload)
        //{
        //    var officeCodes = payload["officeCodes"].ToString();
        //    var caseTypeCodes = payload["caseTypeCodes"].ToString();
        //    var startDate = !string.IsNullOrEmpty(payload["startDate"].ToString()) ? DateTime.Parse(payload["startDate"].ToString()) : null;
        //    var endDate = !string.IsNullOrEmpty(payload["endDate"].ToString()) ? DateTime.Parse(payload["endDate"].ToString()) : null;

        //    var officeClosureChanges =
        //        await _service.GetOfficeClosureChangesWithinDateRangeByOfficeAndCaseType(officeCodes, caseTypeCodes, startDate, endDate);
        //    return Ok(officeClosureChanges);
        //}
    }
}
