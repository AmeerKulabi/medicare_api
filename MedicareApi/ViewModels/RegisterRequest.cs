namespace MedicareApi.ViewModels
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public bool IsDoctor { get; set; }
    }

    public class RegisterResponse
    {
        public string UserId { get; set; }
        public bool IsActive { get; set; }
        public bool RegistrationCompleted { get; set; }
    }
}