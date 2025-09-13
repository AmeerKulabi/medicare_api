using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Models
{
    public class BlockedTimeSlot
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DoctorId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsWholeDay { get; set; }
        
        [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        public string? Reason { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Optional: Allow for recurring blocked slots in the future
        public bool IsRecurring { get; set; } = false;
        public string? RecurrencePattern { get; set; } // For future use: daily, weekly, etc.
    }
}