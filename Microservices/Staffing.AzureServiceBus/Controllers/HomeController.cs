using Microsoft.AspNetCore.Mvc;

namespace Staffing.AzureServiceBus.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}