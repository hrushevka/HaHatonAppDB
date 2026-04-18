using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{
    public class ProjectReview
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }

        [Required]
        public string JuryUserId { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Comment { get; set; } = string.Empty;

        public float TotalScore { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProjectCriterionScore
    {
        public int Id { get; set; }
        public int ProjectReviewId { get; set; }
        public int CriteriaId { get; set; }
        public float Score { get; set; }
    }
}
