using Microsoft.AspNetCore.Mvc;

namespace BackgroundPolling.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}