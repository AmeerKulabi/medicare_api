using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedicareApi.Tests.Controllers
{
    public class DoctorsControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<DoctorsController>> _loggerMock;

        public DoctorsControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _context = CreateTestDbContext();
            _loggerMock = new Mock<ILogger<DoctorsController>>();
            SeedTestData().Wait();
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

        private async Task SeedTestData()
        {
            var doctor1 = new Doctor
            {
                Id = "doctor-1",
                UserId = "user-1",
                Name = "Dr. John Smith",
                Email = "john@test.com",
                IsActive = true,
                RegistrationCompleted = true,
                Specialization = "Cardiology",
                Location = "Baghdad",
                Phone = "+96470123456789",
                ClinicName = "Heart Clinic",
                ClinicAddress = "123 Heart Street",
                ConsultationFee = "100",
                ProfilePictureUrl = "test-picture.jpg"
            };

            var doctor2 = new Doctor
            {
                Id = "doctor-2", 
                UserId = "user-2",
                Name = "Dr. Jane Doe",
                Email = "jane@test.com",
                IsActive = true,
                RegistrationCompleted = true,
                Specialization = "Dermatology",
                Location = "Basra",
                Phone = "+96470987654321",
                ClinicName = "Skin Clinic",
                ClinicAddress = "456 Skin Avenue",
                ConsultationFee = "80"
            };

            _context.Doctors.AddRange(doctor1, doctor2);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetDoctors_WithoutFilters_ReturnsAllDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors(null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Equal(2, doctors.Count);
        }

        [Fact]
        public async Task GetDoctors_WithSpecializationFilter_ReturnsFilteredDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors("Cardiology", null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithLocationFilter_ReturnsFilteredDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors(null, "Basra", null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Single(doctors);
            Assert.Equal("Dr. Jane Doe", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithSearchFilter_ReturnsMatchingDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors(null, null, null, "John");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithSpecializationSearch_ReturnsMatchingDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors(null, null, null, "Dermatology");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Single(doctors);
            Assert.Equal("Dr. Jane Doe", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctorById_ValidId_ReturnsDoctor()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctorById("doctor-1");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctor = Assert.IsType<Doctor>(okResult.Value);
            Assert.Equal("Dr. John Smith", doctor.Name);
        }

        [Fact]
        public async Task GetDoctorById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctorById("invalid-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateDoctorInfo_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);
            SetupControllerContextWithoutAuth(controller);
            var formData = new DoctorRegistrationInfo
            {
                Specialization = "Updated Specialization"
            };

            // Act
            var result = await controller.UpdateDoctorInfo(formData, null);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateDoctorInfo_NonDoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);
            SetupControllerContext(controller, "patient-user-id", false);
            var formData = new DoctorRegistrationInfo
            {
                Specialization = "Updated Specialization"
            };

            // Act
            var result = await controller.UpdateDoctorInfo(formData, null);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateDoctorInfo_DoctorNotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);
            SetupControllerContext(controller, "non-existent-user", true);
            var formData = new DoctorRegistrationInfo
            {
                Specialization = "Updated Specialization"
            };

            // Act
            var result = await controller.UpdateDoctorInfo(formData, null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateDoctorInfo_ValidDoctor_UpdatesSuccessfully()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);
            SetupControllerContext(controller, "user-1", true);
            var formData = new DoctorRegistrationInfo
            {
                Specialization = "Updated Cardiology",
                MedicalLicense = "ML123456",
                LicenseState = "Baghdad",
                LicenseExpiry = "2025-12-31",
                BoardCertification = "Board Certified",
                YearsOfExperience = "10",
                MedicalSchool = "University of Baghdad"
            };

            // Act
            var result = await controller.UpdateDoctorInfo(formData, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedDoctor = Assert.IsType<Doctor>(okResult.Value);
            Assert.Equal("Updated Cardiology", updatedDoctor.Specialization);
            Assert.Equal("ML123456", updatedDoctor.MedicalLicense);
            Assert.Equal("Baghdad", updatedDoctor.LicenseState);
            
            // Verify data was persisted
            var doctorFromDb = await _context.Doctors.FindAsync("doctor-1");
            Assert.NotNull(doctorFromDb);
            Assert.Equal("Updated Cardiology", doctorFromDb.Specialization);
        }

        [Fact]
        public async Task GetDoctors_EnsuresDefaultProfilePicture()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act  
            var result = await controller.GetDoctors(null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            
            // All doctors should have a profile picture URL (either set or default)
            Assert.All(doctors, doctor => Assert.NotNull(doctor.ProfilePictureUrl));
        }

        [Fact]
        public async Task GetDoctors_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var emptyContext = CreateTestDbContext(); // Fresh context without data
            var controller = new DoctorsController(emptyContext, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors(null, null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Empty(doctors);
        }

        [Fact]
        public async Task GetDoctors_MultipleFilters_ReturnsCorrectResults()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors("Cardiology", "Baghdad", null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_NoMatchingResults_ReturnsEmptyList()
        {
            // Arrange
            var controller = new DoctorsController(_context, _loggerMock.Object);

            // Act
            var result = await controller.GetDoctors("NonExistentSpecialization", null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var doctors = Assert.IsAssignableFrom<List<Doctor>>(okResult.Value);
            Assert.Empty(doctors);
        }

        private void SetupControllerContext(ControllerBase controller, string userId, bool isDoctor)
        {
            var claims = new List<Claim>
            {
                new Claim("uid", userId),
                new Claim("isDoctor", isDoctor.ToString())
            };
            
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        private void SetupControllerContextWithoutAuth(ControllerBase controller)
        {
            var identity = new ClaimsIdentity(); // Empty identity (no claims)
            var principal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}