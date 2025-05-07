using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.HttpAggregator.Contracts.Services;
using System;
using System.Threading.Tasks;

namespace Staffing.HttpAggregator.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RevenueController : ControllerBase
    {
        private readonly IRevenueService _revenueService;

        public RevenueController(IRevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        //commented on 15-jun-23 as it is not being used anymore
        ///// <summary>
        ///// Get Case Revenue by client code and case code
        ///// </summary>
        ///// <param clientCode="string"></param>
        ///// <param caseCode="string"></param>
        ///// <returns></returns>
        //[HttpPost("getRevenueByClientCodeAndCaseCode")]
        //public async Task<IActionResult> GetRevenueByClientCodeAndCaseCode(dynamic payload)
        //{
        //    int? clientCode = payload["clientCode"];
        //    int? caseCode = payload["caseCode"];
        //    string currency = payload["currency"] != null ? Convert.ToString(payload["currency"]) : "US";
        //    var pipelineId = payload["pipelineId"] != null ? Convert.ToString(payload["pipelineId"]) : string.Empty;
        //    var revenueData = await _revenueService.GetRevenueByCaseCodeAndClientCode(clientCode, caseCode, pipelineId, currency);
        //    return Ok(revenueData);
        //}
    }
}