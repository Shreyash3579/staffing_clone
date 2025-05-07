using Microsoft.AspNetCore.Mvc;

namespace Polaris.API.Controller
{
    public class HomeController : ControllerBase
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}
