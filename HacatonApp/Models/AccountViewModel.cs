using System.ComponentModel.DataAnnotations;

namespace HacatonApp.Models
{
    public class LoginUserViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterUserViewModel
    {
        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Range(typeof(bool), "true", "true", ErrorMessage = "Вы должны принять условия")]
        public bool TermsAccepted { get; set; } = false;
    }

    public class RegisterTeamViewModel
    {
        [Required(ErrorMessage = "Введите название команды")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Выберите капитана")]
        public string CapitainId { get; set; } = string.Empty;

        public List<string> UsersId { get; set; } = new List<string>();
    }

    public class ChangeProfileViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Введите имя")]
        public required string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите фамилию")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите email")]
        [EmailAddress(ErrorMessage = "Неверный формат email")]
        public required string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Введите старый пароль")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введите новый пароль")]
        [MinLength(6, ErrorMessage = "Пароль должен содержать минимум 6 символов")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
        public string NewConfirmPassword { get; set; } = string.Empty;
    }
}