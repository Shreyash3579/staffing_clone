using Microsoft.AspNetCore.Mvc;

namespace Vacation.API.Controllers
{
    public class HomeController : Controller
    {
        //test comment
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}