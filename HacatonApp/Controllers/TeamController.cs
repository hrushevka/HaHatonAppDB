using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
    [Authorize]
    public class TeamController : Controller
    {
        private const int MaxTeamSize = 5;
        private const int MaxAdditionalMembers = MaxTeamSize - 1;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public TeamController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return RedirectToAction(nameof(SubmitTeamZaiavka));
        }

        [HttpGet]
        public async Task<IActionResult> SubmitTeamZaiavka()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (currentUser.TeamID.HasValue)
            {
                TempData["ErrorMessage"] = "Вы уже состоите в команде.";
                return RedirectToAction("Index", "Teams");
            }

            if (await _context.TeamZaiavkas.AnyAsync(item => item.UserId == currentUser.Id && item.Status == "Wait"))
            {
                TempData["ErrorMessage"] = "У вас уже есть необработанная заявка команды.";
                return RedirectToAction("Index", "Home");
            }

            var model = new TeamZaiavkaViewModel
            {
                ContactEmail = currentUser.Email ?? string.Empty,
                CaptainName = GetUserDisplayName(currentUser)
            };

            await PopulateAvailableMembersAsync(model, currentUser.Id);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTeamZaiavka(TeamZaiavkaViewModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return Challenge();
            }

            if (currentUser.TeamID.HasValue)
            {
                TempData["ErrorMessage"] = "Вы уже состоите в команде.";
                return RedirectToAction("Index", "Teams");
            }

            model.CaptainName = GetUserDisplayName(currentUser);
            model.TeamMemberIds = (model.TeamMemberIds ?? new List<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            if (model.TeamMemberIds.Count > MaxAdditionalMembers)
            {
                ModelState.AddModelError(nameof(model.TeamMemberIds), $"Можно выбрать не более {MaxAdditionalMembers} дополнительных участников. Всего в команде — до {MaxTeamSize} человек вместе с капитаном.");
            }

            if (model.TeamMemberIds.Contains(currentUser.Id))
            {
                ModelState.AddModelError(nameof(model.TeamMemberIds), "Капитан уже добавлен автоматически. Не выбирайте себя повторно.");
            }

            if (await _context.TeamZaiavkas.AnyAsync(item => item.UserId == currentUser.Id && item.Status == "Wait"))
            {
                ModelState.AddModelError(string.Empty, "У вас уже есть необработанная заявка команды.");
            }

            var selectedUsers = await _userManager.Users
                .Where(user => model.TeamMemberIds.Contains(user.Id))
                .ToListAsync();

            if (selectedUsers.Count != model.TeamMemberIds.Count)
            {
                ModelState.AddModelError(nameof(model.TeamMemberIds), "Часть выбранных участников не найдена.");
            }

            var pendingApplications = await _context.TeamZaiavkas
                .Where(item => item.Status == "Wait")
                .ToListAsync();

            foreach (var selectedUser in selectedUsers)
            {
                if (selectedUser.TeamID.HasValue)
                {
                    ModelState.AddModelError(nameof(model.TeamMemberIds), $"Пользователь {GetUserDisplayName(selectedUser)} уже состоит в команде.");
                }

                var roles = await _userManager.GetRolesAsync(selectedUser);
                if (roles.Any(role => role == "Admin" || role == "Jury" || role == "Teamer"))
                {
                    ModelState.AddModelError(nameof(model.TeamMemberIds), $"Пользователя {GetUserDisplayName(selectedUser)} нельзя добавить в команду.");
                }

                var alreadyInPending = pendingApplications.Any(item =>
                    item.UserId == selectedUser.Id
                    || (item.TeamMemberIds?.Contains(selectedUser.Id) ?? false));

                if (alreadyInPending)
                {
                    ModelState.AddModelError(nameof(model.TeamMemberIds), $"Пользователь {GetUserDisplayName(selectedUser)} уже указан в другой необработанной заявке.");
                }
            }

            if (!ModelState.IsValid)
            {
                await PopulateAvailableMembersAsync(model, currentUser.Id);
                return View(model);
            }

            var application = new TeamZaiavka
            {
                UserId = currentUser.Id,
                TeamName = model.TeamName.Trim(),
                ProjectName = model.ProjectName.Trim(),
                ProjectDescription = model.ProjectDescription.Trim(),
                ContactEmail = model.ContactEmail.Trim(),
                Motivation = model.Motivation?.Trim() ?? string.Empty,
                SubmitedAt = DateTime.UtcNow,
                Status = "Wait",
                TeamMemberIds = model.TeamMemberIds
            };

            await _context.TeamZaiavkas.AddAsync(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Заявка команды «{model.TeamName}» отправлена.";
            return RedirectToAction("Index", "Home");
        }

        private async Task PopulateAvailableMembersAsync(TeamZaiavkaViewModel model, string currentUserId)
        {
            var users = await _userManager.Users
                .Where(user => user.Id != currentUserId && user.TeamID == null)
                .OrderBy(user => user.FirstName)
                .ThenBy(user => user.LastName)
                .ToListAsync();

            var pendingApplications = await _context.TeamZaiavkas
                .Where(item => item.Status == "Wait")
                .ToListAsync();

            var selectedIds = model.TeamMemberIds ?? new List<string>();
            var availableMembers = new List<ParticipantOptionViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Any(role => role == "Admin" || role == "Jury" || role == "Teamer"))
                {
                    continue;
                }

                var alreadyInPending = pendingApplications.Any(item =>
                    item.UserId == user.Id
                    || (item.TeamMemberIds?.Contains(user.Id) ?? false));

                if (alreadyInPending && !selectedIds.Contains(user.Id))
                {
                    continue;
                }

                availableMembers.Add(new ParticipantOptionViewModel
                {
                    UserId = user.Id,
                    FullName = GetUserDisplayName(user),
                    Email = user.Email ?? string.Empty
                });
            }

            model.AvailableMembers = availableMembers;
        }

        private static string GetUserDisplayName(ApplicationUser user)
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            return string.IsNullOrWhiteSpace(fullName)
                ? (user.Email ?? user.UserName ?? user.Id)
                : fullName;
        }
    }
}
