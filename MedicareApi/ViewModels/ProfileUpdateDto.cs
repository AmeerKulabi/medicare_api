using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class ProfileUpdateDto
    {
        // البريد الإلكتروني (email)
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string? Email { get; set; }
        
        // رقم الهاتف (phone number)
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Phone number must start with +964 and be followed by exactly 10 digits")]
        public string? Phone { get; set; }
        
        // التخصص (specialization)
        [StringLength(100, ErrorMessage = "Specialization must not exceed 100 characters")]
        public string? Specialization { get; set; }

        // الموقع (location)
        [StringLength(100, ErrorMessage = "Location must not exceed 100 characters")]
        public string? Location { get; set; }
    }
}