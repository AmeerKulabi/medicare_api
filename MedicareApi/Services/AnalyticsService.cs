using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace MedicareApi.Services
{
    /// <summary>
    /// Service for tracking analytics events to Azure Application Insights.
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<AnalyticsService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="telemetryClient">Application Insights telemetry client</param>
        /// <param name="logger">Logger</param>
        public AnalyticsService(TelemetryClient telemetryClient, ILogger<AnalyticsService> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public void TrackSignIn(string userId, bool isDoctor)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "UserId", userId },
                    { "UserType", isDoctor ? "Doctor" : "Patient" }
                };

                _telemetryClient.TrackEvent("UserSignIn", properties);
                _logger.LogInformation("Tracked sign-in event for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track sign-in event for user {UserId}", userId);
            }
        }

        /// <inheritdoc/>
        public void TrackUserRegistration(string userId, bool isDoctor)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "UserId", userId },
                    { "UserType", isDoctor ? "Doctor" : "Patient" }
                };

                _telemetryClient.TrackEvent("UserRegistration", properties);
                _logger.LogInformation("Tracked registration event for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track registration event for user {UserId}", userId);
            }
        }

        /// <inheritdoc/>
        public void TrackAppointmentCreated(string appointmentId, string patientId, string doctorId)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "AppointmentId", appointmentId },
                    { "PatientId", patientId },
                    { "DoctorId", doctorId }
                };

                _telemetryClient.TrackEvent("AppointmentCreated", properties);
                _logger.LogInformation("Tracked appointment creation for appointment {AppointmentId}", appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track appointment creation for appointment {AppointmentId}", appointmentId);
            }
        }

        /// <inheritdoc/>
        public void TrackAppointmentUpdated(string appointmentId)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "AppointmentId", appointmentId }
                };

                _telemetryClient.TrackEvent("AppointmentUpdated", properties);
                _logger.LogInformation("Tracked appointment update for appointment {AppointmentId}", appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track appointment update for appointment {AppointmentId}", appointmentId);
            }
        }

        /// <inheritdoc/>
        public void TrackAppointmentDeleted(string appointmentId)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "AppointmentId", appointmentId }
                };

                _telemetryClient.TrackEvent("AppointmentDeleted", properties);
                _logger.LogInformation("Tracked appointment deletion for appointment {AppointmentId}", appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track appointment deletion for appointment {AppointmentId}", appointmentId);
            }
        }

        /// <inheritdoc/>
        public void TrackDoctorSearch(string? searchQuery, string? specialization, string? location, int resultsCount)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "SearchQuery", searchQuery ?? "none" },
                    { "Specialization", specialization ?? "none" },
                    { "Location", location ?? "none" },
                    { "ResultsCount", resultsCount.ToString() }
                };

                _telemetryClient.TrackEvent("DoctorSearch", properties);
                _logger.LogInformation("Tracked doctor search with query: {SearchQuery}", searchQuery ?? "none");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track doctor search event");
            }
        }

        /// <inheritdoc/>
        public void TrackDoctorDetailsViewed(string doctorId)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "DoctorId", doctorId }
                };

                _telemetryClient.TrackEvent("DoctorDetailsViewed", properties);
                _logger.LogInformation("Tracked doctor details view for doctor {DoctorId}", doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track doctor details view for doctor {DoctorId}", doctorId);
            }
        }

        /// <inheritdoc/>
        public void TrackDoctorProfileUpdated(string doctorId)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "DoctorId", doctorId }
                };

                _telemetryClient.TrackEvent("DoctorProfileUpdated", properties);
                _logger.LogInformation("Tracked doctor profile update for doctor {DoctorId}", doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track doctor profile update for doctor {DoctorId}", doctorId);
            }
        }

        /// <inheritdoc/>
        public void TrackDoctorProfilePictureUpdated(string doctorId)
        {
            try
            {
                var properties = new Dictionary<string, string>
                {
                    { "DoctorId", doctorId }
                };

                _telemetryClient.TrackEvent("DoctorProfilePictureUpdated", properties);
                _logger.LogInformation("Tracked doctor profile picture update for doctor {DoctorId}", doctorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track doctor profile picture update for doctor {DoctorId}", doctorId);
            }
        }

        /// <inheritdoc/>
        public void TrackDailyMetrics(int doctorCount, int patientCount)
        {
            try
            {
                var metrics = new Dictionary<string, double>
                {
                    { "DoctorCount", doctorCount },
                    { "PatientCount", patientCount }
                };

                foreach (var metric in metrics)
                {
                    _telemetryClient.TrackMetric(metric.Key, metric.Value);
                }

                var properties = new Dictionary<string, string>
                {
                    { "EventType", "DailyMetrics" },
                    { "Date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
                };

                _telemetryClient.TrackEvent("DailyMetrics", properties, metrics);
                _logger.LogInformation("Tracked daily metrics: Doctors={DoctorCount}, Patients={PatientCount}", doctorCount, patientCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to track daily metrics");
            }
        }
    }
}
