using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using StreamVault.Application.Services;
using StreamVault.Domain.Entities;
using StreamVault.Infrastructure.Data;
using StreamVault.Infrastructure.Options;

namespace StreamVault.Application.Tests.Services
{
    public class AuthServiceTests
    {
        private readonly Mock<StreamVaultDbContext> _mockDbContext;
        private readonly Mock<IJwtService> _mockJwtService;
        private readonly Mock<IEmailService> _mockEmailService;
        private readonly Mock<ITwoFactorService> _mockTwoFactorService;
        private readonly Mock<IOptions<JwtOptions>> _mockJwtOptions;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockDbContext = new Mock<StreamVaultDbContext>();
            _mockJwtService = new Mock<IJwtService>();
            _mockEmailService = new Mock<IEmailService>();
            _mockTwoFactorService = new Mock<ITwoFactorService>();
            _mockJwtOptions = new Mock<IOptions<JwtOptions>>();

            _mockJwtOptions.Setup(x => x.Value).Returns(new JwtOptions
            {
                SecretKey = "test-secret-key-with-sufficient-length",
                Issuer = "StreamVault",
                Audience = "StreamVault",
                ExpiryMinutes = 60,
                RefreshTokenExpiryDays = 7
            });

            _authService = new AuthService(
                _mockDbContext.Object,
                _mockJwtService.Object,
                _mockEmailService.Object,
                _mockTwoFactorService.Object,
                _mockJwtOptions.Object
            );
        }

        [Fact]
        public async Task LoginAsync_WithValidCredentials_ReturnsTokens()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = true,
                EmailVerified = true,
                TwoFactorEnabled = false
            };

            var mockUsers = CreateMockDbSet(new[] { user });
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var expectedTokens = new TokenResponse
            {
                AccessToken = "access-token",
                RefreshToken = "refresh-token"
            };

            _mockJwtService.Setup(x => x.GenerateTokens(user))
                .ReturnsAsync(expectedTokens);

            // Act
            var result = await _authService.LoginAsync(email, password);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTokens.AccessToken, result.AccessToken);
            Assert.Equal(expectedTokens.RefreshToken, result.RefreshToken);
        }

        [Fact]
        public async Task LoginAsync_WithInvalidPassword_ThrowsException()
        {
            // Arrange
            var email = "test@example.com";
            var password = "wrong-password";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct-password"),
                IsActive = true,
                EmailVerified = true
            };

            var mockUsers = CreateMockDbSet(new[] { user });
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(email, password));
            Assert.Equal("Invalid credentials", exception.Message);
        }

        [Fact]
        public async Task LoginAsync_WithInactiveUser_ThrowsException()
        {
            // Arrange
            var email = "test@example.com";
            var password = "password123";
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                IsActive = false,
                EmailVerified = true
            };

            var mockUsers = CreateMockDbSet(new[] { user });
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _authService.LoginAsync(email, password));
            Assert.Equal("Account is disabled", exception.Message);
        }

        [Fact]
        public async Task RegisterAsync_WithValidData_CreatesUserAndSendsVerification()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "newuser@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe",
                TenantSlug = "test-tenant"
            };

            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Slug = "test-tenant",
                IsActive = true
            };

            var mockTenants = CreateMockDbSet(new[] { tenant });
            var mockUsers = CreateMockDbSet(Array.Empty<User>());

            _mockDbContext.Setup(x => x.Tenants).Returns(mockTenants.Object);
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _authService.RegisterAsync(request);

            // Assert
            _mockDbContext.Verify(x => x.Users.Add(It.IsAny<User>()), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _mockEmailService.Verify(x => x.SendVerificationEmailAsync(
                request.Email, 
                It.IsAny<string>(), 
                It.IsAny<string>()), 
                Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateEmail_ThrowsException()
        {
            // Arrange
            var request = new RegisterRequest
            {
                Email = "existing@example.com",
                Password = "password123",
                FirstName = "John",
                LastName = "Doe"
            };

            var existingUser = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = "hash"
            };

            var mockUsers = CreateMockDbSet(new[] { existingUser });
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _authService.RegisterAsync(request));
            Assert.Equal("Email already exists", exception.Message);
        }

        [Fact]
        public async Task RefreshTokenAsync_WithValidToken_ReturnsNewTokens()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var refreshToken = "valid-refresh-token";
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                IsActive = true
            };

            var mockUsers = CreateMockDbSet(new[] { user });
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var expectedTokens = new TokenResponse
            {
                AccessToken = "new-access-token",
                RefreshToken = "new-refresh-token"
            };

            _mockJwtService.Setup(x => x.ValidateRefreshToken(refreshToken))
                .ReturnsAsync(userId);
            _mockJwtService.Setup(x => x.GenerateTokens(user))
                .ReturnsAsync(expectedTokens);

            // Act
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTokens.AccessToken, result.AccessToken);
            Assert.Equal(expectedTokens.RefreshToken, result.RefreshToken);
        }

        [Fact]
        public async Task VerifyEmailAsync_WithValidToken_VerifiesUserEmail()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var token = "verification-token";
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                EmailVerified = false,
                VerificationToken = token
            };

            var mockUsers = CreateMockDbSet(new[] { user });
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _authService.VerifyEmailAsync(userId, token);

            // Assert
            Assert.True(user.EmailVerified);
            Assert.Null(user.VerificationToken);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task EnableTwoFactorAsync_WithValidUser_GeneratesSecret()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "test@example.com",
                TwoFactorEnabled = false
            };

            var mockUsers = CreateMockDbSet(new[] { user });
            _mockDbContext.Setup(x => x.Users).Returns(mockUsers.Object);

            var expectedSecret = "test-secret";
            var expectedQrCode = "data:image/png;base64,test";

            _mockTwoFactorService.Setup(x => x.GenerateSecret())
                .Returns(expectedSecret);
            _mockTwoFactorService.Setup(x => x.GenerateQrCode(user.Email, expectedSecret))
                .Returns(expectedQrCode);

            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _authService.EnableTwoFactorAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSecret, result.Secret);
            Assert.Equal(expectedQrCode, result.QrCode);
            Assert.Equal(expectedSecret, user.TwoFactorSecret);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IEnumerable<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            mockSet.Setup(m => m.Add(It.IsAny<T>())).Callback<T>(data.Add);
            mockSet.Setup(m => m.Remove(It.IsAny<T>())).Callback<T>(item => data = data.Where(x => !x.Equals(item)));

            return mockSet;
        }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? TenantSlug { get; set; }
    }

    public interface IJwtService
    {
        Task<TokenResponse> GenerateTokens(User user);
        Task<Guid> ValidateRefreshToken(string refreshToken);
    }

    public interface IEmailService
    {
        Task SendVerificationEmailAsync(string email, string firstName, string verificationLink);
    }

    public interface ITwoFactorService
    {
        string GenerateSecret();
        string GenerateQrCode(string email, string secret);
    }

    public class JwtOptions
    {
        public string SecretKey { get; set; } = null!;
        public string Issuer { get; set; } = null!;
        public string Audience { get; set; } = null!;
        public int ExpiryMinutes { get; set; }
        public int RefreshTokenExpiryDays { get; set; }
    }

    public class TwoFactorSetupResponse
    {
        public string Secret { get; set; } = null!;
        public string QrCode { get; set; } = null!;
        public string BackupCodes { get; set; } = null!;
    }
}
