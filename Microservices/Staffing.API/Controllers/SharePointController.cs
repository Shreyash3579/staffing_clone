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
    public class SharePointController : ControllerBase
    {
        private readonly ISharePointService _sharePointService;

        public SharePointController(ISharePointService sharePointService)
        {
            _sharePointService = sharePointService;
        }

        ///// <summary>
        /////   Gets the Missions ceated for employees in Sharepoint Site
        ///// </summary>
        ///// <param name="employeeCodes">Comma separated list of employee codes that have missions created for them</param>
        ///// <returns></returns>
        [HttpGet]
        [Route("getSmapMissionNotesByEmployeeCodes")]
        public async Task<IActionResult> GetSmapMissionNotesByEmployeeCodes(string employeeCodes)
        {
            var resourceNotes = await _sharePointService.GetSmapMissionNotesByEmployeeCodes(employeeCodes);
            return Ok(resourceNotes);
        }
    }
}

