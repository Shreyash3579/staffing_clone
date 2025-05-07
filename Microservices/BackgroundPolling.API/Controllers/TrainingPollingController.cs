using BackgroundPolling.API.Contracts.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BackgroundPolling.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingPollingController : ControllerBase
    {
         public ITrainingPollingService _trainingPollingService;
        public TrainingPollingController(ITrainingPollingService trainingPollingService)
        {
            _trainingPollingService = trainingPollingService;
        }

        /// <summary>
        /// Saves all Trainings data from BVU_CD to Analytics database
        /// 
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UpsertTrainings()
        {
            await _trainingPollingService.upsertTrainings();
            return Ok();
        }
    }
}