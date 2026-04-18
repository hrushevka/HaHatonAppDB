using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
	public class AdminController : Controller
	{
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index() => View();

        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersWithRoles = new List<UserWithRolesViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                usersWithRoles.Add(new UserWithRolesViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles.ToList()
                });
            }
            return View(usersWithRoles);
        }

        [HttpGet]
        public async Task<IActionResult> EditRoles(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = new List<string> { "Admin", "Jury", "Teamer", "Ghost" };

            var model = new EditUserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                Roles = allRoles.Select(r => new RoleSelection
                {
                    RoleName = r,
                    IsSelected = userRoles.Contains(r)
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return NotFound();

            var selectedRoles = model.Roles
                .Where(r => r.IsSelected)
                .Select(r => r.RoleName)
                .ToList();

            var currentRoles = await _userManager.GetRolesAsync(user);

            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (selectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }

            TempData["SuccessMessage"] = "Роли пользователя обновлены!";
            return RedirectToAction("Users");
        }

        [HttpGet]
        public async Task<IActionResult> Zaiavki() 
        {
            var list = await _context.JuryZaiavkas.ToArrayAsync();
            List<JuryZaiavkaViewModel> listModels = new List<JuryZaiavkaViewModel>();
            for (int i =0; i<list.Length; i++)
            {
                listModels.Add(new JuryZaiavkaViewModel
                {
                    ZaiavkaId = list[i].Id,
                    FirstName = list[i].FirstName,
                    LastName = list[i].LastName,
                    Motivation = list[i].Motivation,
                    ContactEmail = list[i].ContactEmail
                });
            }
            return View(listModels);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KrutiZaiavky(int id, string? adminComment = null)
        {
            var zaiavka = await _context.JuryZaiavkas.FirstOrDefaultAsync(o => o.Id == id);
            if (zaiavka == null) return NotFound();
            var user = await _userManager.FindByIdAsync(zaiavka.UserId);
            if (user == null) return NotFound();

            await _userManager.RemoveFromRoleAsync(user, "Ghost");
            await _userManager.AddToRoleAsync(user, "Jury");

            zaiavka.ReviewedAt = DateTime.Now;
            zaiavka.Status = "Accepted";
            zaiavka.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            return RedirectToAction("Zaiavki");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlohoZaiavky(int id, string? adminComment = null)
        {
            var zaiavka = await _context.JuryZaiavkas.FirstOrDefaultAsync(o => o.Id == id);
            if (zaiavka == null) return NotFound();
            var user = await _userManager.FindByIdAsync(zaiavka.UserId);
            if (user == null) return NotFound();

            zaiavka.ReviewedAt = DateTime.Now;
            zaiavka.Status = "Danied";
            zaiavka.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            return RedirectToAction("Zaiavki");
        }
        [HttpGet]
        public async Task<IActionResult> CheckZaiavka(int id)
        {
            var zaiavka = _context.JuryZaiavkas.FirstOrDefault(o => o.Id == id);
            if (zaiavka == null) return NotFound();
            if (zaiavka.Status != "Wait") return RedirectToAction("Zaiavki");
            var model = new JuryZaiavkaViewModel
            {
                ZaiavkaId = id,
                FirstName = zaiavka.FirstName,
                LastName = zaiavka.LastName,
                Motivation = zaiavka.Motivation,
                ContactEmail = zaiavka.ContactEmail
            };
            return View(model);
        }
        // Добавьте эти методы в существующий AdminController

        [HttpGet]
        public async Task<IActionResult> TeamZaiavki()
        {
            var list = await _context.TeamZaiavkas.ToArrayAsync();
            List<TeamZaiavkaViewModel> listModels = new List<TeamZaiavkaViewModel>();

            for (int i = 0; i < list.Length; i++)
            {
                listModels.Add(new TeamZaiavkaViewModel
                {
                    ZaiavkaId = list[i].Id,
                    TeamName = list[i].TeamName,
                    ProjectName = list[i].ProjectName,
                    ContactEmail = list[i].ContactEmail
                });
            }
            return View(listModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTeamZaiavka(int id, string? adminComment = null)
        {
            var zaiavka = await _context.TeamZaiavkas.FirstOrDefaultAsync(o => o.Id == id);
            if (zaiavka == null) return NotFound();

            // Создаем команду
            var team = new Team
            {
                Name = zaiavka.TeamName,
                ContactEmail = zaiavka.ContactEmail
            };

            var teamResult = await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();
            var teamId = teamResult.Entity.Id;

            // Создаем проект
            var project = new Project
            {
                Name = zaiavka.ProjectName,
                Description = zaiavka.ProjectDescription,
                Score = 0
            };

            var projectResult = await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            // Связываем команду с проектом
            team.ProjectId = projectResult.Entity.Id;
            await _context.SaveChangesAsync();

            // Назначаем пользователя, подавшего заявку, капитаном команды
            var user = await _userManager.FindByIdAsync(zaiavka.UserId);
            if (user != null)
            {
                user.TeamID = teamId;
                await _userManager.UpdateAsync(user);
                await _userManager.RemoveFromRoleAsync(user, "Ghost");
                await _userManager.AddToRoleAsync(user, "Teamer");
            }

            // Добавляем остальных участников
            foreach (var memberId in zaiavka.TeamMemberIds)
            {
                var member = await _userManager.FindByIdAsync(memberId);
                if (member != null)
                {
                    member.TeamID = teamId;
                    await _userManager.UpdateAsync(member);
                    await _userManager.RemoveFromRoleAsync(member, "Ghost");
                    await _userManager.AddToRoleAsync(member, "Teamer");
                }
            }

            zaiavka.ReviewedAt = DateTime.Now;
            zaiavka.Status = "Accepted";
            zaiavka.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка команды {zaiavka.TeamName} одобрена!";
            return RedirectToAction("TeamZaiavki");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTeamZaiavka(int id, string? adminComment = null)
        {
            var zaiavka = await _context.TeamZaiavkas.FirstOrDefaultAsync(o => o.Id == id);
            if (zaiavka == null) return NotFound();

            zaiavka.ReviewedAt = DateTime.Now;
            zaiavka.Status = "Denied";
            zaiavka.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка команды {zaiavka.TeamName} отклонена";
            return RedirectToAction("TeamZaiavki");
        }

        [HttpGet]
        public async Task<IActionResult> CheckTeamZaiavka(int id)
        {
            var zaiavka = await _context.TeamZaiavkas.FirstOrDefaultAsync(o => o.Id == id);
            if (zaiavka == null) return NotFound();
            if (zaiavka.Status != "Wait") return RedirectToAction("TeamZaiavki");

            var model = new TeamZaiavkaViewModel
            {
                ZaiavkaId = zaiavka.Id,
                TeamName = zaiavka.TeamName,
                ProjectName = zaiavka.ProjectName,
                ProjectDescription = zaiavka.ProjectDescription,
                Motivation = zaiavka.Motivation,
                ContactEmail = zaiavka.ContactEmail,
                TeamMemberIds = zaiavka.TeamMemberIds
            };

            return View(model);
        }
    }
}
