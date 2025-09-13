using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;
using MedicareApi.Tests.Helpers;

namespace MedicareApi.Tests.Controllers
{
    public class BlockedTimeControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;

        public BlockedTimeControllerTests(TestFixture fixture)
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
                UserId = "test-user-id",
                Name = "Test Doctor",
                Email = "doctor@test.com",
                IsActive = true,
                RegistrationCompleted = true
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
        }

        private BlockedTimeController CreateController()
        {
            var controller = new BlockedTimeController(_context);
            
            // Set up claims for authenticated doctor user
            var claims = new List<Claim>
            {
                new("uid", "test-user-id"),
                new("isDoctor", "True")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            return controller;
        }

        [Fact]
        public async Task GetBlockedTimeSlots_AuthenticatedDoctor_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            var blockedSlot = new BlockedTimeSlot
            {
                DoctorId = "test-doctor-id",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsWholeDay = false,
                Reason = "Personal time"
            };
            _context.BlockedTimeSlots.Add(blockedSlot);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.GetBlockedTimeSlots();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var blockedSlots = Assert.IsAssignableFrom<List<BlockedTimeSlotResponse>>(okResult.Value);
            Assert.Single(blockedSlots);
            Assert.Equal("Personal time", blockedSlots[0].reason);
        }

        [Fact]
        public async Task CreateBlockedTimeSlot_ValidRequest_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            var tomorrow = DateTime.UtcNow.AddDays(2);
            var request = new CreateBlockedTimeSlotRequest
            {
                Date = tomorrow.ToString("yyyy-MM-dd"),
                StartTime = "09:00",
                EndTime = "12:00",
                IsWholeDay = false,
                Reason = "Medical conference"
            };

            // Act
            var result = await controller.CreateBlockedTimeSlot(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BlockedTimeSlotResponse>(okResult.Value);
            Assert.Equal("Medical conference", response.reason);
            Assert.Equal("test-doctor-id", response.doctorId);
        }

        [Fact]
        public async Task CreateBlockedTimeSlot_InvalidTimeRange_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();
            var tomorrow = DateTime.UtcNow.AddDays(2);
            var request = new CreateBlockedTimeSlotRequest
            {
                Date = tomorrow.ToString("yyyy-MM-dd"),
                StartTime = "15:00",
                EndTime = "09:00", // End before start
                IsWholeDay = false,
                Reason = "Invalid range"
            };

            // Act
            var result = await controller.CreateBlockedTimeSlot(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("End time must be after start time", badRequestResult.Value);
        }

        [Fact]
        public async Task CreateBlockedTimeSlot_ConflictingAppointment_ReturnsBadRequest()
        {
            // Arrange
            var controller = CreateController();
            var startTime = DateTime.UtcNow.AddDays(3);
            var endTime = startTime.AddHours(2);

            // Add existing appointment
            var appointment = new Appointment
            {
                DoctorId = "test-doctor-id",
                PatientId = "patient-id",
                ScheduledAt = startTime.AddMinutes(30), // Within the blocked range
                Status = "Booked"
            };
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var request = new CreateBlockedTimeSlotRequest
            {
                Date = startTime.ToString("yyyy-MM-dd"),
                StartTime = startTime.ToString("HH:mm"),
                EndTime = endTime.ToString("HH:mm"),
                IsWholeDay = false,
                Reason = "Should fail"
            };

            // Act
            var result = await controller.CreateBlockedTimeSlot(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("existing appointments", badRequestResult.Value?.ToString());
        }

        [Fact]
        public async Task GetDoctorBlockedTimeSlots_ValidDoctorId_ReturnsOkWithoutReasons()
        {
            // Arrange
            var controller = CreateController();
            var blockedSlot = new BlockedTimeSlot
            {
                DoctorId = "test-doctor-id",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsWholeDay = false,
                Reason = "Private reason" // This should be hidden
            };
            _context.BlockedTimeSlots.Add(blockedSlot);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.GetDoctorBlockedTimeSlots("test-doctor-id");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var blockedSlots = Assert.IsAssignableFrom<List<BlockedTimeSlotResponse>>(okResult.Value);
            Assert.Single(blockedSlots);
            Assert.Null(blockedSlots[0].reason); // Reason should be hidden for privacy
        }

        [Fact]
        public async Task UpdateBlockedTimeSlot_ValidRequest_ReturnsOk()
        {
            // Arrange
            var controller = CreateController();
            var blockedSlot = new BlockedTimeSlot
            {
                DoctorId = "test-doctor-id",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsWholeDay = false,
                Reason = "Original reason"
            };
            _context.BlockedTimeSlots.Add(blockedSlot);
            await _context.SaveChangesAsync();

            var updateRequest = new UpdateBlockedTimeSlotRequest
            {
                Reason = "Updated reason"
            };

            // Act
            var result = await controller.UpdateBlockedTimeSlot(blockedSlot.Id, updateRequest);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<BlockedTimeSlotResponse>(okResult.Value);
            Assert.Equal("Updated reason", response.reason);
        }

        [Fact]
        public async Task DeleteBlockedTimeSlot_ValidId_ReturnsNoContent()
        {
            // Arrange
            var controller = CreateController();
            var blockedSlot = new BlockedTimeSlot
            {
                DoctorId = "test-doctor-id",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(2),
                IsWholeDay = false,
                Reason = "To be deleted"
            };
            _context.BlockedTimeSlots.Add(blockedSlot);
            await _context.SaveChangesAsync();

            // Act
            var result = await controller.DeleteBlockedTimeSlot(blockedSlot.Id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            
            // Verify deletion
            var deletedSlot = await _context.BlockedTimeSlots.FindAsync(blockedSlot.Id);
            Assert.Null(deletedSlot);
        }

        [Fact]
        public async Task GetBlockedTimeSlots_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new BlockedTimeController(_context);
            
            // Set up claims for non-doctor user
            var claims = new List<Claim>
            {
                new("uid", "test-user-id"),
                new("isDoctor", "False") // Not a doctor
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            // Act
            var result = await controller.GetBlockedTimeSlots();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }
    }
}