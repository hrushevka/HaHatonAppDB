namespace HacatonApp.Models
{
    public class TeamZaiavka
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string TeamName { get; set; } = string.Empty;  
        public string ProjectName { get; set; } = string.Empty; 
        public string ProjectDescription { get; set; } = string.Empty; 
        public string ContactEmail { get; set; } = string.Empty; 
        public string Motivation { get; set; } = string.Empty; 
        public string Status { get; set; } = "Wait";  
        public DateTime SubmitedAt { get; set; }  
        public DateTime? ReviewedAt { get; set; } 
        public string? AdminComment { get; set; } 
        public List<string> TeamMemberIds { get; set; } = new List<string>(); 
    }

    public class TeamZaiavkaViewModel
    {
        public int ZaiavkaId { get; set; } 
        public string TeamName { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ProjectDescription { get; set; } = string.Empty; 
        public string Motivation { get; set; } = string.Empty; 
        public string ContactEmail { get; set; } = string.Empty; 
        public List<string> TeamMemberIds { get; set; } = new List<string>(); 
    }
}