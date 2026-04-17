namespace HacatonApp.Models
{ 
    public class RegisterNewUserModelQ
    {
		public string Password { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty; 
		public string FirstName { get; set; } = string.Empty;
		public string LastName { get; set; } = string.Empty;
	}
    public class RegisterTeamViewModel
    {
        public string Name { get; set; } = string.Empty;
        public string CapitainId { get; set; } = string.Empty;
        public List<string> UsersId { get; set; } = new List<string>();
    }
}
