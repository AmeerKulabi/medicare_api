using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 100 characters")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        public string Password { get; set; } = string.Empty;

        public bool IsDoctor { get; set; }
       
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Phone number must start with +964 and be followed by exactly 10 digits")]
        public string Phone { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public string Message { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool IsDoctor { get; set; }
        public bool IsActive { get; set; }
        public bool RegistrationCompleted { get; set; }
    }
}