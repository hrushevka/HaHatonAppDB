using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{
    public enum JuryZaiavkaStatus
    {
        Wait,
        Accepted,
        Danied
    }

    public class JuryZaiavka
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}".Trim();

        [StringLength(1000)]
        public string Motivation { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string ContactEmail { get; set; } = string.Empty;

        public string Status { get; set; } = "Wait";
        public DateTime SubmitedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        [StringLength(1000)]
        public string? AdminComment { get; set; }
    }

    public class JuryZaiavkaViewModel
    {
        public int ZaiavkaId { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Расскажите о вашей мотивации")]
        public string Motivation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Укажите email")]
        [EmailAddress(ErrorMessage = "Введите корректный email")]
        public string ContactEmail { get; set; } = string.Empty;
    }
}
