using Microsoft.AspNetCore.Identity;

namespace MedicareApi.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Add additional properties as needed (e.g., IsDoctor, etc.)
        public string? FullName { get; set; }
        public bool IsDoctor { get; set; }
        // If needed, you can reference Doctor/Patient profiles here
    }
}