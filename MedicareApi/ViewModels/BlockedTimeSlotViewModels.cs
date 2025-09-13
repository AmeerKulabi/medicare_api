using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class CreateBlockedTimeSlotRequest
    {
        [Required]
        public DateTime StartTime { get; set; }
        
        [Required]
        public DateTime EndTime { get; set; }
        
        public bool IsWholeDay { get; set; }
        
        [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        public string? Reason { get; set; }
        
        public bool IsRecurring { get; set; } = false;
        public string? RecurrencePattern { get; set; }
    }

    public class UpdateBlockedTimeSlotRequest
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool? IsWholeDay { get; set; }
        
        [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        public string? Reason { get; set; }
        
        public bool? IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
    }

    public class BlockedTimeSlotResponse
    {
        public string Id { get; set; } = string.Empty;
        public string DoctorId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsWholeDay { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRecurring { get; set; }
        public string? RecurrencePattern { get; set; }
    }
}