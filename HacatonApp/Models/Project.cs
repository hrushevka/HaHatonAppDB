namespace HacatonApp.Models
{
    public class Project
    {
        public int Id { get; set; }
        public required  string Name { get; set; } = string.Empty;
        public required  string Description { get; set; } = string.Empty;
        public string Criteriars {  get; set; } = string.Empty;
        public float Score { get; set; }
    }
}
