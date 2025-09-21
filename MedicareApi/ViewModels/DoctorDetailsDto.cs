namespace MedicareApi.ViewModels
{
    public class DoctorDetailsDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public string? City { get; set; }
        public string? SubSpecialization { get; set; }
        public int YearsOfExperience { get; set; } = 0;
        public string? ProfessionalBiography { get; set; }
        public string? MedicalSchool { get; set; }
        public int? GraduationYear { get; set; }
        public string? ClinicName { get; set; }
        public string? ClinicAddress { get; set; }
        public List<string> Languages { get; set; } = new List<string>();
        public string? Phone {  get; set; }
    }
}