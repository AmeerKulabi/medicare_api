using MedicareApi.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedicareApi.Tests.Services
{
    public class AnalyticsServiceTests
    {
        private readonly Mock<ILogger<AnalyticsService>> _loggerMock;
        private readonly TelemetryClient _telemetryClient;
        private readonly AnalyticsService _analyticsService;

        public AnalyticsServiceTests()
        {
            _loggerMock = new Mock<ILogger<AnalyticsService>>();
            
            // Create a test telemetry configuration
            var config = new TelemetryConfiguration();
            _telemetryClient = new TelemetryClient(config);
            
            _analyticsService = new AnalyticsService(_telemetryClient, _loggerMock.Object);
        }

        [Fact]
        public void TrackSignIn_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackSignIn("user123", true);
            _analyticsService.TrackSignIn("user456", false);
        }

        [Fact]
        public void TrackUserRegistration_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackUserRegistration("user123", true);
            _analyticsService.TrackUserRegistration("user456", false);
        }

        [Fact]
        public void TrackAppointmentCreated_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackAppointmentCreated("apt123", "patient456", "doctor789");
        }

        [Fact]
        public void TrackAppointmentUpdated_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackAppointmentUpdated("apt123");
        }

        [Fact]
        public void TrackAppointmentDeleted_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackAppointmentDeleted("apt123");
        }

        [Fact]
        public void TrackDoctorSearch_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackDoctorSearch("cardiology", "Cardiology", "Baghdad", 5);
            _analyticsService.TrackDoctorSearch(null, null, null, 0);
        }

        [Fact]
        public void TrackDoctorDetailsViewed_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackDoctorDetailsViewed("doctor123");
        }

        [Fact]
        public void TrackDoctorProfileUpdated_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackDoctorProfileUpdated("doctor123");
        }

        [Fact]
        public void TrackDoctorProfilePictureUpdated_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackDoctorProfilePictureUpdated("doctor123");
        }

        [Fact]
        public void TrackDailyMetrics_ValidData_NoExceptions()
        {
            // Act & Assert - should not throw
            _analyticsService.TrackDailyMetrics(100, 500);
        }
    }
}
