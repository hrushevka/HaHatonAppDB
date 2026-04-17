using Microsoft.AspNetCore.Mvc;

namespace HacatonApp.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
