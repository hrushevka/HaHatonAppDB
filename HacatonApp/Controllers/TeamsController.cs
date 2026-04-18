using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var teams = await _context.Teams.OrderBy(team => team.Name).ToListAsync();
            var projects = await _context.Projects.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var reviews = await _context.ProjectReviews.ToListAsync();

            var model = teams
                .Select(team =>
                {
                    var project = projects.FirstOrDefault(item => item.Id == team.ProjectId);
                    var projectReviews = project == null
                        ? new List<ProjectReview>()
                        : reviews.Where(review => review.ProjectId == project.Id).ToList();
                    var members = users
                        .Where(user => user.TeamID == team.Id)
                        .OrderBy(user => user.FirstName)
                        .ThenBy(user => user.LastName)
                        .Select(user => new TeamMemberViewModel
                        {
                            FullName = GetUserDisplayName(user),
                            Email = user.Email ?? string.Empty
                        })
                        .ToList();

                    return new TeamCatalogItemViewModel
                    {
                        TeamName = team.Name,
                        ProjectName = project?.Name ?? "Без проекта",
                        ContactEmail = team.ContactEmail,
                        AverageScore = project == null
                            ? 0
                            : projectReviews.Any()
                                ? ProjectScoringHelper.CalculateAverageScore(projectReviews)
                                : project.Score,
                        Members = members
                    };
                })
                .ToList();

            return View(model);
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
