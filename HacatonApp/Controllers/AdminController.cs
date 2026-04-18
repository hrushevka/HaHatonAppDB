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
            var list = await _context.JuryZaiavkas.ToListAsync();
            return View(list);
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

            return View();
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

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> CheckZaiavka(int id)
        {
            var zaiavka = _context.JuryZaiavkas.FirstOrDefault(o => o.Id == id);
            if (zaiavka == null) return NotFound();
            return View(zaiavka);
        }
    }
}
