using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Models
{
    public class UpdateAppointment
    {
        public DateTime? ScheduledAt { get; set; }
        public string? Status { get; set; }
        
        [StringLength(500, ErrorMessage = "Appointment reason must not exceed 500 characters")]
        public string? Reason { get; set; }
    }
}
