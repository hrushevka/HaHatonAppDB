using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{
    public class ProjectCriterionInputViewModel
    {
        public int CriteriaId { get; set; }
        public string CriteriaName { get; set; } = string.Empty;
        public float Weight { get; set; }
        public float MaxScore { get; set; }
        public float Score { get; set; }
    }

    public class RateProjectViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public float CurrentAverageScore { get; set; }
        public int ReviewsCount { get; set; }

        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public List<ProjectCriterionInputViewModel> CriteriaScores { get; set; } = new();
    }

    public class JuryProjectListItemViewModel
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public float AverageScore { get; set; }
        public int ReviewsCount { get; set; }
        public bool IsRatedByCurrentUser { get; set; }
    }

    public class TeamMemberViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public class ProjectLeaderboardItemViewModel
    {
        public int Rank { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;
        public float AverageScore { get; set; }
        public int ReviewsCount { get; set; }
        public List<TeamMemberViewModel> Members { get; set; } = new();
    }

    public class TeamCatalogItemViewModel
    {
        public string TeamName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public float AverageScore { get; set; }
        public List<TeamMemberViewModel> Members { get; set; } = new();
    }
}
