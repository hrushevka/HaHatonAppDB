using HacatonApp.Models;
using HacatonApp.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace HacatonApp.Controllers
{
    public class JuryController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<ApplicationUser> _userManager;
        public JuryController(ApplicationDbContext context,  UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //[HttpGet]
        //public async Task<IActionResult> RateProject(int id)
        //{
            
        //    var criterias = await _context.Criterias.ToListAsync();
        //    string criteriasJson = JsonConverter.SeriliaseObject(criterias),
        //    RateProjectWithCriteriaToViewModel model = new RateProjectWithCriteriaToViewModel
        //    {
        //        criteriasJson

        //    };

        //    return View(model);
        //}

        [HttpGet]
        public IActionResult SubmitZaiavka() => View();
        [HttpPost]
        public async Task<IActionResult> SubmitZaiavka(JuryZaiavkaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null) return NotFound();

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

                await _context.JuryZaiavkas.AddAsync(zaiavka);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Заявка успешно отправлена!";
                return RedirectToAction("Index", "Home"); 
            }

            return View(model);
        }
    }
}
