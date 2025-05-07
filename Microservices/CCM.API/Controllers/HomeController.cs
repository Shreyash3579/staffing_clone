using Microsoft.AspNetCore.Mvc;

namespace CCM.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}