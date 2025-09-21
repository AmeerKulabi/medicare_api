using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class DoctorProfileDto
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        // تاريخ الميلاد
        public DateTime? DateOfBirth { get; set; }

        // الجنس
        [StringLength(10)]
        public string? Gender { get; set; }

        // الترخيص الطبي
        [StringLength(100)]
        public string? MedicalLicense { get; set; }


        public DateTime? LicenseExpiry { get; set; }

        // التخصص
        public string? Specialization { get; set; }
        public string? SubSpecialization { get; set; }

        // الخبرة
        [StringLength(20)]
        public int? YearsOfExperience { get; set; }

        // التعليم
        [StringLength(200)]
        public string? MedicalSchool { get; set; }

        [StringLength(10)]
        public int? GraduationYear { get; set; }

        public string? ProfessionalBiography { get; set; }   // maps to ProfessionalBiography

        // المستشفى / العيادة
        [StringLength(200)]
        public string? ClinicName { get; set; }

        [StringLength(100)]
        public string? ClinicType { get; set; }

        [StringLength(100)]
        public string? ClinicAddress { get; set; }


        // الرسوم
        [StringLength(20)]
        public int? ConsultationFee { get; set; }

        public string? City { get; set; }

        // اللغات
        public List<string>? Languages { get; set; }

        public string? Phone {  get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }
}
