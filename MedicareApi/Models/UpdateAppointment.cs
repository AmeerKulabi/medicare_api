namespace MedicareApi.Models
{
    public class UpdateAppointment
    {
        public DateTime? ScheduledAt { get; set; }
        public AppointmentStatus? Status { get; set; }
        public string? Reason { get; set; }
        public string? CancellationReason { get; set; }
    }
}
