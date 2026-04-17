namespace HacatonApp.Models
{
    public enum JuryZaiavkaStatus
    {
        Wait, Accepted, Danied
    }
    public class JuryZaiavka
    {
        public string Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string Motivation { get; set; } = string.Empty; 
        public string Email { get; set; } = string.Empty;
        
        public JuryZaiavkaStatus Status { get; set; }
        public DateTime SubmitedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? AdminComment { get; set; }
    }
}
