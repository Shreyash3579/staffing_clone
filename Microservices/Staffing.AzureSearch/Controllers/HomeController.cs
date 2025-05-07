using Microsoft.AspNetCore.Mvc;

namespace Staffing.AzureSearch.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}