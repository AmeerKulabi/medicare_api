using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class CreateBlockedTimeSlotRequest
    {
        [Required]
        public string Date { get; set; } = string.Empty;
        
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        
        [Required]
        public bool IsWholeDay { get; set; }
        
        [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        public string? Reason { get; set; }
    }

    public class UpdateBlockedTimeSlotRequest
    {
        public string? Date { get; set; }
        public string? StartTime { get; set; }
        public string? EndTime { get; set; }
        public bool? IsWholeDay { get; set; }
        
        [StringLength(500, ErrorMessage = "Reason must not exceed 500 characters")]
        public string? Reason { get; set; }
    }

    public class BlockedTimeSlotResponse
    {
        public string id { get; set; } = string.Empty;
        public string doctorId { get; set; } = string.Empty;
        public string date { get; set; } = string.Empty;
        public string? startTime { get; set; }
        public string? endTime { get; set; }
        public bool blockWholeDay { get; set; }
        public string? reason { get; set; }
    }
}