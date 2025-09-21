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
    }
}
