using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vacation.API.Contracts.Services;
using Vacation.API.Models;

namespace Vacation.API.Controllers
{
    /// <summary>
    /// Employee Indexer Controller
    /// </summary>
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    public class EmployeeIndexerController : ControllerBase
    {
        private readonly IEmployeeIndexerService _service;
        public EmployeeIndexerController(IEmployeeIndexerService service)
        {
            _service = service;
        }

        /// <summary>
        /// Index data to the employee consolidated index
        /// </summary>
        /// <param name="dataToIndex"></param>
        /// <returns>summarized text</returns>
        [HttpPost("index")]
        public async Task<IActionResult> UploadDataToEmployeeConsolidatedIndex(IEnumerable<ResourcePartial> dataToIndex)
        {
            await _service.UploadDataToEmployeeConsolidatedIndex(dataToIndex);
            return Ok();

        }
    }
}
