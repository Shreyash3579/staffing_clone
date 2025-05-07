using Logger.API.Core;
using Microservices.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Logger.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

        [HttpPost]
        public IActionResult SendMailToDeveloperHelpDesk(ErrorLogs errorLogs)
        {
            try
            {
                if (errorLogs != null &&
                    Configuration["Email:ApplicationNames"].ToLower().Split(",").Contains(errorLogs.ApplicationName.ToLower()))
                {
                    EmailHelper.SendMailToDeveloperHelpDesk(errorLogs);
                }
                return Ok();
            }
            catch(Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}