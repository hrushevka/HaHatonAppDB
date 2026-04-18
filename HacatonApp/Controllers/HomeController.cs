using HacatonApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace HacatonApp.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult ProjectsList() => View();
    }
}
