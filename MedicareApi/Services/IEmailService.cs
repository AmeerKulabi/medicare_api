using System.Threading.Tasks;

namespace MedicareApi.Services
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendPasswordResetAsync(string email, string resetLink);
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }
}