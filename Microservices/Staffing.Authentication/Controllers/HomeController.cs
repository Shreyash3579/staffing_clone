﻿using Microsoft.AspNetCore.Mvc;

namespace Staffing.Authentication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}