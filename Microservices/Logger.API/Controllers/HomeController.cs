using Microsoft.AspNetCore.Mvc;

namespace Logger.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}