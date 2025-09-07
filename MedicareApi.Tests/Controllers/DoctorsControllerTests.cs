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
                Phone = "+96470987654321"
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
            var result = await controller.GetDoctors(null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Equal(2, response.Doctors.Count);
        }

        [Fact]
        public async Task GetDoctors_WithSpecializationFilter_ReturnsFilteredDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors("Cardiology", null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Doctors;
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithLocationFilter_ReturnsFilteredDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors(null, "Basra", null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Doctors;
            Assert.Single(doctors);
            Assert.Equal("Dr. Jane Doe", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithSearchFilter_ReturnsMatchingDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors(null, null, null, "John", 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Doctors;
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_WithSpecializationSearch_ReturnsMatchingDoctors()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors(null, null, null, "Dermatology", 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Doctors;
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
                Phone = "+96477777777777",
                Location = "Baghdad"
            };

            // Act
            var result = await controller.UpdateDoctorInfo(formData, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedDoctor = Assert.IsType<Doctor>(okResult.Value);
            Assert.Equal("Updated Cardiology", updatedDoctor.Specialization);
            Assert.Equal("+96477777777777", updatedDoctor.Phone);
            Assert.Equal("Baghdad", updatedDoctor.Location);
            
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
            var result = await controller.GetDoctors(null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            var doctors = response.Doctors;
            
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
            var result = await controller.GetDoctors(null, null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Empty(response.Doctors);
        }

        [Fact]
        public async Task GetDoctors_MultipleFilters_ReturnsCorrectResults()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors("Cardiology", "Baghdad", null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Doctors;
            Assert.Single(doctors);
            Assert.Equal("Dr. John Smith", doctors.First().Name);
        }

        [Fact]
        public async Task GetDoctors_NoMatchingResults_ReturnsEmptyList()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act
            var result = await controller.GetDoctors("NonExistentSpecialization", null, null, null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value); var doctors = response.Doctors;
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
            var result = await controller.GetDoctors(null, null, null, null, 1, 1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Single(response.Doctors);
            Assert.Equal(2, response.TotalCount);
            Assert.Equal(1, response.PageSize);
            Assert.Equal(1, response.CurrentPage);
            Assert.Equal(2, response.TotalPages);
            Assert.False(response.HasPreviousPage);
            Assert.True(response.HasNextPage);
        }

        [Fact]
        public async Task GetDoctors_WithSortBy_SortsByName()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act - Sort by name (default sorting)
            var result = await controller.GetDoctors(null, null, "name", null, 1, 20);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Equal(2, response.Doctors.Count);
            // Should be sorted by name alphabetically
            Assert.Equal("Dr. Jane Doe", response.Doctors.First().Name); 
            Assert.Equal("Dr. John Smith", response.Doctors.Last().Name);
        }

        [Fact]
        public async Task GetDoctors_WithPaginationExceedsMaxPageSize_LimitsToMaxPageSize()
        {
            // Arrange
            var controller = new DoctorsController(_context);

            // Act - Request page size larger than maximum (20)
            var result = await controller.GetDoctors(null, null, null, null, 1, 50);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<PaginatedResponse<Doctor>>(okResult.Value);
            Assert.Equal(20, response.PageSize); // Should be limited to max 20
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}