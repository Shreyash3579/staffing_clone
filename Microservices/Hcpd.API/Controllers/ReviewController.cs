using Hcpd.API.Contracts.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hcpd.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        
        /// <summary>
        /// Get Review and rating for an employee
        /// </summary>
        /// <param name="employeeCode"></param>
        /// <returns></returns>
        [HttpGet("employeeReviews")]
        public async Task<IActionResult> GetReviewsByEmployeeCode(string employeeCode)
        {
            var reviews = await _reviewService.GetReviewsByEmployeeCode(employeeCode);
            return Ok(reviews);
        }
    }
}
