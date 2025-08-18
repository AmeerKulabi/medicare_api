using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public bool IsDoctor { get; set; }
        
        [Required]
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Phone number must start with +964 and be followed by exactly 10 digits")]
        public string Phone { get; set; } = string.Empty;
    }

    public class RegisterResponse
    {
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public bool RegistrationCompleted { get; set; }
    }
}