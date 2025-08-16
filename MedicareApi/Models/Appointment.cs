namespace MedicareApi.Models
{
    public class Appointment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = "Booked"; // or Cancelled, etc.
        public string? Reason { get; set; }
        // Add more fields (e.g., type, notes) as needed
    }
}