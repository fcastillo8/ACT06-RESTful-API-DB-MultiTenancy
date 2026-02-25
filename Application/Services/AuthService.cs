using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly ITenantService _tenantService;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            ITenantService tenantService,
            IPasswordResetRepository passwordResetRepository,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _tenantService = tenantService;
            _passwordResetRepository = passwordResetRepository;
            _logger = logger;
        }

        /// <summary>
        /// Authenticates user and returns JWT token.
        /// </summary>
        public async Task<(bool Success, string Token, string Message)> LoginAsync(string username, string password, string tenantId)
        {
            _logger.LogInformation("Login attempt for user '{Username}' in tenant '{TenantId}'", username, tenantId);

            var user = await _userRepository.GetByUsernameAsync(username, tenantId);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User '{Username}' not found in tenant '{TenantId}'", username, tenantId);
                return (false, string.Empty, "Credenciales inválidas.");
            }

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user '{Username}' in tenant '{TenantId}'", username, tenantId);
                return (false, string.Empty, "Credenciales inválidas.");
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User '{Username}' is inactive in tenant '{TenantId}'", username, tenantId);
                return (false, string.Empty, "Usuario desactivado.");
            }

            var token = _tokenService.GenerateToken(user);
            _logger.LogInformation("Login successful for user '{Username}' in tenant '{TenantId}'", username, tenantId);
            return (true, token, "Login exitoso.");
        }

        /// <summary>
        /// Changes user password. Requires valid current password.
        /// </summary>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var tenantId = _tenantService.GetCurrentTenantId();
            _logger.LogInformation("Password change request for user '{Username}' in tenant '{TenantId}'", username, tenantId);

            var user = await _userRepository.GetByUsernameAsync(username, tenantId);

            if (user == null)
            {
                _logger.LogWarning("Password change failed: User '{Username}' not found", username);
                return (false, "Usuario no encontrado.");
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                _logger.LogWarning("Password change failed: Invalid current password for user '{Username}'", username);
                return (false, "La contraseña actual es incorrecta.");
            }

            if (newPassword.Length < 6)
            {
                return (false, "La nueva contraseña debe tener al menos 6 caracteres.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Password changed successfully for user '{Username}' in tenant '{TenantId}'", username, tenantId);
            return (true, "Contraseña actualizada exitosamente.");
        }

        /// <summary>
        /// Simulates password reset email. Logs the request via Serilog.
        /// </summary>
        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string usernameOrEmail)
        {
            _logger.LogInformation("Password reset request received for '{UsernameOrEmail}'", usernameOrEmail);

            var user = await _userRepository.GetByEmailAsync(usernameOrEmail);

            if (user == null)
            {
                // For security, don't reveal if user exists
                _logger.LogWarning("Password reset requested for non-existent user/email: '{UsernameOrEmail}'", usernameOrEmail);
                return (true, "Si el correo/usuario existe, recibirá instrucciones para restablecer su contraseña.");
            }

            // Generate reset token
            var resetToken = Guid.NewGuid().ToString("N");

            var resetRequest = new PasswordResetRequest
            {
                Username = user.Username,
                Email = user.Email,
                ResetToken = resetToken,
                RequestedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };

            await _passwordResetRepository.CreateAsync(resetRequest);

            // Simulate email sending - logged via Serilog
            _logger.LogInformation(
                "[SIMULATED EMAIL] Password reset email sent to '{Email}' for user '{Username}'. " +
                "Reset Token: {ResetToken}. Expires at: {ExpiresAt}",
                user.Email, user.Username, resetToken, resetRequest.ExpiresAt);

            return (true, "Si el correo/usuario existe, recibirá instrucciones para restablecer su contraseña.");
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        public async Task<(bool Success, string Message)> RegisterAsync(string username, string email, string password, string role, string tenantId)
        {
            _logger.LogInformation("Registration attempt for user '{Username}' in tenant '{TenantId}'", username, tenantId);

            var existingUser = await _userRepository.GetByUsernameAsync(username, tenantId);
            if (existingUser != null)
            {
                return (false, "El nombre de usuario ya existe en este tenant.");
            }

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = role,
                TenantId = tenantId,
                IsActive = true
            };

            await _userRepository.CreateAsync(user);
            _logger.LogInformation("User '{Username}' registered successfully in tenant '{TenantId}'", username, tenantId);

            return (true, "Usuario registrado exitosamente.");
        }
    }
}
