using Api.Models;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Autentica un usuario y devuelve un token JWT.
        /// </summary>
        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, token, message) = await _authService.LoginAsync(
                request.Username, request.Password, request.TenantId);

            if (!success)
                return Unauthorized(new LoginResponse { Success = false, Message = message });

            return Ok(new LoginResponse { Success = true, Token = token, Message = message });
        }

        /// <summary>
        /// Cambia la contraseña del usuario. Requiere autenticación JWT.
        /// </summary>
        [HttpPost("CambioDeClave")]
        [Authorize]
        public async Task<IActionResult> CambioDeClave([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _authService.ChangePasswordAsync(
                request.Username, request.CurrentPassword, request.NewPassword);

            if (!success)
                return BadRequest(new ApiResponse { Success = false, Message = message });

            return Ok(new ApiResponse { Success = true, Message = message });
        }

        /// <summary>
        /// Simula el envío de un correo para restablecer la contraseña.
        /// La solicitud se registra con Serilog.
        /// </summary>
        [HttpPost("OlvideMiClave")]
        [AllowAnonymous]
        public async Task<IActionResult> OlvideMiClave([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _authService.ForgotPasswordAsync(request.UsernameOrEmail);

            return Ok(new ApiResponse { Success = success, Message = message });
        }

        /// <summary>
        /// Registra un nuevo usuario en el sistema.
        /// </summary>
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message) = await _authService.RegisterAsync(
                request.Username, request.Email, request.Password, request.Role, request.TenantId);

            if (!success)
                return BadRequest(new ApiResponse { Success = false, Message = message });

            return Ok(new ApiResponse { Success = true, Message = message });
        }
    }
}
