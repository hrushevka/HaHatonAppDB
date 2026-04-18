using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
    public class CriteriaController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;

        public CriteriaController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var criterias = await _context.Criterias.ToListAsync();
            return View(criterias);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCriteriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var criteria = new Criteria
                {
                    Name = model.Name,
                    Weight = model.Weight
                };
                await _context.Criterias.AddAsync(criteria);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Критерий добавлен!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var criteria = await _context.Criterias.FirstOrDefaultAsync(o => o.Id == id);
            if (criteria == null) return NotFound();
            return View(criteria);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCriteriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var criteria = await _context.Criterias.FirstOrDefaultAsync(o => o.Id == id);
                if (criteria == null) return NotFound();
                criteria.Name = model.Name;
                criteria.Weight = model.Weight;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Критерий обновлён!";
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var criteria = await _context.Criterias.FirstOrDefaultAsync(o => o.Id == id);
            if (criteria == null) return NotFound();
            _context.Criterias.Remove(criteria);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Критерий удалён!";
            return RedirectToAction("Index");
        }
    }
}