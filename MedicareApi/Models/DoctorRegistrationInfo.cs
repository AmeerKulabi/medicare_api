using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Models
{
    public class DoctorRegistrationInfo
    {
        // Personal Information
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Phone number must start with +964 and be followed by exactly 10 digits")]
        public string Phone { get; set; } = string.Empty;

        // Professional Information
        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100, ErrorMessage = "Specialization must not exceed 100 characters")]
        public string Specialization { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        [StringLength(100, ErrorMessage = "Location must not exceed 100 characters")]
        public string Location { get; set; } = string.Empty;
    }
}
