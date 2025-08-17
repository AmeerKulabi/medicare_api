namespace MedicareApi.ViewModels
{
    public class ProfileUpdateDto
    {
        // البريد الإلكتروني (email)
        public string? Email { get; set; }
        
        // رقم الهاتف (phone number)
        public string? Phone { get; set; }
        
        // السيرة المهنية (professional biography)
        public string? ProfessionalBiography { get; set; }
        
        // اللغات المتحدث بها (spoken languages)
        public List<string>? Languages { get; set; }
        
        // المعلومات المهنية (professional information)
        public string? Specialization { get; set; }
        public string? SubSpecialization { get; set; }
        public string? BoardCertification { get; set; }
        public string? YearsOfExperience { get; set; }
        
        // التعليم والتدريب (education & training)
        public string? MedicalSchool { get; set; }
        public string? GraduationYear { get; set; }
        public string? ResidencyProgram { get; set; }
        public string? ResidencyHospital { get; set; }
        public string? FellowshipProgram { get; set; }
        
        // الانتماءات المستشفوية (hospital affiliations)
        public string? HospitalAffiliations { get; set; }
        
        // اسم العيادة/المركز (clinic/center name)
        public string? ClinicName { get; set; }
        
        // عنوان العيادة (clinic address)
        public string? ClinicAddress { get; set; }
        
        // هاتف العيادة (clinic phone)
        public string? ClinicPhone { get; set; }
        
        // رسوم الاستشارة (consultation fee in IQD)
        public string? ConsultationFee { get; set; }
        
        // معلومات العيادة الإضافية (additional clinic information)
        public string? ClinicCity { get; set; }
        public string? ClinicState { get; set; }
        public string? ClinicZip { get; set; }
        public string? PracticeType { get; set; }
        
        // الخدمات والتوفر (services & availability)
        public List<string>? ServicesOffered { get; set; }
        public string? Availability { get; set; }
    }
}