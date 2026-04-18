using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices.JavaScript;

namespace HacatonApp.Controllers
{
    public class TeamController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private ApplicationDbContext _context;
        public TeamController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SubmitTeamZaiavka() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTeamZaiavka(TeamZaiavkaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null) return NotFound();

                var zaiavka = new TeamZaiavka
                {
                    UserId = userId,
                    TeamName = model.TeamName,
                    ProjectName = model.ProjectName,
                    ProjectDescription = model.ProjectDescription,
                    ContactEmail = model.ContactEmail,
                    Motivation = model.Motivation,
                    SubmitedAt = DateTime.Now,
                    Status = "Wait",
                    TeamMemberIds = model.TeamMemberIds ?? new List<string>()
                };

                await _context.TeamZaiavkas.AddAsync(zaiavka);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Заявка команды успешно отправлена!";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}
