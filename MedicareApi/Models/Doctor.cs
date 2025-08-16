namespace MedicareApi.Models
{
    public class Doctor
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } // Foreign key to ApplicationUser
        public string Name { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool RegistrationCompleted { get; set; }

        // Add other doctor profile fields as needed

        // Personal Information
        public string? Specialization { get; set; }
        public string? Location { get; set; }
        
        public string? Phone { get; set; }
        public string? DateOfBirth { get; set; }  // Consider using DateTime if you're sending ISO format
        public string? Gender { get; set; }

        // Professional Information
        public string? MedicalLicense { get; set; }
        public string? LicenseState { get; set; }
        public string? LicenseExpiry { get; set; }
        public string? SubSpecialization { get; set; }
        public string? BoardCertification { get; set; }
        public string? YearsOfExperience { get; set; }
        public string? ProfessionalBiography { get; set; }

        // Education & Training
        public string? MedicalSchool { get; set; }
        public string? GraduationYear { get; set; }
        public string? ResidencyProgram { get; set; }
        public string? ResidencyHospital { get; set; }
        public string? FellowshipProgram { get; set; }

        // Practice Information
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicCity { get; set; }
        public string? ClinicState { get; set; }
        public string? ClinicZip { get; set; }
        public string? ClinicPhone { get; set; }
        public string? PracticeType { get; set; }
        public string? HospitalAffiliations { get; set; }

        // Services & Availability
        public List<string>? ServicesOffered { get; set; }
        public string? ConsultationFee { get; set; }
        public string? Availability { get; set; }
        public List<string>? Languages { get; set; }

        // Terms
        public bool? TermsAccepted { get; set; }
        public bool? PrivacyAccepted { get; set; }
    }
}