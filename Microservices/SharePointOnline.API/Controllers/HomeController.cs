using Microsoft.AspNetCore.Mvc;
using SharePointOnline.API.Models;
using System.Diagnostics;

namespace SharePointOnline.API.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

    }
}