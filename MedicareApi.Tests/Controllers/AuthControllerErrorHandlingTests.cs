using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace MedicareApi.Tests.Controllers
{
    public class AuthControllerErrorHandlingTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly Mock<IConfiguration> _configurationMock;

        public AuthControllerErrorHandlingTests(TestFixture fixture)
        {
            _fixture = fixture;
            _context = CreateTestDbContext();
            _configurationMock = new Mock<IConfiguration>();
            SetupConfiguration();
        }

        private ApplicationDbContext CreateTestDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        private void SetupConfiguration()
        {
            _configurationMock.Setup(c => c["Jwt:Key"]).Returns("TestSecretKeyForJWTTokenGenerationThatIsLongEnough123");
            _configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("test-issuer");
            _configurationMock.Setup(c => c["Jwt:AccessTokenExpirationMinutes"]).Returns("30");
        }

        private AuthController CreateAuthController(UserManager<ApplicationUser> userManager = null!)
        {
            if (userManager == null)
            {
                userManager = CreateTestUserManager();
            }

            var signInManager = CreateTestSignInManager(userManager);
            return new AuthController(userManager, signInManager, _configurationMock.Object, _context);
        }

        private UserManager<ApplicationUser> CreateTestUserManager()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null).Object;
        }

        private SignInManager<ApplicationUser> CreateTestSignInManager(UserManager<ApplicationUser> userManager)
        {
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            var contextFactory = new Mock<IUserClaimsPrincipalFactory<ApplicationUser>>();
            return new Mock<SignInManager<ApplicationUser>>(
                userManager, httpContextAccessor.Object, contextFactory.Object, null, null, null, null).Object;
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsNotFoundWithErrorCode()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null!);

            var controller = CreateAuthController(userManager.Object);
            var request = new LoginRequest
            {
                Email = "nonexistent@test.com",
                Password = "TestPass123!"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            var errorResponse = Assert.IsType<ApiErrorResponse>(notFoundResult.Value);
            Assert.Equal(ErrorCodes.USER_NOT_FOUND, errorResponse.ErrorCode);
            Assert.Equal("No account found with this email address", errorResponse.Message);
            Assert.NotNull(errorResponse.Details);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorizedWithErrorCode()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@test.com",
                IsDoctor = false
            };

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.IsLockedOutAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(false);
            userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var controller = CreateAuthController(userManager.Object);
            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "WrongPassword"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var errorResponse = Assert.IsType<ApiErrorResponse>(unauthorizedResult.Value);
            Assert.Equal(ErrorCodes.INVALID_PASSWORD, errorResponse.ErrorCode);
            Assert.Equal("Incorrect password", errorResponse.Message);
            Assert.NotNull(errorResponse.Details);
        }

        [Fact]
        public async Task Login_AccountLocked_ReturnsUnauthorizedWithLockoutInfo()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "locked@test.com",
                IsDoctor = false,
                LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15)
            };

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.IsLockedOutAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var controller = CreateAuthController(userManager.Object);
            var request = new LoginRequest
            {
                Email = "locked@test.com",
                Password = "TestPass123!"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            var errorResponse = Assert.IsType<ApiErrorResponse>(unauthorizedResult.Value);
            Assert.Equal(ErrorCodes.ACCOUNT_LOCKED, errorResponse.ErrorCode);
            Assert.Equal("Account is temporarily locked due to too many failed login attempts", errorResponse.Message);
            Assert.Contains("Please try again after", errorResponse.Details);
        }

        [Fact]
        public async Task Login_ValidationError_ReturnsBadRequestWithValidationDetails()
        {
            // Arrange
            var controller = CreateAuthController();
            controller.ModelState.AddModelError("Email", "Email is required");
            controller.ModelState.AddModelError("Password", "Password is required");

            var request = new LoginRequest();

            // Act
            var result = await controller.Login(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
            Assert.Equal(ErrorCodes.VALIDATION_ERROR, errorResponse.ErrorCode);
            Assert.Equal("Invalid request data", errorResponse.Message);
            Assert.Contains("Email is required", errorResponse.Details);
            Assert.Contains("Password is required", errorResponse.Details);
        }

        [Fact]
        public async Task Register_EmailAlreadyExists_ReturnsConflictWithErrorCode()
        {
            // Arrange
            var existingUser = new ApplicationUser
            {
                Id = "existing-user-id",
                Email = "existing@test.com"
            };

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(existingUser);

            var controller = CreateAuthController(userManager.Object);
            var request = new RegisterRequest
            {
                Email = "existing@test.com",
                Password = "TestPass123!",
                FullName = "Test User",
                Phone = "+9641234567890",
                IsDoctor = false
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            var errorResponse = Assert.IsType<ApiErrorResponse>(conflictResult.Value);
            Assert.Equal(ErrorCodes.EMAIL_ALREADY_EXISTS, errorResponse.ErrorCode);
            Assert.Equal("An account with this email address already exists", errorResponse.Message);
        }

        [Fact] 
        public async Task Register_WeakPassword_ReturnsBadRequestWithWeakPasswordError()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((ApplicationUser)null!);

            var identityErrors = new[]
            {
                new IdentityError { Description = "Passwords must be at least 6 characters." }
            };
            var identityResult = IdentityResult.Failed(identityErrors);

            userManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(identityResult);

            var controller = CreateAuthController(userManager.Object);
            var request = new RegisterRequest
            {
                Email = "test@test.com",
                Password = "123",
                FullName = "Test User", 
                Phone = "+9641234567890",
                IsDoctor = false
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            var errorResponse = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
            Assert.Equal(ErrorCodes.WEAK_PASSWORD, errorResponse.ErrorCode);
            Assert.Equal("Registration failed", errorResponse.Message);
            Assert.Contains("Passwords must be at least 6 characters", errorResponse.Details);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}