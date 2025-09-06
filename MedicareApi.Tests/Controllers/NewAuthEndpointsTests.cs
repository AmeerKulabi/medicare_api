using MedicareApi.Controllers;
using MedicareApi.Services;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;
using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;

namespace MedicareApi.Tests.Controllers
{
    public class NewAuthEndpointsTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<IConfiguration> _configurationMock;

        public NewAuthEndpointsTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            
            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["Jwt:Key"])
                .Returns("YourTestJwtSecretKeyForTestingPurposesOnly123!");
            _configurationMock.Setup(config => config["Jwt:Issuer"])
                .Returns("medicare.app");
            _configurationMock.Setup(config => config["Jwt:AccessTokenExpirationMinutes"])
                .Returns("10");
        }

        [Fact]
        public async Task ForgotPassword_ValidEmail_ReturnsOk()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@test.com",
                EmailConfirmed = true
            };

            var userManager = CreateMockUserManager();
            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);
            userManager.Setup(um => um.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("reset-token");

            var emailService = new Mock<IEmailService>();
            emailService.Setup(e => e.SendPasswordResetAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var controller = CreateController(userManager.Object, emailService.Object);

            var request = new ForgotPasswordRequest
            {
                Email = "test@test.com"
            };

            // Act
            var result = await controller.ForgotPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Verify email service was called
            emailService.Verify(e => e.SendPasswordResetAsync(
                It.Is<string>(email => email == "test@test.com"), 
                It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ResetPassword_ValidToken_ReturnsOk()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@test.com"
            };

            var userManager = CreateMockUserManager();
            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var emailService = new Mock<IEmailService>();
            var controller = CreateController(userManager.Object, emailService.Object);

            var request = new ResetPasswordRequest
            {
                Email = "test@test.com",
                Token = "valid-token",
                NewPassword = "NewPassword123!"
            };

            // Act
            var result = await controller.ResetPassword(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task ConfirmEmail_ValidToken_ReturnsOk()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "test-user-id",
                Email = "test@test.com"
            };

            var userManager = CreateMockUserManager();
            userManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var emailService = new Mock<IEmailService>();
            var controller = CreateController(userManager.Object, emailService.Object);

            var request = new EmailConfirmationRequest
            {
                UserId = "test-user-id",
                Token = "valid-token"
            };

            // Act
            var result = await controller.ConfirmEmail(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        private Mock<UserManager<ApplicationUser>> CreateMockUserManager()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);
        }

        private AuthController CreateController(UserManager<ApplicationUser> userManager, IEmailService emailService)
        {
            var signInManager = new Mock<SignInManager<ApplicationUser>>(
                userManager, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);

            var controller = new AuthController(userManager, signInManager.Object, _configurationMock.Object, _context, emailService);

            // Setup HTTP context for URL generation
            var httpContext = new Mock<HttpContext>();
            var request = new Mock<HttpRequest>();
            request.Setup(r => r.Scheme).Returns("https");
            request.Setup(r => r.Host).Returns(new HostString("localhost"));
            httpContext.Setup(c => c.Request).Returns(request.Object);

            var controllerContext = new ControllerContext
            {
                HttpContext = httpContext.Object
            };
            controller.ControllerContext = controllerContext;

            return controller;
        }
    }
}