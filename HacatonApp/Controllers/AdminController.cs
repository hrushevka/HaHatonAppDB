using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
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
                    Email = user.Email ?? string.Empty,
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
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = new List<string> { "Admin", "Jury", "Teamer", "Ghost" };

            var model = new EditUserRoleViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = GetUserDisplayName(user),
                Roles = allRoles
                    .Select(role => new RoleSelection
                    {
                        RoleName = role,
                        IsSelected = userRoles.Contains(role)
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoles(EditUserRoleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            var selectedRoles = model.Roles
                .Where(role => role.IsSelected)
                .Select(role => role.RoleName)
                .ToList();

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            if (selectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }

            TempData["SuccessMessage"] = "Роли пользователя обновлены.";
            return RedirectToAction(nameof(Users));
        }

        [HttpGet]
        public async Task<IActionResult> Zaiavki()
        {
            var applications = await _context.JuryZaiavkas
                .OrderByDescending(application => application.SubmitedAt)
                .ToListAsync();

            var model = applications
                .Select(application => new JuryZaiavkaViewModel
                {
                    ZaiavkaId = application.Id,
                    FirstName = application.FirstName,
                    LastName = application.LastName,
                    Motivation = application.Motivation,
                    ContactEmail = application.ContactEmail
                })
                .ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KrutiZaiavky(int id, string? adminComment = null)
        {
            var application = await _context.JuryZaiavkas.FirstOrDefaultAsync(item => item.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(application.UserId);
            if (user == null)
            {
                return NotFound();
            }

            if (await _userManager.IsInRoleAsync(user, "Ghost"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Ghost");
            }

            if (!await _userManager.IsInRoleAsync(user, "Jury"))
            {
                await _userManager.AddToRoleAsync(user, "Jury");
            }

            application.ReviewedAt = DateTime.UtcNow;
            application.Status = "Accepted";
            application.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка {application.FullName} одобрена.";
            return RedirectToAction(nameof(Zaiavki));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlohoZaiavky(int id, string? adminComment = null)
        {
            var application = await _context.JuryZaiavkas.FirstOrDefaultAsync(item => item.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            application.ReviewedAt = DateTime.UtcNow;
            application.Status = "Danied";
            application.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка {application.FullName} отклонена.";
            return RedirectToAction(nameof(Zaiavki));
        }

        [HttpGet]
        public async Task<IActionResult> CheckZaiavka(int id)
        {
            var application = await _context.JuryZaiavkas.FirstOrDefaultAsync(item => item.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            if (application.Status != "Wait")
            {
                return RedirectToAction(nameof(Zaiavki));
            }

            var model = new JuryZaiavkaViewModel
            {
                ZaiavkaId = application.Id,
                FirstName = application.FirstName,
                LastName = application.LastName,
                Motivation = application.Motivation,
                ContactEmail = application.ContactEmail
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> TeamZaiavki()
        {
            var applications = await _context.TeamZaiavkas
                .OrderByDescending(application => application.SubmitedAt)
                .ToListAsync();

            var model = applications
                .Select(application => new TeamZaiavkaViewModel
                {
                    ZaiavkaId = application.Id,
                    TeamName = application.TeamName,
                    ProjectName = application.ProjectName,
                    ContactEmail = application.ContactEmail,
                    Status = application.Status,
                    AdminComment = application.AdminComment
                })
                .ToList();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CheckTeamZaiavka(int id)
        {
            var application = await _context.TeamZaiavkas.FirstOrDefaultAsync(item => item.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            var captain = await _userManager.FindByIdAsync(application.UserId);
            var teamMemberNames = await GetParticipantNamesAsync(application.TeamMemberIds);

            var model = new TeamZaiavkaViewModel
            {
                ZaiavkaId = application.Id,
                TeamName = application.TeamName,
                ProjectName = application.ProjectName,
                ProjectDescription = application.ProjectDescription,
                Motivation = application.Motivation,
                ContactEmail = application.ContactEmail,
                TeamMemberIds = application.TeamMemberIds,
                TeamMemberNames = teamMemberNames,
                CaptainName = captain == null ? "Не найден" : GetUserDisplayName(captain),
                Status = application.Status,
                AdminComment = application.AdminComment
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptTeamZaiavka(int id, string? adminComment = null)
        {
            var application = await _context.TeamZaiavkas.FirstOrDefaultAsync(item => item.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            if (application.Status != "Wait")
            {
                TempData["ErrorMessage"] = "Эта заявка уже обработана.";
                return RedirectToAction(nameof(TeamZaiavki));
            }

            var captain = await _userManager.FindByIdAsync(application.UserId);
            if (captain == null)
            {
                TempData["ErrorMessage"] = "Капитан команды не найден.";
                return RedirectToAction(nameof(CheckTeamZaiavka), new { id });
            }

            if (captain.TeamID.HasValue)
            {
                TempData["ErrorMessage"] = "Капитан уже состоит в другой команде.";
                return RedirectToAction(nameof(CheckTeamZaiavka), new { id });
            }

            var memberIds = (application.TeamMemberIds ?? new List<string>())
                .Where(memberId => !string.IsNullOrWhiteSpace(memberId))
                .Distinct()
                .ToList();

            if (memberIds.Count > 4)
            {
                TempData["ErrorMessage"] = "В заявке можно указать не более 4 дополнительных участников, всего до 5 человек в команде.";
                return RedirectToAction(nameof(CheckTeamZaiavka), new { id });
            }

            var members = await _userManager.Users
                .Where(user => memberIds.Contains(user.Id))
                .ToListAsync();

            if (members.Count != memberIds.Count)
            {
                TempData["ErrorMessage"] = "Часть выбранных участников не найдена.";
                return RedirectToAction(nameof(CheckTeamZaiavka), new { id });
            }

            var invalidMembers = new List<string>();
            foreach (var member in members)
            {
                if (member.TeamID.HasValue)
                {
                    invalidMembers.Add($"{GetUserDisplayName(member)} уже состоит в команде");
                    continue;
                }

                var roles = await _userManager.GetRolesAsync(member);
                if (roles.Any(role => role == "Admin" || role == "Jury" || role == "Teamer"))
                {
                    invalidMembers.Add($"{GetUserDisplayName(member)} недоступен для команды");
                }
            }

            if (invalidMembers.Any())
            {
                TempData["ErrorMessage"] = string.Join("; ", invalidMembers);
                return RedirectToAction(nameof(CheckTeamZaiavka), new { id });
            }

            var project = new Project
            {
                Name = application.ProjectName,
                Description = application.ProjectDescription,
                Score = 0
            };
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var team = new Team
            {
                Name = application.TeamName,
                ContactEmail = application.ContactEmail,
                ProjectId = project.Id
            };
            await _context.Teams.AddAsync(team);
            await _context.SaveChangesAsync();

            captain.TeamID = team.Id;
            await _userManager.UpdateAsync(captain);
            await PromoteToTeamerAsync(captain);

            foreach (var member in members)
            {
                member.TeamID = team.Id;
                await _userManager.UpdateAsync(member);
                await PromoteToTeamerAsync(member);
            }

            application.ReviewedAt = DateTime.UtcNow;
            application.Status = "Accepted";
            application.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка команды «{application.TeamName}» одобрена.";
            return RedirectToAction(nameof(TeamZaiavki));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectTeamZaiavka(int id, string? adminComment = null)
        {
            var application = await _context.TeamZaiavkas.FirstOrDefaultAsync(item => item.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            application.ReviewedAt = DateTime.UtcNow;
            application.Status = "Denied";
            application.AdminComment = adminComment;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка команды «{application.TeamName}» отклонена.";
            return RedirectToAction(nameof(TeamZaiavki));
        }

        private async Task PromoteToTeamerAsync(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Ghost"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Ghost");
            }

            if (!await _userManager.IsInRoleAsync(user, "Teamer"))
            {
                await _userManager.AddToRoleAsync(user, "Teamer");
            }
        }

        private async Task<List<string>> GetParticipantNamesAsync(IEnumerable<string> userIds)
        {
            var ids = userIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new List<string>();
            }

            var users = await _userManager.Users
                .Where(user => ids.Contains(user.Id))
                .ToListAsync();

            var userMap = users.ToDictionary(user => user.Id, user => GetUserDisplayName(user));
            return ids
                .Select(id => userMap.TryGetValue(id, out var value) ? value : id)
                .ToList();
        }

        private static string GetUserDisplayName(ApplicationUser user)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName)
                ? (user.Email ?? user.UserName ?? user.Id)
                : $"{fullName} ({user.Email})";
        }
    }
}
