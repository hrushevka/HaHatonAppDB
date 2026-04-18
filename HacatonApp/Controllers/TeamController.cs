using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;

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
        public IActionResult AddProject() => View();

        [HttpPost]
        public async Task<IActionResult> AddProject(AddProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var project = new Project
                {
                    Name = model.Name,
                    Description = model.Description
                };
                await _context.Projects.AddAsync(project);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Проект добавлен!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult RegisterTeam() => View();

        [HttpPost]
        public async Task<IActionResult> RegisterTeam(RegisterTeamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var capitain = await _userManager.FindByIdAsync(model.CapitainId);
                if (capitain == null) return NotFound();

                var team = new Team
                {
                    Name = model.Name,
                    ContactEmail = capitain.Email
                };
                var teamObj = await _context.Teams.AddAsync(team);
                await _context.SaveChangesAsync();

                var curTeamId = teamObj.Entity.Id;

                var usersIdArray = model.UsersId.ToArray();

                for (int i = 0; i < usersIdArray.Length; i++)
                {
                    var user = await _userManager.FindByIdAsync(usersIdArray[i]);
                    if (user == null) continue;

                    user.TeamID = curTeamId;
                    var resultUpd = await _userManager.UpdateAsync(user);

                    if (resultUpd.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "Teamer");
                        await _userManager.RemoveFromRoleAsync(user, "Ghost");
                    }
                    else
                    {
                        foreach (var error in resultUpd.Errors)
                            ModelState.AddModelError("", error.Description);
                    }
                }

                TempData["SuccessMessage"] = "Команда успешно зарегистрирована!";
                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }
    }
}