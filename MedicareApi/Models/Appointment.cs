namespace MedicareApi.Models
{
    public enum AppointmentStatus
    {
        Booked,
        Confirmed,
        Done,
        Canceled
    }

    public class Appointment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string PatientId { get; set; }
        public string DoctorId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
        public string? Reason { get; set; }
        public decimal ConsultationFee { get; set; }
        public string? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string? CanceledBy { get; set; } // PatientId or DoctorId
        public string? CancellationReason { get; set; }
    }
}