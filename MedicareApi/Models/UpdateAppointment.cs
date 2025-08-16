namespace MedicareApi.Models
{
    public class UpdateAppointment
    {
        public DateTime? ScheduledAt { get; set; }
        public string? Status { get; set; }
        public string? Reason { get; set; }
    }
}
