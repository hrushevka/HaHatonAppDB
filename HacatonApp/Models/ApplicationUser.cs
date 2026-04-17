using Microsoft.AspNetCore.Identity;

namespace HacatonApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        public required string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public required string Email { get; set; } = string.Empty;
        public int? TeamID { get; set; }

    }
}
