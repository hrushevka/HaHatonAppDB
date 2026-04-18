namespace HacatonApp.Models
{
    public class Criteria
    {
        public int Id { get; set; }
        public required string Name { get; set; } = string.Empty;
        public required float Weight { get; set; }
    }
}
