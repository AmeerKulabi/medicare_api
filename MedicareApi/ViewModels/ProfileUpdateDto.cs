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
        
        [StringLength(200, ErrorMessage = "Residency program must not exceed 200 characters")]
        public string? ResidencyProgram { get; set; }
        
        [StringLength(200, ErrorMessage = "Residency hospital must not exceed 200 characters")]
        public string? ResidencyHospital { get; set; }
        
        [StringLength(200, ErrorMessage = "Fellowship program must not exceed 200 characters")]
        public string? FellowshipProgram { get; set; }
        
        // الانتماءات المستشفوية (hospital affiliations)
        [StringLength(500, ErrorMessage = "Hospital affiliations must not exceed 500 characters")]
        public string? HospitalAffiliations { get; set; }
        
        // اسم العيادة/المركز (clinic/center name)
        [StringLength(200, ErrorMessage = "Clinic name must not exceed 200 characters")]
        public string? ClinicName { get; set; }
        
        // عنوان العيادة (clinic address)
        [StringLength(500, ErrorMessage = "Clinic address must not exceed 500 characters")]
        public string? ClinicAddress { get; set; }
        
        // هاتف العيادة (clinic phone)
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Clinic phone must start with +964 and be followed by exactly 10 digits")]
        public string? ClinicPhone { get; set; }
        
        // رسوم الاستشارة (consultation fee in IQD)
        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Consultation fee must be a valid amount")]
        [StringLength(20, ErrorMessage = "Consultation fee must not exceed 20 characters")]
        public string? ConsultationFee { get; set; }
        
        // صورة الملف الشخصي (profile picture) - Note: IFormFile is used for multipart uploads
        // This will be handled separately in the controller for multipart/form-data requests
    }
}