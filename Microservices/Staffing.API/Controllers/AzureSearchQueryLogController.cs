using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureSearchQueryLogController : ControllerBase
    {
        private readonly IAzureSearchQueryLogService _azureSearchQueryLogService;

        public AzureSearchQueryLogController(IAzureSearchQueryLogService azureSearchQueryLogService)
        {
            _azureSearchQueryLogService = azureSearchQueryLogService;
        }

        /// <summary>
        /// Update Query log from the azure search query
        /// </summary>
        /// <param name="azureSearchQueryLog"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sample Request:
        ///    {
        ///       "oldCaseCodes":"Q6BK",
        ///       "employeeCodes":"39980",
        ///       "lastUpdated":"2020-04-09",
        ///       "startDate":"2020-04-01",
        ///       "endDate":"2020-04-17",
        ///       "caseRoleCodes": null
        ///    }
        /// </remarks>
        [HttpPut()]
        public async Task<IActionResult> InsertAzureSearchQueryLog(AzureSearchQueryLog azureSearchQueryLog)
        {
            await _azureSearchQueryLogService.InsertAzureSearchQueryLog(azureSearchQueryLog);
            return Ok("Azure Search Query Logged Successfully");
        }
    }
}
