﻿using Microsoft.AspNetCore.Mvc;

namespace CaseIntake.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return new RedirectResult("~/swagger");
        }
    }
}