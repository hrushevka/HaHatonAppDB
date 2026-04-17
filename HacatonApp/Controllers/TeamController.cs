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

        [HttpGet]
        public IActionResult AddProject() => View();
        [HttpPost]
        public async Task<IActionResult> AddProject(AddProjectViewModel model)
        {
            if (ModelState.IsValid)
            {
                var project = new Project();
                _context.Projects.Add(project);
            }
            return View(model);
        };

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
                var teamObj = _context.Teams.Add(team);

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
                    foreach (var error in resultUpd.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }
    }
}
