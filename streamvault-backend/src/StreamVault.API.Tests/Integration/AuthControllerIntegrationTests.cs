using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;
using StreamVault.API;
using StreamVault.Infrastructure.Data;
using StreamVault.Domain.Entities;

namespace StreamVault.API.Tests.Integration
{
    public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StreamVaultDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<StreamVaultDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("StreamVaultTestDb");
                    });

                    // Initialize the database
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<StreamVaultDbContext>();

                    db.Database.EnsureCreated();
                    InitializeTestData(db);
                });
            });

            _client = _factory.CreateClient();
        }

        private void InitializeTestData(StreamVaultDbContext context)
        {
            // Create test tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Test Tenant",
                Slug = "test-tenant",
                ContactEmail = "admin@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);

            // Create test user
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FirstName = "Test",
                LastName = "User",
                TenantId = tenant.Id,
                Roles = new List<string> { "User" },
                IsActive = true,
                EmailVerified = true,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);

            context.SaveChanges();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ReturnsSuccess()
        {
            // Arrange
            var loginRequest = new
            {
                email = "test@example.com",
                password = "password123",
                tenantSlug = "test-tenant"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.NotNull(result.User);
            Assert.Equal("test@example.com", result.User.Email);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginRequest = new
            {
                email = "test@example.com",
                password = "wrongpassword",
                tenantSlug = "test-tenant"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Register_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var registerRequest = new
            {
                email = "newuser@example.com",
                password = "password123",
                firstName = "New",
                lastName = "User",
                tenantSlug = "test-tenant"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
            
            Assert.NotNull(result);
            Assert.NotNull(result.Message);
            Assert.Equal("Registration successful. Please check your email to verify your account.", result.Message);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsBadRequest()
        {
            // Arrange
            var registerRequest = new
            {
                email = "test@example.com", // Already exists
                password = "password123",
                firstName = "Test",
                lastName = "User",
                tenantSlug = "test-tenant"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
        {
            // First login to get tokens
            var loginRequest = new
            {
                email = "test@example.com",
                password = "password123",
                tenantSlug = "test-tenant"
            };
            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var loginResult = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

            // Arrange
            var refreshRequest = new
            {
                refreshToken = loginResult!.RefreshToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<RefreshTokenResponse>();
            
            Assert.NotNull(result);
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.NotEqual(loginResult.AccessToken, result.AccessToken);
        }

        [Fact]
        public async Task VerifyEmail_WithValidToken_VerifiesEmail()
        {
            // First register a new user to get verification token
            var registerRequest = new
            {
                email = "verify@example.com",
                password = "password123",
                firstName = "Verify",
                lastName = "User",
                tenantSlug = "test-tenant"
            };
            await _client.PostAsJsonAsync("/api/auth/register", registerRequest);

            // Get the user from the database to get the verification token
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "verify@example.com");

            // Arrange
            var verifyRequest = new
            {
                userId = user!.Id,
                token = user.VerificationToken
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/verify-email", verifyRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<VerifyEmailResponse>();
            
            Assert.NotNull(result);
            Assert.Equal("Email verified successfully", result.Message);
        }

        [Fact]
        public async Task ForgotPassword_WithValidEmail_SendsResetEmail()
        {
            // Arrange
            var forgotPasswordRequest = new
            {
                email = "test@example.com"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ForgotPasswordResponse>();
            
            Assert.NotNull(result);
            Assert.Equal("Password reset email sent", result.Message);
        }

        [Fact]
        public async Task ResetPassword_WithValidToken_ResetsPassword()
        {
            // First trigger forgot password to get reset token
            var forgotPasswordRequest = new
            {
                email = "test@example.com"
            };
            await _client.PostAsJsonAsync("/api/auth/forgot-password", forgotPasswordRequest);

            // Get the user from the database to get the reset token
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");

            // Arrange
            var resetPasswordRequest = new
            {
                token = user!.PasswordResetToken,
                newPassword = "newpassword123"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/reset-password", resetPasswordRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ResetPasswordResponse>();
            
            Assert.NotNull(result);
            Assert.Equal("Password reset successfully", result.Message);
        }
    }

    public class VideoControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly string _authToken;

        public VideoControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<StreamVaultDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<StreamVaultDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("StreamVaultVideoTestDb");
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<StreamVaultDbContext>();

                    db.Database.EnsureCreated();
                    InitializeTestData(db);
                });
            });

            _client = _factory.CreateClient();
            _authToken = GetAuthToken().Result;
        }

        private async Task<string> GetAuthToken()
        {
            var loginRequest = new
            {
                email = "test@example.com",
                password = "password123",
                tenantSlug = "test-tenant"
            };
            var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            return result!.AccessToken;
        }

        private void InitializeTestData(StreamVaultDbContext context)
        {
            // Create test tenant and user (same as auth tests)
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Test Tenant",
                Slug = "test-tenant",
                ContactEmail = "admin@test.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Tenants.Add(tenant);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                FirstName = "Test",
                LastName = "User",
                TenantId = tenant.Id,
                Roles = new List<string> { "User" },
                IsActive = true,
                EmailVerified = true,
                TwoFactorEnabled = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Users.Add(user);

            // Create test video
            var video = new Video
            {
                Id = Guid.NewGuid(),
                Title = "Test Video",
                Description = "Test Description",
                TenantId = tenant.Id,
                UserId = user.Id,
                Status = VideoStatus.Ready,
                Visibility = VideoVisibility.Public,
                FileSize = 1024000,
                Duration = TimeSpan.FromMinutes(5),
                ViewCount = 100,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.Videos.Add(video);

            context.SaveChanges();
        }

        [Fact]
        public async Task GetVideos_WithAuthentication_ReturnsVideos()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

            // Act
            var response = await _client.GetAsync("/api/videos");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<VideoListResponse>();
            
            Assert.NotNull(result);
            Assert.NotNull(result.Videos);
            Assert.Single(result.Videos);
            Assert.Equal("Test Video", result.Videos[0].Title);
        }

        [Fact]
        public async Task GetVideos_WithoutAuthentication_ReturnsUnauthorized()
        {
            // Act
            var response = await _client.GetAsync("/api/videos");

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task CreateVideo_WithValidData_ReturnsCreatedVideo()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

            var createRequest = new
            {
                title = "New Test Video",
                description = "New Test Description",
                visibility = "public",
                tags = new[] { "test", "new" }
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/videos", createRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<VideoResponse>();
            
            Assert.NotNull(result);
            Assert.Equal("New Test Video", result.Title);
            Assert.Equal("New Test Description", result.Description);
        }

        [Fact]
        public async Task GetVideo_WithValidId_ReturnsVideo()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

            // Get the video ID from the database
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
            var video = await db.Videos.FirstAsync();

            // Act
            var response = await _client.GetAsync($"/api/videos/{video.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<VideoResponse>();
            
            Assert.NotNull(result);
            Assert.Equal(video.Id, result.Id);
            Assert.Equal("Test Video", result.Title);
        }

        [Fact]
        public async Task UpdateVideo_WithValidData_UpdatesVideo()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
            var video = await db.Videos.FirstAsync();

            var updateRequest = new
            {
                title = "Updated Test Video",
                description = "Updated Test Description"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/videos/{video.Id}", updateRequest);

            // Assert
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<VideoResponse>();
            
            Assert.NotNull(result);
            Assert.Equal("Updated Test Video", result.Title);
            Assert.Equal("Updated Test Description", result.Description);
        }

        [Fact]
        public async Task DeleteVideo_WithValidId_DeletesVideo()
        {
            // Arrange
            _client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<StreamVaultDbContext>();
            var video = await db.Videos.FirstAsync();

            // Act
            var response = await _client.DeleteAsync($"/api/videos/{video.Id}");

            // Assert
            response.EnsureSuccessStatusCode();
            
            // Verify video is marked as deleted
            var deletedVideo = await db.Videos.FindAsync(video.Id);
            Assert.True(deletedVideo.IsDeleted);
        }
    }

    // Response DTOs
    public class LoginResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public UserResponse User { get; set; } = null!;
    }

    public class UserResponse
    {
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
    }

    public class RegisterResponse
    {
        public string Message { get; set; } = null!;
    }

    public class RefreshTokenResponse
    {
        public string AccessToken { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
    }

    public class VerifyEmailResponse
    {
        public string Message { get; set; } = null!;
    }

    public class ForgotPasswordResponse
    {
        public string Message { get; set; } = null!;
    }

    public class ResetPasswordResponse
    {
        public string Message { get; set; } = null!;
    }

    public class VideoListResponse
    {
        public List<VideoResponse> Videos { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public class VideoResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string Visibility { get; set; } = null!;
        public long FileSize { get; set; }
        public TimeSpan Duration { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
