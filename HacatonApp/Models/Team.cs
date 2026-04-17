namespace HacatonApp.Models
{
    public class Team
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required  string ContactEmail { get; set; }
        public int ProjectId { get; set; }
    }
}
