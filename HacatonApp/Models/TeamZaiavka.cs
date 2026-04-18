using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{
    public class TeamZaiavka
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ProjectName { get; set; } = string.Empty;

        [Required]
        public string ProjectDescription { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Motivation { get; set; } = string.Empty;

        public string Status { get; set; } = "Wait";
        public DateTime SubmitedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? AdminComment { get; set; }

        public List<string> TeamMemberIds { get; set; } = new();
    }

    public class TeamZaiavkaViewModel
    {
        public int ZaiavkaId { get; set; }

        [Required(ErrorMessage = "Укажите название команды")]
        [StringLength(100)]
        public string TeamName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите название проекта")]
        [StringLength(200)]
        public string ProjectName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Добавьте описание проекта")]
        public string ProjectDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите email для связи")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        public string ContactEmail { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Motivation { get; set; } = string.Empty;

        public List<string> TeamMemberIds { get; set; } = new();
        public List<ParticipantOptionViewModel> AvailableMembers { get; set; } = new();
        public List<string> TeamMemberNames { get; set; } = new();
        public string CaptainName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? AdminComment { get; set; }
    }

    public class ParticipantOptionViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
