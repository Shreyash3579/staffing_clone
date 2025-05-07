using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasisPollingController : ControllerBase
    {
        private readonly IBasisPollingService _basisPollingService;

        public BasisPollingController(IBasisPollingService basisPollingService)
        {
            _basisPollingService = basisPollingService;
        }

        /// <summary>
        /// Upsert Practice Areas in analytics DB
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("insertPracticeAreaLookUpData")]
        public async Task<IActionResult> InsertPracticeAreaLookUpData()
        {
            await _basisPollingService.InsertPracticeAreaLookUpData();
            return Ok();
        }

        /// <summary>
        /// Upsert Practice affiliation in analytics DB
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("upsertAffiliations")]
        public async Task<IActionResult> UpsertPracticeAffiliations()
        {
            await _basisPollingService.UpsertPracticeAffiliations();
            return Ok();
        }

        /// <summary>
        /// Insert snapshot every month
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("insertMonthlySnapshotForPracticeAffiliations")]
        public async Task<IActionResult> InsertMonthlySnapshotForPracticeAffiliations()
        {
            await _basisPollingService.InsertMonthlySnapshotForPracticeAffiliations();
            return Ok();
        }


    }
}
