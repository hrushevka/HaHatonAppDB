namespace HacatonApp.Models
{
    public class RegisterTeamViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string CapitainId { get; set; } = string.Empty;
        public List<string> UsersId { get; set; } = new List<string>();
    }
}
