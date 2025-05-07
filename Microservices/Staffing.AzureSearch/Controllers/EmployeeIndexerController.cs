using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.AzureSearch.Contracts.Services;
using Staffing.AzureSearch.Models;

namespace Staffing.AzureSearch.Controllers
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

        /// <summary>
        /// Index data to the employee consolidated index
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>summarized text</returns>
        [HttpGet("indexNotes")]
        public async Task<IActionResult> IndexResourceNotesByLastUpdatedDate(DateTime dateTime)
        {
            var notesUploadedSuccessString =  await _service.IndexResourceNotesByLastUpdatedDate(dateTime);
            return Ok(notesUploadedSuccessString);

        }
    }
}
