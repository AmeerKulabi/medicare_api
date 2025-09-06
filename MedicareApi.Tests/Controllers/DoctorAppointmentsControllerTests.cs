using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace MedicareApi.Tests.Controllers
{
    public class DoctorAppointmentsControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IPaymentService> _mockPaymentService;

        public DoctorAppointmentsControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _context = CreateTestDbContext();
            _userManagerMock = CreateUserManagerMock();
            _mockPaymentService = new Mock<IPaymentService>();
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

        private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var userStore = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                userStore.Object, null, null, null, null, null, null, null, null);
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

            var patient = new ApplicationUser
            {
                Id = "patient-user-id",
                Email = "patient@test.com",
                FullName = "Test Patient",
                Phone = "+96470123456789"
            };

            var appointment1 = new Appointment
            {
                Id = "appointment-1",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = AppointmentStatus.Confirmed,
                ScheduledAt = DateTime.Now.AddDays(1),
                Reason = "Checkup"
            };

            var appointment2 = new Appointment
            {
                Id = "appointment-2",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = AppointmentStatus.Booked,
                ScheduledAt = DateTime.Now.AddDays(2),
                Reason = "Follow-up"
            };

            _context.Doctors.Add(doctor);
            _context.Users.Add(patient);
            _context.Appointments.AddRange(appointment1, appointment2);
            await _context.SaveChangesAsync();

            // Setup UserManager mock
            _userManagerMock.Setup(um => um.FindByIdAsync("patient-user-id"))
                .ReturnsAsync(patient);
        }

        [Fact]
        public async Task GetAppointments_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContextWithoutAuth(controller);

            // Act
            var result = await controller.GetAppointments();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAppointments_NonDoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetAppointments();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAppointments_DoctorNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "non-existent-user", true);

            // Act
            var result = await controller.GetAppointments();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetAppointments_ValidDoctor_ReturnsAppointments()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Act
            var result = await controller.GetAppointments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, appointments.Count());
        }

        [Fact]
        public async Task CreateAppointment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContextWithoutAuth(controller);
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = AppointmentStatus.Confirmed,
                ScheduledAt = DateTime.Now.AddDays(1),
                Reason = "Test appointment"
            };

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task CreateAppointment_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "unauthorized-user", false);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id", // Different from current user
                DoctorId = "test-doctor-id",
                Status = AppointmentStatus.Confirmed,
                ScheduledAt = DateTime.Now.AddDays(1),
                Reason = "Test appointment"
            };

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task CreateAppointment_PatientCreatingOwnAppointment_CreatesSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "patient-user-id", false);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = AppointmentStatus.Confirmed,
                ScheduledAt = DateTime.Now.AddDays(3),
                Reason = "New appointment"
            };

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal("patient-user-id", createdAppointment.PatientId);
            Assert.Equal("test-doctor-id", createdAppointment.DoctorId);
            Assert.Equal("New appointment", createdAppointment.Reason);

            // Verify appointment was saved to database
            var dbAppointment = await _context.Appointments.FindAsync(createdAppointment.Id);
            Assert.NotNull(dbAppointment);
        }

        [Fact]
        public async Task CreateAppointment_DoctorCreatingAppointment_CreatesSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "doctor-user-id", true);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = AppointmentStatus.Confirmed,
                ScheduledAt = DateTime.Now.AddDays(3),
                Reason = "Doctor created appointment"
            };

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var createdAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal("patient-user-id", createdAppointment.PatientId);
            Assert.Equal("test-doctor-id", createdAppointment.DoctorId);
        }

        [Fact]
        public async Task UpdateAppointment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContextWithoutAuth(controller);
            var update = new UpdateAppointment
            {
                Status = AppointmentStatus.Canceled
            };

            // Act
            var result = await controller.UpdateAppointment("appointment-1", update);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_NonDoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "patient-user-id", false);
            var update = new UpdateAppointment
            {
                Status = AppointmentStatus.Canceled
            };

            // Act
            var result = await controller.UpdateAppointment("appointment-1", update);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_AppointmentNotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "doctor-user-id", true);
            var update = new UpdateAppointment
            {
                Status = AppointmentStatus.Canceled
            };

            // Act
            var result = await controller.UpdateAppointment("non-existent-appointment", update);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_ValidUpdate_UpdatesSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "doctor-user-id", true);
            var newScheduledTime = DateTime.Now.AddDays(5);
            var update = new UpdateAppointment
            {
                Status = AppointmentStatus.Booked,
                ScheduledAt = newScheduledTime,
                Reason = "Updated reason"
            };

            // Act
            var result = await controller.UpdateAppointment("appointment-1", update);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal(AppointmentStatus.Booked, updatedAppointment.Status);
            Assert.Equal("Updated reason", updatedAppointment.Reason);

            // Verify data was persisted
            var dbAppointment = await _context.Appointments.FindAsync("appointment-1");
            Assert.NotNull(dbAppointment);
            Assert.Equal(AppointmentStatus.Booked, dbAppointment.Status);
        }

        [Fact]
        public async Task DeleteAppointment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContextWithoutAuth(controller);

            // Act
            var result = await controller.DeleteAppointment("appointment-1");

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_NonDoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.DeleteAppointment("appointment-1");

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_AppointmentNotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Act
            var result = await controller.DeleteAppointment("non-existent-appointment");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_ValidAppointment_DeletesSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Verify appointment exists before deletion
            var appointmentBeforeDelete = await _context.Appointments.FindAsync("appointment-2");
            Assert.NotNull(appointmentBeforeDelete);

            // Act
            var result = await controller.DeleteAppointment("appointment-2");

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify appointment was deleted
            var appointmentAfterDelete = await _context.Appointments.FindAsync("appointment-2");
            Assert.Null(appointmentAfterDelete);
        }

        [Fact]
        public async Task CreateAppointment_RequiredFieldsValidation()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, _mockPaymentService.Object);
            SetupControllerContext(controller, "patient-user-id", false);
            
            var appointment = new Appointment
            {
                // Missing required fields
                PatientId = "patient-user-id",
                DoctorId = "", // Empty doctor ID
                Status = AppointmentStatus.Confirmed,
                ScheduledAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert - This would normally be handled by model validation
            // But since we're testing the controller directly, we just verify it accepts the call
            var result = await controller.CreateAppointment(appointment);
            
            // The controller should still process it, validation would happen at the API level
            Assert.IsType<OkObjectResult>(result);
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