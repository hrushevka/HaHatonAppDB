using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{ 
    public class LoginUserViewModel
    {
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    public class RegisterUserViewModel
    {
        public bool  TermsAccepted { get; set; } = false;
		public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

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
    public class ChangeProfileViewModel
    {
        public string Id { get; set; }
        public required string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public required string Email { get; set; } = string.Empty;
    }
}
