using HacatonApp.Data;
using HacatonApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HacatonApp.Controllers
{
    public class ResultsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ResultsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects
                .OrderByDescending(project => project.Score)
                .ThenBy(project => project.Name)
                .ToListAsync();
            var teams = await _context.Teams.ToListAsync();
            var users = await _context.Users.ToListAsync();
            var reviews = await _context.ProjectReviews.ToListAsync();

            var model = projects
                .Select(project =>
                {
                    var team = teams.FirstOrDefault(item => item.ProjectId == project.Id);
                    var projectReviews = reviews.Where(review => review.ProjectId == project.Id).ToList();
                    var members = team == null
                        ? new List<TeamMemberViewModel>()
                        : users
                            .Where(user => user.TeamID == team.Id)
                            .OrderBy(user => user.FirstName)
                            .ThenBy(user => user.LastName)
                            .Select(user => new TeamMemberViewModel
                            {
                                FullName = GetUserDisplayName(user),
                                Email = user.Email ?? string.Empty
                            })
                            .ToList();

                    return new ProjectLeaderboardItemViewModel
                    {
                        ProjectId = project.Id,
                        ProjectName = project.Name,
                        ProjectDescription = project.Description,
                        TeamName = team?.Name ?? "Без команды",
                        AverageScore = projectReviews.Any()
                            ? ProjectScoringHelper.CalculateAverageScore(projectReviews)
                            : project.Score,
                        ReviewsCount = projectReviews.Count,
                        Members = members
                    };
                })
                .OrderByDescending(item => item.AverageScore)
                .ThenByDescending(item => item.ReviewsCount)
                .ThenBy(item => item.ProjectName)
                .ToList();

            for (var index = 0; index < model.Count; index++)
            {
                model[index].Rank = index + 1;
            }

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
