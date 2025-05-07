using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using System.Threading.Tasks;

namespace Staffing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSyncMismatchController : ControllerBase
    {
        private readonly IDataSyncMismatchService _dataSyncMismatchService;

        public DataSyncMismatchController(IDataSyncMismatchService dataSyncMismatchService)
        {
            _dataSyncMismatchService = dataSyncMismatchService;
        }

        [HttpGet("getCountforSyncTablesInStaffing")]
        public async Task<IActionResult> GetCountforSyncTablesInStaffing()
        {
            var syncTablesInfo = await _dataSyncMismatchService.GetCountforSyncTablesInStaffing();
            return Ok(syncTablesInfo);
        }
    }
}
