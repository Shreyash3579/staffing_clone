using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staffing.API.Contracts.Services;
using Staffing.API.Models;
using System.Threading.Tasks;
using Staffing.API.Core.Helpers;

namespace Staffing.API.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Constants.Policy.StaffingAllAccess)]
    public class UserPreferencesController : ControllerBase
    {
        private readonly IUserPreferencesService _userPreferencesService;
        public UserPreferencesController(IUserPreferencesService userPreferencesService)
        {
            _userPreferencesService = userPreferencesService;
        }

        /// <summary>
        /// Get user preferences set by user as their default settings.
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns>Saved user preferences for employee</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserPreferences(string employeeCode)
        {
            var userPreferences = await _userPreferencesService.GetUserPreferences(employeeCode);
            return Ok(userPreferences);
        }

        ///// <summary>
        ///// Insert user preferences
        ///// </summary>
        ///// <param name="userPreferences"></param>
        ///// <returns></returns>
        //[HttpPost]
        //[ProducesResponseType(201)]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> InsertUserPreferences(UserPreferences userPreferences)
        //{
        //    var result = await _userPreferencesService.InsertUserPreferences(userPreferences);
        //    return Ok(result);
        //}

        ///// <summary>
        ///// Update User Preferences
        ///// </summary>
        ///// <param name="userPreferences"></param>
        ///// <returns></returns>
        //[HttpPut]
        //[ProducesResponseType(201)]
        //[ProducesResponseType(400)]
        //public async Task<IActionResult> UpdateUserPreferences(UserPreferences userPreferences)
        //{
        //    var result = await _userPreferencesService.UpdateUserPreferences(userPreferences);
        //    return Ok(result);
        //}

        /// <summary>
        /// Upsert User Preferences
        /// </summary>
        /// <param name="userPreferences"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpsertUserPreferences(UserPreferences userPreferences)
        {
            var result = await _userPreferencesService.UpsertUserPreferences(userPreferences);
            return Ok(result);
        }

        /// <summary>
        /// Delete User Preferences for employee 
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteUserPreferences(string employeeCode)
        {
            await _userPreferencesService.DeleteUserPreferences(employeeCode);
            return Ok();
        }
    }
}