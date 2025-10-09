namespace MedicareApi.Services
{
    /// <summary>
    /// Interface for analytics tracking service.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Track user sign in event.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="isDoctor">Whether the user is a doctor</param>
        void TrackSignIn(string userId, bool isDoctor);

        /// <summary>
        /// Track new user registration event.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="isDoctor">Whether the user is a doctor</param>
        void TrackUserRegistration(string userId, bool isDoctor);

        /// <summary>
        /// Track appointment creation event.
        /// </summary>
        /// <param name="appointmentId">Appointment ID</param>
        /// <param name="patientId">Patient ID</param>
        /// <param name="doctorId">Doctor ID</param>
        void TrackAppointmentCreated(string appointmentId, string patientId, string doctorId);

        /// <summary>
        /// Track appointment update event.
        /// </summary>
        /// <param name="appointmentId">Appointment ID</param>
        void TrackAppointmentUpdated(string appointmentId);

        /// <summary>
        /// Track appointment deletion event.
        /// </summary>
        /// <param name="appointmentId">Appointment ID</param>
        void TrackAppointmentDeleted(string appointmentId);

        /// <summary>
        /// Track doctor search event.
        /// </summary>
        /// <param name="searchQuery">Search query text</param>
        /// <param name="specialization">Specialization filter</param>
        /// <param name="location">Location filter</param>
        /// <param name="resultsCount">Number of results returned</param>
        void TrackDoctorSearch(string? searchQuery, string? specialization, string? location, int resultsCount);

        /// <summary>
        /// Track doctor details view event.
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        void TrackDoctorDetailsViewed(string doctorId);

        /// <summary>
        /// Track doctor profile update event.
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        void TrackDoctorProfileUpdated(string doctorId);

        /// <summary>
        /// Track doctor profile picture update event.
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        void TrackDoctorProfilePictureUpdated(string doctorId);

        /// <summary>
        /// Track daily metrics (doctor and patient counts).
        /// </summary>
        /// <param name="doctorCount">Total number of doctors</param>
        /// <param name="patientCount">Total number of patients</param>
        void TrackDailyMetrics(int doctorCount, int patientCount);
    }
}
