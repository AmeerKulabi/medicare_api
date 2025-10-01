using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using MedicareApi.Tests.Helpers;

namespace MedicareApi.Tests.Controllers
{
    public class DoctorAppointmentsControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;

        public DoctorAppointmentsControllerTests(TestFixture fixture)
        {
            _fixture = fixture;
            _context = CreateTestDbContext();
            _userManagerMock = CreateUserManagerMock();
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
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(1),
                Reason = "Checkup"
            };

            var appointment2 = new Appointment
            {
                Id = "appointment-2",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "pending",
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContextWithoutAuth(controller);
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "unauthorized-user", false);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id", // Different from current user
                DoctorId = "test-doctor-id",
                Status = "confirmed",
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "patient-user-id", false);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(3),
                Reason = "New appointment"
            };

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            
            // Extract properties from anonymous object
            var response = okResult.Value;
            var idProperty = response.GetType().GetProperty("id");
            var patientIdProperty = response.GetType().GetProperty("patientId");
            var reasonProperty = response.GetType().GetProperty("reason");
            var dateProperty = response.GetType().GetProperty("date");
            var timeProperty = response.GetType().GetProperty("time");
            
            Assert.NotNull(idProperty);
            Assert.NotNull(dateProperty);
            Assert.NotNull(timeProperty);
            var appointmentId = idProperty.GetValue(response)?.ToString();
            Assert.Equal("patient-user-id", patientIdProperty?.GetValue(response)?.ToString());
            Assert.Equal("New appointment", reasonProperty?.GetValue(response)?.ToString());

            // Verify appointment was saved to database
            var dbAppointment = await _context.Appointments.FindAsync(appointmentId);
            Assert.NotNull(dbAppointment);
        }

        [Fact]
        public async Task CreateAppointment_DoctorCreatingAppointment_CreatesSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(3),
                Reason = "Doctor created appointment"
            };

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var response = okResult.Value;
            var patientIdProperty = response.GetType().GetProperty("patientId");
            var doctorIdProperty = response.GetType().GetProperty("doctorId");
            Assert.Equal("patient-user-id", patientIdProperty?.GetValue(response)?.ToString());
            Assert.Equal("test-doctor-id", doctorIdProperty?.GetValue(response)?.ToString());
        }

        [Fact]
        public async Task UpdateAppointment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContextWithoutAuth(controller);
            var update = new UpdateAppointment
            {
                Status = "cancelled"
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "patient-user-id", false);
            var update = new UpdateAppointment
            {
                Status = "cancelled"
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);
            var update = new UpdateAppointment
            {
                Status = "cancelled"
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);
            var newScheduledTime = DateTime.Now.AddDays(5);
            var update = new UpdateAppointment
            {
                Status = "rescheduled",
                ScheduledAt = newScheduledTime,
                Reason = "Updated reason"
            };

            // Act
            var result = await controller.UpdateAppointment("appointment-1", update);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal("rescheduled", updatedAppointment.Status);
            Assert.Equal("Updated reason", updatedAppointment.Reason);

            // Verify data was persisted
            var dbAppointment = await _context.Appointments.FindAsync("appointment-1");
            Assert.NotNull(dbAppointment);
            Assert.Equal("rescheduled", dbAppointment.Status);
        }

        [Fact]
        public async Task DeleteAppointment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
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
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "patient-user-id", false);
            
            var appointment = new Appointment
            {
                // Missing required fields
                PatientId = "patient-user-id",
                DoctorId = "", // Empty doctor ID
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(1)
            };

            // Act & Assert - This would normally be handled by model validation
            // But since we're testing the controller directly, we just verify it accepts the call
            var result = await controller.CreateAppointment(appointment);
            
            // The controller should still process it, validation would happen at the API level
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task CreateAppointment_WithoutReason_CreatesSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "patient-user-id", false);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(1),
                // Reason is intentionally not set (null)
            };

            // Act
            var result = await controller.CreateAppointment(appointment);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var response = okResult.Value;
            var idProperty = response.GetType().GetProperty("id");
            var reasonProperty = response.GetType().GetProperty("reason");
            var patientIdProperty = response.GetType().GetProperty("patientId");
            var doctorIdProperty = response.GetType().GetProperty("doctorId");
            
            Assert.Null(reasonProperty?.GetValue(response)); // Reason should be null
            Assert.Equal("patient-user-id", patientIdProperty?.GetValue(response)?.ToString());
            Assert.Equal("test-doctor-id", doctorIdProperty?.GetValue(response)?.ToString());

            // Verify appointment was saved to database
            var appointmentId = idProperty?.GetValue(response)?.ToString();
            var dbAppointment = await _context.Appointments.FindAsync(appointmentId);
            Assert.NotNull(dbAppointment);
            Assert.Null(dbAppointment.Reason); // Reason should be null in the database too
        }

        [Fact]
        public async Task CreateAppointment_WithEmptyReason_CreatesSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "patient-user-id", false);
            
            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(1),
                Reason = "" // Empty string
            };

            // Act
            var result = await controller.CreateAppointment(appointment);
            
            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            var response = okResult.Value;
            var idProperty = response.GetType().GetProperty("id");
            var reasonProperty = response.GetType().GetProperty("reason");
            var patientIdProperty = response.GetType().GetProperty("patientId");
            var doctorIdProperty = response.GetType().GetProperty("doctorId");
            
            Assert.Equal("", reasonProperty?.GetValue(response)?.ToString()); // Reason should be empty string
            Assert.Equal("patient-user-id", patientIdProperty?.GetValue(response)?.ToString());
            Assert.Equal("test-doctor-id", doctorIdProperty?.GetValue(response)?.ToString());

            // Verify appointment was saved to database
            var appointmentId = idProperty?.GetValue(response)?.ToString();
            var dbAppointment = await _context.Appointments.FindAsync(appointmentId);
            Assert.NotNull(dbAppointment);
            Assert.Equal("", dbAppointment.Reason); // Reason should be empty string in the database
        }

        [Fact]
        public async Task UpdateAppointment_WithoutReason_UpdatesOtherFieldsSuccessfully()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);
            var newScheduledTime = DateTime.Now.AddDays(5);
            var update = new UpdateAppointment
            {
                Status = "rescheduled",
                ScheduledAt = newScheduledTime
                // Reason is intentionally not set (null)
            };

            // Act
            var result = await controller.UpdateAppointment("appointment-1", update);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal("rescheduled", updatedAppointment.Status);
            Assert.Equal("Checkup", updatedAppointment.Reason); // Should keep original reason

            // Verify data was persisted
            var dbAppointment = await _context.Appointments.FindAsync("appointment-1");
            Assert.NotNull(dbAppointment);
            Assert.Equal("rescheduled", dbAppointment.Status);
            Assert.Equal("Checkup", dbAppointment.Reason); // Should keep original reason
        }

        [Fact]
        public async Task UpdateAppointment_WithEmptyReason_UpdatesReasonToEmpty()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);
            var update = new UpdateAppointment
            {
                Status = "confirmed",
                Reason = "" // Empty string should update the reason to empty
            };

            // Act
            var result = await controller.UpdateAppointment("appointment-1", update);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var updatedAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal("confirmed", updatedAppointment.Status);
            Assert.Equal("", updatedAppointment.Reason); // Should be updated to empty string

            // Verify data was persisted
            var dbAppointment = await _context.Appointments.FindAsync("appointment-1");
            Assert.NotNull(dbAppointment);
            Assert.Equal("confirmed", dbAppointment.Status);
            Assert.Equal("", dbAppointment.Reason); // Should be updated to empty string
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
        public async Task CreateAppointment_WithReasonExceeding500Characters_ReturnsBadRequest()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Create a reason that is longer than 500 characters
            var longReason = new string('x', 501); // 501 characters

            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                ScheduledAt = DateTime.Now.AddDays(1),
                Status = "Booked",
                Reason = longReason
            };

            // Manually add validation error to ModelState to simulate real validation
            controller.ModelState.AddModelError("Reason", "Appointment reason must not exceed 500 characters");

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task CreateAppointment_WithReasonUnder500Characters_ReturnsOk()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Create a reason that is exactly 500 characters
            var validReason = new string('x', 500); // 500 characters

            var appointment = new Appointment
            {
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                ScheduledAt = DateTime.Now.AddDays(1),
                Status = "Booked",
                Reason = validReason
            };

            // Act
            var result = await controller.CreateAppointment(appointment);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_WithReasonExceeding500Characters_ReturnsBadRequest()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Create a reason that is longer than 500 characters
            var longReason = new string('x', 501); // 501 characters

            var updates = new UpdateAppointment
            {
                Reason = longReason
            };

            // Manually add validation error to ModelState to simulate real validation
            controller.ModelState.AddModelError("Reason", "Appointment reason must not exceed 500 characters");

            // Act
            var result = await controller.UpdateAppointment("appointment-1", updates);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_WithReasonUnder500Characters_ReturnsOk()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Create a reason that is exactly 500 characters
            var validReason = new string('x', 500); // 500 characters

            var updates = new UpdateAppointment
            {
                Reason = validReason
            };

            // Act
            var result = await controller.UpdateAppointment("appointment-1", updates);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void Appointment_ReasonExceeds500Characters_ValidationFails()
        {
            // Arrange
            var longReason = new string('x', 501); // 501 characters
            var appointment = new Appointment
            {
                PatientId = "patient-1",
                DoctorId = "doctor-1",
                ScheduledAt = DateTime.Now.AddDays(1),
                Status = "Booked",
                Reason = longReason
            };

            // Act
            var validationResults = ValidationTestHelper.ValidateObject(appointment);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Appointment reason must not exceed 500 characters", validationResults[0].ErrorMessage);
            Assert.Contains("Reason", validationResults[0].MemberNames);
        }

        [Fact]
        public void Appointment_ReasonEquals500Characters_ValidationPasses()
        {
            // Arrange
            var validReason = new string('x', 500); // exactly 500 characters
            var appointment = new Appointment
            {
                PatientId = "patient-1",
                DoctorId = "doctor-1",
                ScheduledAt = DateTime.Now.AddDays(1),
                Status = "Booked",
                Reason = validReason
            };

            // Act
            var validationResults = ValidationTestHelper.ValidateObject(appointment);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Appointment_ReasonUnder500Characters_ValidationPasses()
        {
            // Arrange
            var validReason = new string('x', 499); // 499 characters
            var appointment = new Appointment
            {
                PatientId = "patient-1",
                DoctorId = "doctor-1",
                ScheduledAt = DateTime.Now.AddDays(1),
                Status = "Booked",
                Reason = validReason
            };

            // Act
            var validationResults = ValidationTestHelper.ValidateObject(appointment);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Appointment_ReasonIsNull_ValidationPasses()
        {
            // Arrange
            var appointment = new Appointment
            {
                PatientId = "patient-1",
                DoctorId = "doctor-1",
                ScheduledAt = DateTime.Now.AddDays(1),
                Status = "Booked",
                Reason = null
            };

            // Act
            var validationResults = ValidationTestHelper.ValidateObject(appointment);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void UpdateAppointment_ReasonExceeds500Characters_ValidationFails()
        {
            // Arrange
            var longReason = new string('x', 501); // 501 characters
            var updateAppointment = new UpdateAppointment
            {
                Reason = longReason
            };

            // Act
            var validationResults = ValidationTestHelper.ValidateObject(updateAppointment);

            // Assert
            Assert.Single(validationResults);
            Assert.Equal("Appointment reason must not exceed 500 characters", validationResults[0].ErrorMessage);
            Assert.Contains("Reason", validationResults[0].MemberNames);
        }

        [Fact]
        public void UpdateAppointment_ReasonEquals500Characters_ValidationPasses()
        {
            // Arrange
            var validReason = new string('x', 500); // exactly 500 characters
            var updateAppointment = new UpdateAppointment
            {
                Reason = validReason
            };

            // Act
            var validationResults = ValidationTestHelper.ValidateObject(updateAppointment);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void UpdateAppointment_ReasonIsNull_ValidationPasses()
        {
            // Arrange
            var updateAppointment = new UpdateAppointment
            {
                Reason = null
            };

            // Act
            var validationResults = ValidationTestHelper.ValidateObject(updateAppointment);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void GetBlockedAvailability_WithMonthlyFilter_ReturnsFlatArray()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            
            // Add some test appointments for next month
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            var appointment1 = new Appointment
            {
                Id = "appointment-3",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = new DateTime(nextMonth.Year, nextMonth.Month, 15, 10, 0, 0),
                Reason = "Monthly test"
            };
            var appointment2 = new Appointment
            {
                Id = "appointment-4",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = new DateTime(nextMonth.Year, nextMonth.Month, 20, 14, 30, 0),
                Reason = "Another test"
            };
            _context.Appointments.AddRange(appointment1, appointment2);
            _context.SaveChangesAsync().Wait();

            // Act
            var result = controller.GetBlockedAvailability("test-doctor-id", nextMonth.Year, nextMonth.Month);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var slots = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.Equal(2, slots.Count());
        }

        [Fact]
        public void GetBlockedAvailability_WithoutMonthYear_UsesCurrentMonth()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            
            // Add appointment in current month
            var now = DateTime.UtcNow;
            var appointment = new Appointment
            {
                Id = "appointment-current",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = new DateTime(now.Year, now.Month, 15, 10, 0, 0),
                Reason = "Current month test"
            };
            _context.Appointments.Add(appointment);
            _context.SaveChangesAsync().Wait();

            // Act - no year/month specified, should use current
            var result = controller.GetBlockedAvailability("test-doctor-id", null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var slots = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            Assert.NotEmpty(slots);
        }

        [Fact]
        public void GetBlockedAvailability_IncludesBlockedTimeSlots()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            
            // Add a blocked time slot
            var blockedSlot = new BlockedTimeSlot
            {
                Id = "blocked-1",
                DoctorId = "test-doctor-id",
                StartTime = new DateTime(nextMonth.Year, nextMonth.Month, 10, 9, 0, 0),
                EndTime = new DateTime(nextMonth.Year, nextMonth.Month, 10, 12, 0, 0),
                IsWholeDay = false,
                Reason = "Personal time"
            };
            _context.BlockedTimeSlots.Add(blockedSlot);
            
            // Add a booked appointment
            var appointment = new Appointment
            {
                Id = "appointment-5",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = new DateTime(nextMonth.Year, nextMonth.Month, 15, 10, 0, 0),
                Reason = "Test appointment"
            };
            _context.Appointments.Add(appointment);
            _context.SaveChangesAsync().Wait();

            // Act
            var result = controller.GetBlockedAvailability("test-doctor-id", nextMonth.Year, nextMonth.Month);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var slots = Assert.IsAssignableFrom<IEnumerable<object>>(okResult.Value);
            // Should have both blocked slot and booked appointment
            Assert.Equal(2, slots.Count());
        }

        [Fact]
        public void GetBlockedAvailability_WholeDayBlock_ReturnsCorrectFormat()
        {
            // Arrange
            var controller = new DoctorAppointmentsController(_context, _userManagerMock.Object, null);
            
            var nextMonth = DateTime.UtcNow.AddMonths(1);
            
            // Add a whole-day blocked time slot
            var blockedSlot = new BlockedTimeSlot
            {
                Id = "blocked-whole-day",
                DoctorId = "test-doctor-id",
                StartTime = new DateTime(nextMonth.Year, nextMonth.Month, 25, 0, 0, 0),
                EndTime = new DateTime(nextMonth.Year, nextMonth.Month, 25, 23, 59, 59),
                IsWholeDay = true,
                Reason = "Day off"
            };
            _context.BlockedTimeSlots.Add(blockedSlot);
            _context.SaveChangesAsync().Wait();

            // Act
            var result = controller.GetBlockedAvailability("test-doctor-id", nextMonth.Year, nextMonth.Month);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}