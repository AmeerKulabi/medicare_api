using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add additional properties as needed (e.g., IsDoctor, etc.)
        public string? FullName { get; set; }
        public bool IsDoctor { get; set; }
        
        [Required]
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Phone number must start with +964 and be followed by exactly 10 digits")]
        public string Phone { get; set; } = string.Empty;
        // If needed, you can reference Doctor/Patient profiles here
    }
}