using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Models
{
    public class DoctorRegistrationInfo
    {
        // Personal Information
        [Required(ErrorMessage = "Phone number is required")]
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Phone number must start with +964 and be followed by exactly 10 digits")]
        public string Phone { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Board certification is required")]
        [StringLength(200, ErrorMessage = "Board certification must not exceed 200 characters")]
        public string BoardCertification { get; set; } = string.Empty;

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

        [Required(ErrorMessage = "Residency program is required")]
        [StringLength(200, ErrorMessage = "Residency program must not exceed 200 characters")]
        public string ResidencyProgram { get; set; } = string.Empty;

        [Required(ErrorMessage = "Residency hospital is required")]
        [StringLength(200, ErrorMessage = "Residency hospital must not exceed 200 characters")]
        public string ResidencyHospital { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Fellowship program must not exceed 200 characters")]
        public string? FellowshipProgram { get; set; }

        // Practice Information
        [Required(ErrorMessage = "Clinic name is required")]
        [StringLength(200, ErrorMessage = "Clinic name must not exceed 200 characters")]
        public string ClinicName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic address is required")]
        [StringLength(500, ErrorMessage = "Clinic address must not exceed 500 characters")]
        public string ClinicAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic city is required")]
        [StringLength(100, ErrorMessage = "Clinic city must not exceed 100 characters")]
        public string ClinicCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic state is required")]
        [StringLength(100, ErrorMessage = "Clinic state must not exceed 100 characters")]
        public string ClinicState { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic zip code is required")]
        [RegularExpression(@"^\d{5}$", ErrorMessage = "Clinic zip must be a 5-digit postal code")]
        public string ClinicZip { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic phone is required")]
        [RegularExpression(@"^\+964\d{10}$", ErrorMessage = "Clinic phone must start with +964 and be followed by exactly 10 digits")]
        public string ClinicPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Practice type is required")]
        [StringLength(100, ErrorMessage = "Practice type must not exceed 100 characters")]
        public string PracticeType { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Hospital affiliations must not exceed 500 characters")]
        public string? HospitalAffiliations { get; set; }

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
