using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<ITenantService> _tenantServiceMock;
        private readonly Mock<IPasswordResetRepository> _passwordResetRepositoryMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _tokenServiceMock = new Mock<ITokenService>();
            _tenantServiceMock = new Mock<ITenantService>();
            _passwordResetRepositoryMock = new Mock<IPasswordResetRepository>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            _authService = new AuthService(
                _userRepositoryMock.Object,
                _tokenServiceMock.Object,
                _tenantServiceMock.Object,
                _passwordResetRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsToken()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                TenantId = "tenant-a",
                Role = "Admin",
                Email = "admin@test.com",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("admin", "tenant-a"))
                .ReturnsAsync(user);

            _tokenServiceMock
                .Setup(t => t.GenerateToken(It.IsAny<User>()))
                .Returns("fake-jwt-token");

            // Act
            var (success, token, message) = await _authService.LoginAsync("admin", "Admin123!", "tenant-a");

            // Assert
            Assert.True(success);
            Assert.Equal("fake-jwt-token", token);
            Assert.Equal("Login exitoso.", message);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsFailure()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                TenantId = "tenant-a",
                Role = "Admin",
                Email = "admin@test.com",
                IsActive = true
            };

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("admin", "tenant-a"))
                .ReturnsAsync(user);

            // Act
            var (success, token, message) = await _authService.LoginAsync("admin", "WrongPassword", "tenant-a");

            // Assert
            Assert.False(success);
            Assert.Equal(string.Empty, token);
            Assert.Equal("Credenciales inválidas.", message);
        }

        [Fact]
        public async Task Login_WithNonExistentUser_ReturnsFailure()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("nouser", "tenant-a"))
                .ReturnsAsync((User?)null);

            // Act
            var (success, token, message) = await _authService.LoginAsync("nouser", "Password", "tenant-a");

            // Assert
            Assert.False(success);
            Assert.Equal("Credenciales inválidas.", message);
        }

        [Fact]
        public async Task ChangePassword_WithValidCurrentPassword_Succeeds()
        {
            // Arrange
            _tenantServiceMock.Setup(t => t.GetCurrentTenantId()).Returns("tenant-a");

            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("OldPass123"),
                TenantId = "tenant-a",
                Role = "Admin",
                Email = "admin@test.com"
            };

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("admin", "tenant-a"))
                .ReturnsAsync(user);

            // Act
            var (success, message) = await _authService.ChangePasswordAsync("admin", "OldPass123", "NewPass456");

            // Assert
            Assert.True(success);
            Assert.Equal("Contraseña actualizada exitosamente.", message);
            _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task ChangePassword_WithInvalidCurrentPassword_Fails()
        {
            // Arrange
            _tenantServiceMock.Setup(t => t.GetCurrentTenantId()).Returns("tenant-a");

            var user = new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPass"),
                TenantId = "tenant-a",
                Email = "admin@test.com"
            };

            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("admin", "tenant-a"))
                .ReturnsAsync(user);

            // Act
            var (success, message) = await _authService.ChangePasswordAsync("admin", "WrongPass", "NewPass456");

            // Assert
            Assert.False(success);
            Assert.Equal("La contraseña actual es incorrecta.", message);
        }

        [Fact]
        public async Task ForgotPassword_WithExistingUser_CreatesResetRequest()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@test.com",
                TenantId = "tenant-a"
            };

            _userRepositoryMock
                .Setup(r => r.GetByEmailAsync("admin@test.com"))
                .ReturnsAsync(user);

            _passwordResetRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<PasswordResetRequest>()))
                .ReturnsAsync(new PasswordResetRequest());

            // Act
            var (success, message) = await _authService.ForgotPasswordAsync("admin@test.com");

            // Assert
            Assert.True(success);
            _passwordResetRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<PasswordResetRequest>()), Times.Once);
        }

        [Fact]
        public async Task Register_NewUser_Succeeds()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("newuser", "tenant-a"))
                .ReturnsAsync((User?)null);

            _userRepositoryMock
                .Setup(r => r.CreateAsync(It.IsAny<User>()))
                .ReturnsAsync(new User { Id = 5, Username = "newuser" });

            // Act
            var (success, message) = await _authService.RegisterAsync(
                "newuser", "new@test.com", "Pass123!", "User", "tenant-a");

            // Assert
            Assert.True(success);
            Assert.Equal("Usuario registrado exitosamente.", message);
        }

        [Fact]
        public async Task Register_ExistingUser_Fails()
        {
            // Arrange
            _userRepositoryMock
                .Setup(r => r.GetByUsernameAsync("admin", "tenant-a"))
                .ReturnsAsync(new User { Username = "admin", TenantId = "tenant-a" });

            // Act
            var (success, message) = await _authService.RegisterAsync(
                "admin", "admin@test.com", "Pass123!", "User", "tenant-a");

            // Assert
            Assert.False(success);
            Assert.Equal("El nombre de usuario ya existe en este tenant.", message);
        }
    }
}
