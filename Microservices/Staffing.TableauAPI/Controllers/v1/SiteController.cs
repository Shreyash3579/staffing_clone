using Microsoft.AspNetCore.Mvc;
using Staffing.TableauAPI.Helpers;

namespace Staffing.TableauAPI.v1.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //[Route("api/[controller]")]
    public class SiteController : ControllerBase
    {

        [HttpGet]
        public ActionResult Get()
        {
            return Ok("2.0");
        }
        [HttpGet]
        [Route("SendRequest", Name = nameof(SendRequest))]
        public ActionResult SendRequest()
        {
            InvokeWebRequest request = new InvokeWebRequest();
            //request.GetWebResponse();
            return Ok(request.GetWebResponse());
        }
    }
}
