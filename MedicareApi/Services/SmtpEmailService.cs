using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MedicareApi.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            var subject = "Confirm your Medicare API account";
            var body = $@"
                <html>
                <body>
                    <h2>Welcome to Medicare API</h2>
                    <p>Please confirm your email address by clicking the link below:</p>
                    <p><a href='{confirmationLink}'>Confirm Email Address</a></p>
                    <p>If you didn't create this account, you can safely ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Medicare API Team</p>
                </body>
                </html>";
            
            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "Reset your Medicare API password";
            var body = $@"
                <html>
                <body>
                    <h2>Password Reset Request</h2>
                    <p>You requested to reset your password. Click the link below to set a new password:</p>
                    <p><a href='{resetLink}'>Reset Password</a></p>
                    <p>This link will expire in 24 hours.</p>
                    <p>If you didn't request this password reset, you can safely ignore this email.</p>
                    <br>
                    <p>Best regards,<br>Medicare API Team</p>
                </body>
                </html>";
            
            await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                
                using var client = new SmtpClient(smtpSettings["Host"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(
                        smtpSettings["Username"],
                        smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpSettings["FromEmail"] ?? smtpSettings["Username"]!, 
                                         smtpSettings["FromName"] ?? "Medicare API"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {Email}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", to);
                return false;
            }
        }
    }
}