using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
    [Authorize]
    public class JuryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JuryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "Jury,Admin")]
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var projects = await _context.Projects.OrderBy(project => project.Name).ToListAsync();
            var teams = await _context.Teams.ToListAsync();
            var reviews = await _context.ProjectReviews.ToListAsync();

            var model = projects
                .Select(project =>
                {
                    var team = teams.FirstOrDefault(item => item.ProjectId == project.Id);
                    var projectReviews = reviews.Where(review => review.ProjectId == project.Id).ToList();

                    return new JuryProjectListItemViewModel
                    {
                        ProjectId = project.Id,
                        ProjectName = project.Name,
                        ProjectDescription = project.Description,
                        TeamName = team?.Name ?? "Без команды",
                        AverageScore = projectReviews.Any()
                            ? ProjectScoringHelper.CalculateAverageScore(projectReviews)
                            : project.Score,
                        ReviewsCount = projectReviews.Count,
                        IsRatedByCurrentUser = !string.IsNullOrWhiteSpace(currentUserId)
                            && projectReviews.Any(review => review.JuryUserId == currentUserId)
                    };
                })
                .OrderByDescending(item => item.AverageScore)
                .ThenBy(item => item.ProjectName)
                .ToList();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Jury,Admin")]
        public async Task<IActionResult> RateProject(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            var project = await _context.Projects.FirstOrDefaultAsync(item => item.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            var criteria = await _context.Criterias.OrderBy(item => item.Id).ToListAsync();
            if (!criteria.Any())
            {
                TempData["ErrorMessage"] = "Администратор ещё не настроил критерии оценки.";
                return RedirectToAction(nameof(Index));
            }

            var teamName = await _context.Teams
                .Where(team => team.ProjectId == id)
                .Select(team => team.Name)
                .FirstOrDefaultAsync() ?? "Без команды";

            var existingReview = await _context.ProjectReviews
                .FirstOrDefaultAsync(review => review.ProjectId == id && review.JuryUserId == currentUserId);

            var existingScores = existingReview == null
                ? new List<ProjectCriterionScore>()
                : await _context.ProjectCriterionScores
                    .Where(score => score.ProjectReviewId == existingReview.Id)
                    .ToListAsync();

            var model = new RateProjectViewModel
            {
                ProjectId = project.Id,
                ProjectName = project.Name,
                ProjectDescription = project.Description,
                TeamName = teamName,
                CurrentAverageScore = project.Score,
                ReviewsCount = await _context.ProjectReviews.CountAsync(review => review.ProjectId == id),
                Comment = existingReview?.Comment ?? string.Empty,
                CriteriaScores = criteria
                    .Select(criterion => new ProjectCriterionInputViewModel
                    {
                        CriteriaId = criterion.Id,
                        CriteriaName = criterion.Name,
                        Weight = criterion.Weight,
                        MaxScore = criterion.MaxScore,
                        Score = existingScores.FirstOrDefault(score => score.CriteriaId == criterion.Id)?.Score ?? 0
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Jury,Admin")]
        public async Task<IActionResult> RateProject(RateProjectViewModel model)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                return Challenge();
            }

            var project = await _context.Projects.FirstOrDefaultAsync(item => item.Id == model.ProjectId);
            if (project == null)
            {
                return NotFound();
            }

            var criteria = await _context.Criterias.OrderBy(item => item.Id).ToListAsync();
            if (!criteria.Any())
            {
                TempData["ErrorMessage"] = "Администратор ещё не настроил критерии оценки.";
                return RedirectToAction(nameof(Index));
            }

            var criteriaIds = criteria.Select(item => item.Id).OrderBy(id => id).ToList();
            var inputIds = (model.CriteriaScores ?? new List<ProjectCriterionInputViewModel>())
                .Select(item => item.CriteriaId)
                .OrderBy(id => id)
                .ToList();

            if (criteriaIds.Count != inputIds.Count || !criteriaIds.SequenceEqual(inputIds))
            {
                ModelState.AddModelError(string.Empty, "Набор критериев изменился. Обновите страницу и попробуйте ещё раз.");
            }

            var criteriaMap = criteria.ToDictionary(item => item.Id);
            foreach (var criterionInput in model.CriteriaScores ?? new List<ProjectCriterionInputViewModel>())
            {
                if (!criteriaMap.TryGetValue(criterionInput.CriteriaId, out var criterion))
                {
                    ModelState.AddModelError(string.Empty, "Один из критериев больше не существует.");
                    continue;
                }

                criterionInput.CriteriaName = criterion.Name;
                criterionInput.Weight = criterion.Weight;
                criterionInput.MaxScore = criterion.MaxScore;

                if (criterionInput.Score < 0 || criterionInput.Score > criterion.MaxScore)
                {
                    ModelState.AddModelError(string.Empty, $"Критерий «{criterion.Name}» должен быть в диапазоне от 0 до {criterion.MaxScore}.");
                }
            }

            if (!ModelState.IsValid)
            {
                model.ProjectName = project.Name;
                model.ProjectDescription = project.Description;
                model.TeamName = await _context.Teams
                    .Where(team => team.ProjectId == project.Id)
                    .Select(team => team.Name)
                    .FirstOrDefaultAsync() ?? "Без команды";
                model.CurrentAverageScore = project.Score;
                model.ReviewsCount = await _context.ProjectReviews.CountAsync(review => review.ProjectId == project.Id);

                return View(model);
            }

            var review = await _context.ProjectReviews
                .FirstOrDefaultAsync(item => item.ProjectId == model.ProjectId && item.JuryUserId == currentUserId);

            if (review == null)
            {
                review = new ProjectReview
                {
                    ProjectId = model.ProjectId,
                    JuryUserId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.ProjectReviews.AddAsync(review);
                await _context.SaveChangesAsync();
            }
            else
            {
                var previousScores = await _context.ProjectCriterionScores
                    .Where(score => score.ProjectReviewId == review.Id)
                    .ToListAsync();

                if (previousScores.Any())
                {
                    _context.ProjectCriterionScores.RemoveRange(previousScores);
                    await _context.SaveChangesAsync();
                }
            }

            var newScores = (model.CriteriaScores ?? new List<ProjectCriterionInputViewModel>())
                .Select(item => new ProjectCriterionScore
                {
                    ProjectReviewId = review.Id,
                    CriteriaId = item.CriteriaId,
                    Score = item.Score
                })
                .ToList();

            if (newScores.Any())
            {
                await _context.ProjectCriterionScores.AddRangeAsync(newScores);
            }

            review.Comment = model.Comment?.Trim() ?? string.Empty;
            review.TotalScore = ProjectScoringHelper.CalculateTotalScore(criteria, newScores);
            review.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var projectReviews = await _context.ProjectReviews
                .Where(item => item.ProjectId == project.Id)
                .ToListAsync();

            project.Score = ProjectScoringHelper.CalculateAverageScore(projectReviews);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Оценка для проекта «{project.Name}» сохранена.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult SubmitZaiavka() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitZaiavka(JuryZaiavkaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            if (await _context.JuryZaiavkas.AnyAsync(item => item.UserId == userId && item.Status == "Wait"))
            {
                ModelState.AddModelError(string.Empty, "У вас уже есть необработанная заявка на судейство.");
                return View(model);
            }

            var application = new JuryZaiavka
            {
                UserId = userId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ContactEmail = model.ContactEmail,
                SubmitedAt = DateTime.UtcNow,
                Status = "Wait",
                Motivation = model.Motivation
            };

            await _context.JuryZaiavkas.AddAsync(application);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Заявка на судейство отправлена.";
            return RedirectToAction("Index", "Home");
        }
    }
}
