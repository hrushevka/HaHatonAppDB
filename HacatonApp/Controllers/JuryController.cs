using HacatonApp.Models;
using HacatonApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace HacatonApp.Controllers
{
    public class JuryController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;

        public JuryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize]
        public IActionResult SubmitZaiavka()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitZaiavka(JuryZaiavkaViewModel model)
        {
            // Отладка
            System.Diagnostics.Debug.WriteLine($"FirstName: {model.FirstName}");
            System.Diagnostics.Debug.WriteLine($"LastName: {model.LastName}");
            System.Diagnostics.Debug.WriteLine($"Email: {model.ContactEmail}");
            System.Diagnostics.Debug.WriteLine($"Motivation: {model.Motivation}");

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine($"Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Проверка, нет ли уже активной заявки
            var existingApplication = await _context.JuryZaiavkas
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Status == "Wait");

            if (existingApplication != null)
            {
                ModelState.AddModelError("", "У вас уже есть активная заявка");
                return View(model);
            }

            var zaiavka = new JuryZaiavka
            {
                UserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ContactEmail = model.ContactEmail,
                SubmitedAt = DateTime.Now,
                Status = "Wait",
                Motivation = model.Motivation
            };

            try
            {
                await _context.JuryZaiavkas.AddAsync(zaiavka);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Заявка успешно отправлена!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex.Message}");
                ModelState.AddModelError("", "Ошибка при сохранении заявки");
                return View(model);
            }
        }
    }
}