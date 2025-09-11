using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Models
{
    public class DoctorRegistrationInfo
    {
        // Personal Information

        [Required(ErrorMessage = "Date of birth is required")]
        public string DateOfBirth { get; set; } = string.Empty;  // Consider using DateTime if you're sending ISO format

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression(@"^(Male|Female|Other)$", ErrorMessage = "Gender must be Male, Female, or Other")]
        public string Gender { get; set; } = string.Empty;

        // Professional Information
        [Required(ErrorMessage = "Medical license is required")]
        [StringLength(50, ErrorMessage = "Medical license must not exceed 50 characters")]
        public string MedicalLicense { get; set; } = string.Empty;

        [Required(ErrorMessage = "License state is required")]
        [StringLength(50, ErrorMessage = "License state must not exceed 50 characters")]
        public string LicenseState { get; set; } = string.Empty;

        [Required(ErrorMessage = "License expiry is required")]
        public string LicenseExpiry { get; set; } = string.Empty;

        [Required(ErrorMessage = "Specialization is required")]
        [StringLength(100, ErrorMessage = "Specialization must not exceed 100 characters")]
        public string Specialization { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Sub-specialization must not exceed 100 characters")]
        public string? SubSpecialization { get; set; }

        [Required(ErrorMessage = "Years of experience is required")]
        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50")]
        public string YearsOfExperience { get; set; } = string.Empty;

        // Education & Training
        [Required(ErrorMessage = "Medical school is required")]
        [StringLength(200, ErrorMessage = "Medical school must not exceed 200 characters")]
        public string MedicalSchool { get; set; } = string.Empty;

        [Required(ErrorMessage = "Graduation year is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Graduation year must be a 4-digit year")]
        public string GraduationYear { get; set; } = string.Empty;

        // Practice Information
        [Required(ErrorMessage = "Clinic name is required")]
        [StringLength(200, ErrorMessage = "Clinic name must not exceed 200 characters")]
        public string ClinicName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Practice type is required")]
        [StringLength(100, ErrorMessage = "Practice type must not exceed 100 characters")]
        public string PracticeType { get; set; } = string.Empty;

        // Services & Availability
        public List<string>? ServicesOffered { get; set; }

        [RegularExpression(@"^\d+(\.\d{1,2})?$", ErrorMessage = "Consultation fee must be a valid amount")]
        [StringLength(20, ErrorMessage = "Consultation fee must not exceed 20 characters")]
        public string? ConsultationFee { get; set; }

        [StringLength(1000, ErrorMessage = "Availability must not exceed 1000 characters")]
        public string? Availability { get; set; }

        public List<string>? Languages { get; set; }

        // Documents & Verification (assuming these are sent as files in multipart/form-data)

        // Terms
        [Required(ErrorMessage = "Terms acceptance is required")]
        public bool? TermsAccepted { get; set; }

        [Required(ErrorMessage = "Privacy policy acceptance is required")]
        public bool? PrivacyAccepted { get; set; }
    }

}
