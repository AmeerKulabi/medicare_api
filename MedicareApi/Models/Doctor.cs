namespace MedicareApi.Models
{
    public class Doctor
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } // Foreign key to ApplicationUser
        public string Name { get; set; }
        public string Email { get; set; }
        public string? Specialization { get; set; }
        public string? Location { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public bool RegistrationCompleted { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}