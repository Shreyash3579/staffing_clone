using Microsoft.AspNetCore.Mvc;

namespace Staffing.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}