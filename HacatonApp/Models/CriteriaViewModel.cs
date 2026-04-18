using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{
    public class CreateCriteriaViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Укажите название критерия")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Range(0.1, 1000, ErrorMessage = "Вес должен быть больше 0")]
        public float Weight { get; set; } = 1;

        [Range(1, 1000, ErrorMessage = "Максимальный балл должен быть больше 0")]
        public float MaxScore { get; set; } = 10;
    }

    public class CriteriaConstructorViewModel
    {
        public CreateCriteriaViewModel Form { get; set; } = new();
        public List<Criteria> Items { get; set; } = new();
    }
}
