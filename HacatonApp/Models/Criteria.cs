using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{
    public class Criteria
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(0.1, 1000)]
        public float Weight { get; set; } = 1;

        [Range(1, 1000)]
        public float MaxScore { get; set; } = 10;
    }
}
