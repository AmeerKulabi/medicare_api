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
    public class AvailabilityControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;

        public AvailabilityControllerTests(TestFixture fixture)
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
            var doctor = new Doctor
            {
                Id = "test-doctor-id",
                UserId = "doctor-user-id",
                Name = "Dr. Test",
                Email = "doctor@test.com",
                IsActive = true,
                RegistrationCompleted = true
            };

            var availabilitySlot1 = new AvailabilitySlot
            {
                Id = "slot-1",
                DoctorId = "test-doctor-id",
                day = "الأحد",
                Start = "8.00",
                End = "16.00",
                IsAvailable = true
            };

            var availabilitySlot2 = new AvailabilitySlot
            {
                Id = "slot-2",
                DoctorId = "test-doctor-id",
                day = "الإثنين",
                Start = "8.00",
                End = "16.00",
                IsAvailable = false
            };

            _context.Doctors.Add(doctor);
            _context.AvailabilitySlots.AddRange(availabilitySlot1, availabilitySlot2);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAvailability_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContextWithoutAuth(controller);

            // Act
            var result = await controller.GetAvailability();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAvailability_NonDoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetAvailability();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAvailability_DoctorNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "non-existent-user", true);

            // Act
            var result = await controller.GetAvailability();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAvailability_ValidDoctor_ReturnsExistingSlots()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Act
            var result = await controller.GetAvailability();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var slots = Assert.IsAssignableFrom<List<AvailabilitySlot>>(okResult.Value);
            Assert.True(slots.Count >= 2); // At least the seeded slots
        }

        [Fact]
        public async Task GetAvailability_DoctorWithoutSlots_CreatesDefaultSlots()
        {
            // Arrange
            var newDoctor = new Doctor
            {
                Id = "new-doctor-id",
                UserId = "new-doctor-user-id",
                Name = "Dr. New",
                Email = "new@test.com",
                IsActive = true,
                RegistrationCompleted = true
            };
            _context.Doctors.Add(newDoctor);
            await _context.SaveChangesAsync();

            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "new-doctor-user-id", true);

            // Act
            var result = await controller.GetAvailability();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var slots = Assert.IsAssignableFrom<List<AvailabilitySlot>>(okResult.Value);
            Assert.Equal(7, slots.Count); // One for each day of the week
            
            // Verify all days are covered
            string[] expectedDays = ["الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"];
            foreach (var day in expectedDays)
            {
                Assert.Contains(slots, s => s.day == day);
            }
        }

        [Fact]
        public async Task GetDoctorAvailability_ValidDoctorId_ReturnsSlots()
        {
            // Arrange
            var controller = new AvailabilityController(_context);

            // Act
            var result = await controller.GetDoctorAvailability("test-doctor-id");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var slots = Assert.IsAssignableFrom<List<AvailabilitySlot>>(okResult.Value);
            Assert.True(slots.Count >= 2);
        }

        [Fact]
        public async Task GetDoctorAvailability_DoctorWithoutSlots_CreatesDefaultSlots()
        {
            // Arrange
            var newDoctor = new Doctor
            {
                Id = "another-doctor-id",
                UserId = "another-user-id",
                Name = "Dr. Another",
                Email = "another@test.com",
                IsActive = true,
                RegistrationCompleted = true
            };
            _context.Doctors.Add(newDoctor);
            await _context.SaveChangesAsync();

            var controller = new AvailabilityController(_context);

            // Act
            var result = await controller.GetDoctorAvailability("another-doctor-id");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var slots = Assert.IsAssignableFrom<List<AvailabilitySlot>>(okResult.Value);
            Assert.Equal(7, slots.Count);
        }

        [Fact]
        public async Task UpdateAvailabilitySlot_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContextWithoutAuth(controller);
            var update = new AvailabilitySlot
            {
                Start = "9.00",
                End = "17.00",
                IsAvailable = true
            };

            // Act
            var result = await controller.UpdateAvailabilitySlot("slot-1", update);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateAvailabilitySlot_NonDoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "patient-user-id", false);
            var update = new AvailabilitySlot
            {
                Start = "9.00",
                End = "17.00",
                IsAvailable = true
            };

            // Act
            var result = await controller.UpdateAvailabilitySlot("slot-1", update);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateAvailabilitySlot_DoctorNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "non-existent-user", true);
            var update = new AvailabilitySlot
            {
                Start = "9.00",
                End = "17.00",
                IsAvailable = true
            };

            // Act
            var result = await controller.UpdateAvailabilitySlot("slot-1", update);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateAvailabilitySlot_SlotNotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "doctor-user-id", true);
            var update = new AvailabilitySlot
            {
                Start = "9.00",
                End = "17.00",
                IsAvailable = true
            };

            // Act
            var result = await controller.UpdateAvailabilitySlot("non-existent-slot", update);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateAvailabilitySlot_SlotBelongsToAnotherDoctor_ReturnsNotFound()
        {
            // Arrange
            var anotherDoctor = new Doctor
            {
                Id = "another-doctor-id",
                UserId = "another-doctor-user-id",
                Name = "Dr. Another",
                Email = "another@test.com",
                IsActive = true,
                RegistrationCompleted = true
            };

            var anotherSlot = new AvailabilitySlot
            {
                Id = "another-slot",
                DoctorId = "another-doctor-id",
                day = "الثلاثاء",
                Start = "8.00",
                End = "16.00",
                IsAvailable = true
            };

            _context.Doctors.Add(anotherDoctor);
            _context.AvailabilitySlots.Add(anotherSlot);
            await _context.SaveChangesAsync();

            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "doctor-user-id", true); // Different doctor

            var update = new AvailabilitySlot
            {
                Start = "9.00",
                End = "17.00",
                IsAvailable = true
            };

            // Act
            var result = await controller.UpdateAvailabilitySlot("another-slot", update);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateAvailabilitySlot_ValidUpdate_UpdatesSuccessfully()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "doctor-user-id", true);
            var update = new AvailabilitySlot
            {
                Start = "9.00",
                End = "17.00",
                IsAvailable = true
            };

            // Act
            var result = await controller.UpdateAvailabilitySlot("slot-1", update);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedSlot = Assert.IsType<AvailabilitySlot>(okResult.Value);
            Assert.Equal("9.00", updatedSlot.Start);
            Assert.Equal("17.00", updatedSlot.End);
            Assert.True(updatedSlot.IsAvailable);

            // Verify data was persisted
            var slotFromDb = await _context.AvailabilitySlots.FindAsync("slot-1");
            Assert.NotNull(slotFromDb);
            Assert.Equal("9.00", slotFromDb.Start);
            Assert.Equal("17.00", slotFromDb.End);
            Assert.True(slotFromDb.IsAvailable);
        }

        [Fact]
        public async Task UpdateAvailabilitySlot_PartialUpdate_UpdatesOnlySpecifiedFields()
        {
            // Arrange
            var controller = new AvailabilityController(_context);
            SetupControllerContext(controller, "doctor-user-id", true);
            
            // Get original values
            var originalSlot = await _context.AvailabilitySlots.FindAsync("slot-2");
            Assert.NotNull(originalSlot);
            var originalStart = originalSlot.Start;
            var originalEnd = originalSlot.End;

            var update = new AvailabilitySlot
            {
                Start = originalStart, // Keep same
                End = originalEnd,     // Keep same
                IsAvailable = true     // Change this
            };

            // Act
            var result = await controller.UpdateAvailabilitySlot("slot-2", update);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedSlot = Assert.IsType<AvailabilitySlot>(okResult.Value);
            Assert.Equal(originalStart, updatedSlot.Start);
            Assert.Equal(originalEnd, updatedSlot.End);
            Assert.True(updatedSlot.IsAvailable); // This should be changed
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