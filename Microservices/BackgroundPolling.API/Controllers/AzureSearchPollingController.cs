using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AzureSearchPollingController : ControllerBase
    {
        public IAzureSearchPollingService _azureSearchPollingService;
        public AzureSearchPollingController(IAzureSearchPollingService azureSearchPollingService)
        {
            _azureSearchPollingService = azureSearchPollingService;
        }

        [HttpPost]
        public async Task<IActionResult> UpsertEmployeeConsildatedDataForSearch(bool isFullLoad = false)
        {
            var response = await _azureSearchPollingService.UpsertEmployeeConsildatedDataForSearch(isFullLoad);
            return Ok(response);
        }
    }
}
