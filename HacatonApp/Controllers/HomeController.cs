using HacatonApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HacatonApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
