using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CriteriaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CriteriaController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? id)
        {
            var items = await _context.Criterias
                .OrderBy(criteria => criteria.Id)
                .ToListAsync();

            var model = new CriteriaConstructorViewModel
            {
                Items = items,
                Form = new CreateCriteriaViewModel
                {
                    Weight = 1,
                    MaxScore = 10
                }
            };

            if (id.HasValue)
            {
                var criteria = items.FirstOrDefault(item => item.Id == id.Value);
                if (criteria == null)
                {
                    return NotFound();
                }

                model.Form = new CreateCriteriaViewModel
                {
                    Id = criteria.Id,
                    Name = criteria.Name,
                    Weight = criteria.Weight,
                    MaxScore = criteria.MaxScore
                };
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(CriteriaConstructorViewModel model)
        {
            model.Items = await _context.Criterias
                .OrderBy(criteria => criteria.Id)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            if (model.Form.Id.HasValue)
            {
                var criteria = await _context.Criterias.FirstOrDefaultAsync(item => item.Id == model.Form.Id.Value);
                if (criteria == null)
                {
                    return NotFound();
                }

                criteria.Name = model.Form.Name.Trim();
                criteria.Weight = model.Form.Weight;
                criteria.MaxScore = model.Form.MaxScore;

                TempData["SuccessMessage"] = "Критерий обновлён.";
            }
            else
            {
                var criteria = new Criteria
                {
                    Name = model.Form.Name.Trim(),
                    Weight = model.Form.Weight,
                    MaxScore = model.Form.MaxScore
                };

                await _context.Criterias.AddAsync(criteria);
                TempData["SuccessMessage"] = "Критерий добавлен.";
            }

            await _context.SaveChangesAsync();
            await RecalculateAllProjectScoresAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var criteria = await _context.Criterias.FirstOrDefaultAsync(item => item.Id == id);
            if (criteria == null)
            {
                return NotFound();
            }

            _context.Criterias.Remove(criteria);
            await _context.SaveChangesAsync();
            await RecalculateAllProjectScoresAsync();

            TempData["SuccessMessage"] = "Критерий удалён.";
            return RedirectToAction(nameof(Index));
        }

        private async Task RecalculateAllProjectScoresAsync()
        {
            var criterias = await _context.Criterias.OrderBy(criteria => criteria.Id).ToListAsync();
            var reviews = await _context.ProjectReviews.ToListAsync();
            var criterionScores = await _context.ProjectCriterionScores.ToListAsync();

            foreach (var review in reviews)
            {
                var scores = criterionScores.Where(score => score.ProjectReviewId == review.Id).ToList();
                review.TotalScore = ProjectScoringHelper.CalculateTotalScore(criterias, scores);
                review.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            var projects = await _context.Projects.ToListAsync();
            foreach (var project in projects)
            {
                var projectReviews = reviews.Where(review => review.ProjectId == project.Id).ToList();
                project.Score = ProjectScoringHelper.CalculateAverageScore(projectReviews);
            }

            await _context.SaveChangesAsync();
        }
    }
}
