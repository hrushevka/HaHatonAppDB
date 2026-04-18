using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        // GET: CriteriaController
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCriteriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var cretaria = new Criteria
                {
                    Name = model.Name,
                    Weight = model.Weight
                };
                await _context.Criterias.AddAsync(cretaria);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var criteria = _context.Criterias.FirstOrDefault(o => o.Id == id);
            if (criteria == null) return NotFound();
            return View(criteria);
        }

        // POST: CriteriaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CreateCriteriaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var criteria = _context.Criterias.FirstOrDefault(o => o.Id == id);
                if (criteria == null) return NotFound();
                criteria.Name = model.Name;
                criteria.Weight = model.Weight;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        // GET: CriteriaController/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var criteria = _context.Criterias.FirstOrDefault(o => o.Id == id);
            if (criteria == null) return NotFound(); 
            _context.Criterias.Remove(criteria);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
