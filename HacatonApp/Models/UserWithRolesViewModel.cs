namespace HacatonApp.Models
{
    public class UserWithRolesViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class EditUserRoleViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public List<RoleSelection> Roles { get; set; } = new List<RoleSelection>();
    }

    public class RoleSelection
    {
        public string RoleName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
