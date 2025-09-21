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
        public string? ProfilePictureUrl { get; set; }

        // Add other doctor profile fields as needed

        // Personal Information
        public string? Specialization { get; set; }
        public string? City { get; set; }
        
        public DateTime? DateOfBirth { get; set; }  // Consider using DateTime if you're sending ISO format
        public string? Gender { get; set; }

        // Professional Information
        public string? MedicalLicense { get; set; }
        public DateTime? LicenseExpiry { get; set; }
        public string? SubSpecialization { get; set; }
        public int? YearsOfExperience { get; set; }
        public string? ProfessionalBiography { get; set; }

        // Education & Training
        public string? MedicalSchool { get; set; }
        public int? GraduationYear { get; set; }

        // Practice Information
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public string? ClinicType { get; set; }

        // Services & Availability
        public int? ConsultationFee { get; set; }
        public List<string>? Languages { get; set; }

        // Terms
        public bool? TermsAccepted { get; set; }
        public bool? PrivacyAccepted { get; set; }
    }
}