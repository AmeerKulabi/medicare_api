using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class ProfileUpdateDto
    {
        // البريد الإلكتروني (email)
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string? Email { get; set; }
        
        // السيرة المهنية (professional biography)
        [StringLength(2000, ErrorMessage = "Professional biography must not exceed 2000 characters")]
        public string? ProfessionalBiography { get; set; }
        
        // اللغات المتحدث بها (spoken languages)
        public List<string>? Languages { get; set; }
        
        // التعليم والتدريب (education & training)
        [StringLength(200, ErrorMessage = "Medical school name must not exceed 200 characters")]
        public string? MedicalSchool { get; set; }
        
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Graduation year must be a 4-digit year")]
        public string? GraduationYear { get; set; }
        
        // اسم العيادة/المركز (clinic/center name)
        [StringLength(200, ErrorMessage = "Clinic name must not exceed 200 characters")]
        public string? ClinicName { get; set; }
        
        // رسوم الاستشارة (consultation fee in IQD)
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Consultation fee must be a valid amount")]
        [StringLength(20, ErrorMessage = "Consultation fee must not exceed 20 characters")]
        public string? ConsultationFee { get; set; }

        [RegularExpression(@"^\d{1,2}$", ErrorMessage = "Years of experience must be between 0-99 years")]
        public string? YearsOfExperience { get; set; }

        public string? Specialization {  get; set; }

        public string? Location { get; set; }
        
        // صورة الملف الشخصي (profile picture) - Note: IFormFile is used for multipart uploads
        // This will be handled separately in the controller for multipart/form-data requests
    }
}