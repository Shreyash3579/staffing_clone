using Microsoft.AspNetCore.Mvc;

namespace BvuCD.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}