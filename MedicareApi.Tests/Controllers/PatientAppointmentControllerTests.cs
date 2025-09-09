using MedicareApi.Controllers;
using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Xunit;
using System.Linq;

namespace MedicareApi.Tests.Controllers
{
    public class PatientAppointmentControllerTests : IClassFixture<TestFixture>
    {
        private readonly TestFixture _fixture;
        private readonly ApplicationDbContext _context;

        public PatientAppointmentControllerTests(TestFixture fixture)
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
                RegistrationCompleted = true,
                Specialization = "Cardiology",
                ClinicName = "Heart Clinic",
                ConsultationFee = "100"
            };

            var pastAppointment = new Appointment
            {
                Id = "past-appointment",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "completed",
                ScheduledAt = DateTime.Now.AddDays(-5),
                Reason = "Past checkup"
            };

            var futureAppointment = new Appointment
            {
                Id = "future-appointment",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(5),
                Reason = "Future checkup"
            };

            var anotherPatientAppointment = new Appointment
            {
                Id = "other-patient-appointment",
                PatientId = "other-patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(3),
                Reason = "Other patient appointment"
            };

            _context.Doctors.Add(doctor);
            _context.Appointments.AddRange(pastAppointment, futureAppointment, anotherPatientAppointment);
            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetPatientAppointments_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContextWithoutAuth(controller);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetPatientAppointments_DoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task GetPatientAppointments_UpcomingType_ReturnsFutureAppointments()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<List<PatientAppointment>>(okResult.Value);
            Assert.Single(appointments);
            Assert.Equal("future-appointment", appointments.First().id);
        }

        [Fact]
        public async Task GetPatientAppointments_PastType_ReturnsPastAppointments()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("past");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<List<PatientAppointment>>(okResult.Value);
            Assert.Single(appointments);
            Assert.Equal("past-appointment", appointments.First().id);
        }

        [Fact]
        public async Task GetPatientAppointments_DefaultType_ReturnsFutureAppointments()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("anything");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<List<PatientAppointment>>(okResult.Value);
            Assert.Single(appointments); // Should return future appointments by default
        }

        [Fact]
        public async Task GetPatientAppointments_OnlyReturnsOwnAppointments()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<List<PatientAppointment>>(okResult.Value);
            
            // Should only return appointments for this patient, not other patients
            Assert.All(appointments, apt => Assert.Equal("future-appointment", apt.id));
        }

        [Fact]
        public async Task GetPatientAppointments_PopulatesCorrectDoctorInformation()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<List<PatientAppointment>>(okResult.Value);
            var appointment = appointments.First();
            
            Assert.Equal("Dr. Test", appointment.doctorName);
            Assert.Equal("test-doctor-id", appointment.doctorId);
            Assert.Equal("Cardiology", appointment.doctorSpecialization);
            Assert.Equal("Heart Clinic", appointment.clinicName);
            Assert.Null(appointment.address); // ClinicAddress field was removed as deprecated
            Assert.Equal(100, appointment.consultationFee);
        }

        [Fact]
        public async Task DeletePatientAppointment_WithoutAuth_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContextWithoutAuth(controller);

            // Act
            var result = await controller.DeletePatientAppointment("future-appointment");

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task DeletePatientAppointment_DoctorUser_ReturnsUnauthorized()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "doctor-user-id", true);

            // Act
            var result = await controller.DeletePatientAppointment("future-appointment");

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public async Task DeletePatientAppointment_AppointmentNotFound_ReturnsNotFound()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.DeletePatientAppointment("non-existent-appointment");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeletePatientAppointment_OtherPatientsAppointment_ReturnsNotFound()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act - Try to delete another patient's appointment
            var result = await controller.DeletePatientAppointment("other-patient-appointment");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeletePatientAppointment_ValidAppointment_DeletesSuccessfully()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Verify appointment exists before deletion
            var appointmentBeforeDelete = await _context.Appointments.FindAsync("future-appointment");
            Assert.NotNull(appointmentBeforeDelete);

            // Act
            var result = await controller.DeletePatientAppointment("future-appointment");

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify appointment was deleted
            var appointmentAfterDelete = await _context.Appointments.FindAsync("future-appointment");
            Assert.Null(appointmentAfterDelete);
        }

        [Fact]
        public async Task GetPatientAppointments_NoAppointments_ReturnsEmptyList()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-without-appointments", false);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<List<PatientAppointment>>(okResult.Value);
            Assert.Empty(appointments);
        }

        [Fact]
        public async Task GetPatientAppointments_FormatsDateAndTimeCorrectly()
        {
            // Arrange
            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsAssignableFrom<List<PatientAppointment>>(okResult.Value);
            var appointment = appointments.First();
            
            // Verify date and time are formatted correctly
            Assert.NotNull(appointment.date);
            Assert.NotNull(appointment.time);
            Assert.NotEmpty(appointment.date);
            Assert.NotEmpty(appointment.time);
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
        public async Task GetPatientAppointments_WithNullReason_ReturnsEmptyReasonString()
        {
            // Arrange - Add appointment with null reason
            var appointmentWithNullReason = new Appointment
            {
                Id = "appointment-null-reason",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(3),
                Reason = null // Explicitly null
            };
            
            _context.Appointments.Add(appointmentWithNullReason);
            await _context.SaveChangesAsync();

            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsType<List<PatientAppointment>>(okResult.Value);
            
            var appointmentWithNullReasonResult = appointments.FirstOrDefault(a => a.id == "appointment-null-reason");
            Assert.NotNull(appointmentWithNullReasonResult);
            Assert.Equal("", appointmentWithNullReasonResult.reason); // Should be empty string, not null
        }

        [Fact]
        public async Task GetPatientAppointments_WithEmptyReason_ReturnsEmptyReasonString()
        {
            // Arrange - Add appointment with empty reason
            var appointmentWithEmptyReason = new Appointment
            {
                Id = "appointment-empty-reason",
                PatientId = "patient-user-id",
                DoctorId = "test-doctor-id",
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(4),
                Reason = "" // Explicitly empty string
            };
            
            _context.Appointments.Add(appointmentWithEmptyReason);
            await _context.SaveChangesAsync();

            var controller = new PatientAppointmentController(_context);
            SetupControllerContext(controller, "patient-user-id", false);

            // Act
            var result = await controller.GetPatientAppointments("upcoming");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var appointments = Assert.IsType<List<PatientAppointment>>(okResult.Value);
            
            var appointmentWithEmptyReasonResult = appointments.FirstOrDefault(a => a.id == "appointment-empty-reason");
            Assert.NotNull(appointmentWithEmptyReasonResult);
            Assert.Equal("", appointmentWithEmptyReasonResult.reason); // Should be empty string
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}