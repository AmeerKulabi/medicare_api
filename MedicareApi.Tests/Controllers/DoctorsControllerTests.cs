using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace MedicareApi.Tests.Controllers
{
    public class DoctorsControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;

        public DoctorsControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _context = CreateTestDbContext();
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
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors(null, null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Equal(2, response.Data.Count);
        }

        [Fact]
        public async Task GetDoctors_WithSpecializationFilter_ReturnsFilteredDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors("Cardiology", null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Data;
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithLocationFilter_ReturnsFilteredDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors(null, "Basra", null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Data;
            Assert.Single(doctors);
            Assert.Equal("Dr. Jane Doe", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithSearchFilter_ReturnsMatchingDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors(null, null, null, "John", null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Data;
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithSpecializationSearch_ReturnsMatchingDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors(null, null, null, "Dermatology", null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Data;
            Assert.Single(doctors);
            Assert.Equal("Dr. Jane Doe", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctorById_ValidId_ReturnsDoctor()
        {
            // Arrange
            var controller = new DoctorsController(_context);

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
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctorById("invalid-id");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        // TODO: UpdateDoctorInfo tests commented out as method doesn't exist yet
        /*
        [Fact]
        public async Task UpdateDoctorInfo_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorsController(_context);
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
        */

        /*
        [Fact]
        public async Task UpdateDoctorInfo_NonDoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorsController(_context);
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
        */

        /*
        [Fact]
        public async Task UpdateDoctorInfo_DoctorNotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = new DoctorsController(_context);
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
            var controller = new DoctorsController(_context);
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
        */

        [Fact]
        public async Task GetDoctors_EnsuresDefaultProfilePicture()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act  
            var result = await controller.GetDoctors(null, null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            var doctors = response.Data;
            
            // All doctors should have a profile picture URL (either set or default)
            Assert.All(doctors, doctor => Assert.NotNull(doctor.ProfilePictureUrl));
        }

        [Fact]
        public async Task GetDoctors_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var emptyContext = CreateTestDbContext(); // Fresh context without data
            var controller = new DoctorsController(emptyContext);

            // Act
            var result = await controller.GetDoctors(null, null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task GetDoctors_MultipleFilters_ReturnsCorrectResults()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors("Cardiology", "Baghdad", null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Data;
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_NoMatchingResults_ReturnsEmptyList()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors("NonExistentSpecialization", null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Data;
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

        [Fact]
        public async Task GetDoctors_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act - Get first page with page size 1
            var result = await controller.GetDoctors(null, null, null, null, null, 1, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Single(response.Data);
            Assert.Equal(2, response.TotalCount);
            Assert.Equal(1, response.PageSize);
            Assert.Equal(1, response.CurrentPage);
            Assert.Equal(2, response.TotalPages);
            Assert.False(response.HasPreviousPage);
            Assert.True(response.HasNextPage);
        }

        [Fact]
        public async Task GetDoctors_WithLanguageFilter_ReturnsFilteredDoctors()
        {
            // Arrange - Add languages to our test doctors
            var doctor1 = await _context.Doctors.FindAsync("doctor-1");
            var doctor2 = await _context.Doctors.FindAsync("doctor-2");
            doctor1!.Languages = new List<string> { "English", "Arabic" };
            doctor2!.Languages = new List<string> { "English", "Kurdish" };
            await _context.SaveChangesAsync();

            var controller = new DoctorsController(_context);

            // Act - Filter by Arabic
            var result = await controller.GetDoctors(null, null, null, null, new List<string> { "Arabic" }, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Single(response.Data);
            Assert.Equal("Dr. John Smith", response.Data.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithMultipleLanguageFilter_ReturnsOnlyDoctorsWithAllLanguages()
        {
            // Arrange - Add languages to our test doctors
            var doctor1 = await _context.Doctors.FindAsync("doctor-1");
            var doctor2 = await _context.Doctors.FindAsync("doctor-2");
            doctor1!.Languages = new List<string> { "English", "Arabic", "French" };
            doctor2!.Languages = new List<string> { "English", "Kurdish" };
            await _context.SaveChangesAsync();

            var controller = new DoctorsController(_context);

            // Act - Filter by English AND Arabic (doctor must speak both)
            var result = await controller.GetDoctors(null, null, null, null, new List<string> { "English", "Arabic" }, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Single(response.Data);
            Assert.Equal("Dr. John Smith", response.Data.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithExperienceSort_SortsByExperience()
        {
            // Arrange - Add experience to our test doctors
            var doctor1 = await _context.Doctors.FindAsync("doctor-1");
            var doctor2 = await _context.Doctors.FindAsync("doctor-2");
            doctor1!.YearsOfExperience = "5 years";
            doctor2!.YearsOfExperience = "10 years";
            await _context.SaveChangesAsync();

            var controller = new DoctorsController(_context);

            // Act - Sort by experience ascending
            var result = await controller.GetDoctors(null, null, "experience_asc", null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Equal(2, response.Data.Count);
            Assert.Equal("Dr. John Smith", response.Data.First().Name); // 5 years first
            Assert.Equal("Dr. Jane Doe", response.Data.Last().Name); // 10 years second
        }

        [Fact]
        public async Task GetDoctors_WithPaginationExceedsMaxPageSize_LimitsToMaxPageSize()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act - Request page size larger than maximum (20)
            var result = await controller.GetDoctors(null, null, null, null, null, 1, 50);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Equal(20, response.PageSize); // Should be limited to max 20
        }

        [Fact]
        public async Task GetLanguages_ReturnsAllUniqueLanguages()
        {
            // Arrange - Add languages to our test doctors
            var doctor1 = await _context.Doctors.FindAsync("doctor-1");
            var doctor2 = await _context.Doctors.FindAsync("doctor-2");
            doctor1!.Languages = new List<string> { "English", "Arabic" };
            doctor2!.Languages = new List<string> { "English", "Kurdish", "French" };
            await _context.SaveChangesAsync();

            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetLanguages();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var languages = Assert.IsType<List<string>>(okResult.Value);
            Assert.Equal(4, languages.Count);
            Assert.Contains("English", languages);
            Assert.Contains("Arabic", languages);
            Assert.Contains("Kurdish", languages);
            Assert.Contains("French", languages);
            // Should be sorted alphabetically
            Assert.Equal("Arabic", languages[0]);
            Assert.Equal("English", languages[1]);
            Assert.Equal("French", languages[2]);
            Assert.Equal("Kurdish", languages[3]);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}