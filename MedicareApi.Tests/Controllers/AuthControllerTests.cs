using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using MedicareApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using RegisterRequest = MedicareApi.ViewModels.RegisterRequest;
using LoginRequest = MedicareApi.ViewModels.LoginRequest;

namespace MedicareApi.Tests.Controllers
{
    public class AuthControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Mock<IConfiguration> _configurationMock;

        public AuthControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _context = CreateTestDbContext();
            _userManager = CreateTestUserManager();
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

        private UserManager<ApplicationUser> CreateTestUserManager()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var keyNormalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var serviceProvider = new Mock<IServiceProvider>();
            var logger = new Mock<Microsoft.Extensions.Logging.ILogger<UserManager<ApplicationUser>>>();

            return new UserManager<ApplicationUser>(
                userStore.Object,
                null,
                passwordHasher.Object,
                userValidators,
                passwordValidators,
                keyNormalizer.Object,
                errors.Object,
                serviceProvider.Object,
                logger.Object
            );
        }

        private AuthController CreateAuthController(UserManager<ApplicationUser>? userManager = null)
        {
            var um = userManager ?? new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null).Object;
            var signInManager = new Mock<SignInManager<ApplicationUser>>(
                um, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            var emailService = new Mock<IEmailService>();
            var webHostEnvironment = new Mock<IWebHostEnvironment>();
            
            var controller = new AuthController(um, signInManager.Object, _configurationMock.Object, _context, emailService.Object, webHostEnvironment.Object);
            
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
            
            // Setup URL helper
            var urlHelper = new Mock<IUrlHelper>();
            urlHelper.Setup(u => u.Action(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns("https://localhost/confirm-email?userId=test&token=test");
            controller.Url = urlHelper.Object;
            
            return controller;
        }

        private void SetupConfiguration()
        {
            _configurationMock.Setup(config => config["Jwt:Key"])
                .Returns("YourTestJwtSecretKeyForTestingPurposesOnly123!");
            _configurationMock.Setup(config => config["Jwt:Issuer"])
                .Returns("medicare.app");

            // This mocks the indexer, but not GetValue
            _configurationMock.SetupGet(config => config["Jwt:AccessTokenExpirationMinutes"])
                .Returns("10");
        }

        [Fact]
        public async Task Register_ValidInput_ReturnsOkWithConfirmationMessage()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            userManager.Setup(um => um.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync("sample-token");

            var controller = CreateAuthController(userManager.Object);

            var request = new RegisterRequest
            {
                Email = "newuser@test.com",
                FullName = "New User",
                Password = "TestPass123!",
                IsDoctor = false,
                Phone = "+96470123456789"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = okResult.Value;
            Assert.NotNull(response);
            // Check that the response contains the message and userId
            var messageProperty = response.GetType().GetProperty("message");
            Assert.NotNull(messageProperty);
            var message = messageProperty.GetValue(response) as string;
            Assert.Contains("check your email", message);
        }

        [Fact]
        public async Task Register_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateAuthController();
            controller.ModelState.AddModelError("Email", "Email is required");

            var request = new RegisterRequest();

            // Act
            var result = await controller.Register(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Register_UserCreationFails_ReturnsBadRequest()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            var identityError = new IdentityError { Code = "DuplicateEmail", Description = "Email already exists" };
            userManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(identityError));

            var controller = CreateAuthController(userManager.Object);

            var request = new RegisterRequest
            {
                Email = "existing@test.com",
                FullName = "Existing User",
                Password = "TestPass123!",
                IsDoctor = false,
                Phone = "+96470123456789"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequestResult.Value);
        }

        [Fact]
        public async Task Register_DoctorUser_CreatesDocumentRecord()
        {
            // Arrange
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            var user = new ApplicationUser { Id = "test-doctor-user-id" };
            userManager.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<ApplicationUser, string>((u, p) => u.Id = user.Id);

            var controller = CreateAuthController(userManager.Object);

            var request = new RegisterRequest
            {
                Email = "doctor@test.com",
                FullName = "Dr. Test",
                Password = "TestPass123!",
                IsDoctor = true,
                Phone = "+96470123456789"
            };

            // Act
            var result = await controller.Register(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<RegisterResponse>(okResult.Value);
            Assert.True(response.IsDoctor);

            // Verify doctor record was created
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == request.Email);
            Assert.NotNull(doctor);
            Assert.Equal(request.FullName, doctor.Name);
            Assert.False(doctor.IsActive);
            Assert.False(doctor.RegistrationCompleted);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOkWithToken()
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
            userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            userManager.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var controller = CreateAuthController(userManager.Object);

            var request = new LoginRequest
            {
                Email = "test@test.com",
                Password = "TestPass123!"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.NotEmpty(response.Token);
            Assert.Equal(user.Id, response.UserId);
            Assert.False(response.IsDoctor);
        }

        [Fact]
        public async Task Login_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateAuthController();
            controller.ModelState.AddModelError("Email", "Email is required");

            var request = new LoginRequest();

            // Act
            var result = await controller.Login(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
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
            Assert.IsType<UnauthorizedObjectResult>(result);
        }

        [Fact]
        public async Task Login_DoctorUser_ReturnsCorrectFlags()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "doctor-user-id",
                Email = "doctor@test.com",
                IsDoctor = true
            };

            var doctor = new Doctor
            {
                Id = "doctor-id",
                UserId = user.Id,
                Name = "Dr. Test",
                Email = user.Email,
                IsActive = true,
                RegistrationCompleted = true
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            userManager.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);

            var controller = CreateAuthController(userManager.Object);

            var request = new LoginRequest
            {
                Email = "doctor@test.com",
                Password = "TestPass123!"
            };

            var result = await controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.True(response.IsDoctor);
            Assert.True(response.IsActive);
            Assert.True(response.RegistrationCompleted);
        }

        [Fact]
        public async Task Login_DoctorNotInDatabase_ReturnsNotFound()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = "missing-doctor-user-id",
                Email = "missing-doctor@test.com",
                IsDoctor = true
            };

            var userStore = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            userManager.Setup(um => um.IsEmailConfirmedAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(true);
            userManager.Setup(um => um.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            var controller = CreateAuthController(userManager.Object);

            var request = new LoginRequest
            {
                Email = "missing-doctor@test.com",
                Password = "TestPass123!"
            };

            // Act
            var result = await controller.Login(request);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}