using MedicareApi.Models;
using System.Threading.Tasks;

namespace MedicareApi.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendPasswordResetAsync(string email, string resetLink);
        Task<bool> SendEmailAsync(string to, string subject, string body);
        Task SendAppointmentDeleted(string email, DateTime aptTime, string doctorName, string doctorAddress, string DoctorPhoneNumber);
        Task SendAppointmentChanged(string email, DateTime aptTimeOld, DateTime aptTimeNew, string doctorName, string doctorAddress, string DoctorPhoneNumber);
        Task SendAppointmentBooked(string email, DateTime aptTime, string doctorName, string doctorAddress, string DoctorPhoneNumber);
    }
}