namespace HacatonApp.Models
{
    public enum JuryZaiavkaStatus
    {
        Wait, Accepted, Danied
    }
    public class JuryZaiavka
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public required string  FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Motivation { get; set; } = string.Empty; 
        public required string ContactEmail { get; set; } = string.Empty;
        
        public JuryZaiavkaStatus Status { get; set; }
        public DateTime SubmitedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminComment { get; set; }
    }
    public class JuryZaiavkaViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Motivation { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }
}
