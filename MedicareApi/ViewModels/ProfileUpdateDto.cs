using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class ProfileUpdateDto
    {
        // تاريخ الميلاد
        public DateTime? DateOfBirth { get; set; }

        // الجنس
        [StringLength(10)]
        public string? Gender { get; set; }

        // الترخيص الطبي
        [StringLength(100)]
        public string? MedicalLicense { get; set; }

        [StringLength(100)]
        public string? LicenseState { get; set; }

        public DateTime? LicenseExpiry { get; set; }

        // التخصص
        public string? Specialization { get; set; }
        public string? SubSpecialization { get; set; }

        // الخبرة
        [StringLength(20)]
        public string? YearsOfExperience { get; set; }

        // التعليم
        [StringLength(200)]
        public string? MedicalSchool { get; set; }

        [StringLength(10)]
        public string? GraduationYear { get; set; }

        public string? Bio { get; set; }   // maps to ProfessionalBiography
        public string? Education { get; set; }

        // المستشفى / العيادة
        [StringLength(200)]
        public string? ClinicName { get; set; }

        [StringLength(100)]
        public string? PracticeType { get; set; }

        public List<string>? ServicesOffered { get; set; }

        // الرسوم
        [StringLength(20)]
        public string? ConsultationFee { get; set; }

        // المواعيد
        public string? Availability { get; set; }

        // اللغات
        public List<string>? Languages { get; set; }

        // الشروط والخصوصية
        public bool? TermsAccepted { get; set; }
        public bool? PrivavyAccepted { get; set; }

        // صورة الملف الشخصي
        public string? ProfilePictureUrl { get; set; }
    }
}
