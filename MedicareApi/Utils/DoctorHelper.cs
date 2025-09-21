using MedicareApi.ViewModels;
using MedicareApi.Models;

namespace MedicareApi.Utils
{
    public static class DoctorHelper
    {
        public static DoctorListingItem FromDoctorToDoctorListingItem(Doctor doctor, ApplicationUser user)
        {
            return new DoctorListingItem()
            {
                Id = doctor.Id,
                Name = doctor.Name,
                Email = doctor.Email,
                Experience = doctor.YearsOfExperience ?? 0,
                ConsultationFee = doctor.ConsultationFee ?? 0,
                Languages = doctor.Languages,
                Specialization = doctor.Specialization,
                SubSpecialization = doctor.SubSpecialization,
                Phone = user.Phone,
                ProfilePictureUrl = doctor.ProfilePictureUrl,
                City = doctor.City
            };
        }

        public static DoctorDetailsDto FromDoctorToDoctorDetailsDto(Doctor doctor, ApplicationUser user)
        {
            return new DoctorDetailsDto()
            {
                Name = doctor.Name ?? string.Empty,
                Email = doctor.Email ?? string.Empty,
                Specialization = doctor.Specialization,
                City = doctor.City,
                SubSpecialization = doctor.SubSpecialization,
                YearsOfExperience = doctor.YearsOfExperience ?? 0,
                ProfessionalBiography = doctor.ProfessionalBiography,
                MedicalSchool = doctor.MedicalSchool,
                GraduationYear = doctor.GraduationYear,
                ClinicName = doctor.ClinicName,
                ClinicAddress = doctor.ClinicAddress,
                Languages = doctor.Languages ?? new List<string>(),
                Phone = user.Phone
            };
        }

        public static DoctorProfileDto FromDoctorToDoctorProfileDto(Doctor doctor, ApplicationUser user)
        {
            return new DoctorProfileDto()
            {
                Id = doctor.Id,
                Name = doctor.Name ?? string.Empty,
                Email = doctor.Email ?? string.Empty,
                DateOfBirth = doctor.DateOfBirth,
                Gender = doctor.Gender,
                ConsultationFee = doctor.ConsultationFee,
                YearsOfExperience = doctor.YearsOfExperience,
                Phone = user.Phone,
                Specialization = doctor.Specialization,
                SubSpecialization = doctor.SubSpecialization,
                City = doctor.City,
                ClinicAddress = doctor.ClinicAddress,
                ClinicName = doctor.ClinicName,
                ClinicType = doctor.ClinicType,
                MedicalLicense = doctor.MedicalLicense,
                LicenseExpiry = doctor.LicenseExpiry,
                ProfessionalBiography = doctor.ProfessionalBiography,
                GraduationYear = doctor.GraduationYear,
                MedicalSchool = doctor.MedicalSchool,
                Languages = doctor.Languages,
                ProfilePictureUrl = doctor.ProfilePictureUrl
            };
        }
    }
}
