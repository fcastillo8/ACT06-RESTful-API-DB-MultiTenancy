using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "El TenantId es requerido.")]
        public string TenantId { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña actual es requerida.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La nueva contraseña debe tener al menos 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "El correo o nombre de usuario es requerido.")]
        public string UsernameOrEmail { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required(ErrorMessage = "El nombre de usuario es requerido.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "El email es requerido.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es requerida.")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        [Required(ErrorMessage = "El TenantId es requerido.")]
        public string TenantId { get; set; } = string.Empty;
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
