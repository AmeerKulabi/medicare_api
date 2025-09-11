using System.Net;
using System.Net.Mail;

namespace MedicareApi.Services
{
    /// <summary>
    /// Email service implementation for sending emails.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="configuration">Configuration manager</param>
        /// <param name="logger">Logger</param>
        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Sends a password reset email to the specified email address.
        /// </summary>
        /// <param name="email">Recipient email address</param>
        /// <param name="resetLink">Password reset link</param>
        /// <param name="appName">Application name</param>
        /// <returns>Task representing the async operation</returns>
        public async Task SendPasswordResetEmailAsync(string email, string resetLink, string appName)
        {
            try
            {
                var htmlTemplate = GetPasswordResetTemplate(resetLink, appName);
                
                using var client = CreateSmtpClient();
                using var message = new MailMessage
                {
                    From = new MailAddress(_configuration["Email:FromAddress"] ?? "noreply@medicare.app", 
                                         _configuration["Email:FromName"] ?? "Medicare App"),
                    Subject = "إعادة تعيين كلمة المرور - " + appName,
                    Body = htmlTemplate,
                    IsBodyHtml = true
                };

                message.To.Add(email);
                
                await client.SendMailAsync(message);
                _logger.LogInformation("Password reset email sent successfully to {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Creates and configures SMTP client.
        /// </summary>
        /// <returns>Configured SMTP client</returns>
        private SmtpClient CreateSmtpClient()
        {
            var client = new SmtpClient
            {
                Host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com",
                Port = int.Parse(_configuration["Email:SmtpPort"] ?? "587"),
                EnableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true"),
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    _configuration["Email:Username"] ?? throw new InvalidOperationException("Email username not configured"),
                    _configuration["Email:Password"] ?? throw new InvalidOperationException("Email password not configured")
                )
            };

            return client;
        }

        /// <summary>
        /// Gets the HTML template for password reset email with placeholders replaced.
        /// </summary>
        /// <param name="resetLink">Password reset link</param>
        /// <param name="appName">Application name</param>
        /// <returns>HTML email template</returns>
        private static string GetPasswordResetTemplate(string resetLink, string appName)
        {
            var template = @"
<!DOCTYPE html>
<html dir=""rtl"" lang=""ar"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>إعادة تعيين كلمة المرور</title>
</head>
<body style=""margin: 0; padding: 0; font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; background-color: #f5f5f5;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f5f5f5; padding: 20px;"">
        <tr>
            <td align=""center"">
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #ffffff; border-radius: 10px; box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1); overflow: hidden;"">
                    <tr>
                        <td style=""background: linear-gradient(135deg, #1976d2 0%, #1565c0 100%); padding: 40px 30px; text-align: center;"">
                            <h1 style=""color: #ffffff; margin: 0; font-size: 28px; font-weight: 600;"">{{app_name}}</h1>
                            <p style=""color: #e3f2fd; margin: 10px 0 0 0; font-size: 16px;"">تطبيقك الموثوق للخدمات الطبية</p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""padding: 40px 30px;"">
                            <h2 style=""color: #333333; margin: 0 0 20px 0; font-size: 24px; font-weight: 600; text-align: center;"">إعادة تعيين كلمة المرور</h2>
                            
                            <p style=""color: #333333; margin: 0 0 15px 0; font-size: 16px; line-height: 1.6;"">
                                عزيزي المستخدم،
                            </p>
                            
                            <p style=""color: #333333; margin: 0 0 15px 0; font-size: 16px; line-height: 1.6;"">
                                لقد تلقينا طلبًا لإعادة تعيين كلمة المرور الخاصة بحسابك في <strong>{{app_name}}</strong>.
                            </p>
                            
                            <p style=""color: #333333; margin: 0 0 25px 0; font-size: 16px; line-height: 1.6;"">
                                إذا كنت أنت من طلب إعادة التعيين، يرجى الضغط على الزر أدناه لإعادة تعيين كلمة المرور:
                            </p>
                            
                            <div style=""text-align: center; margin: 30px 0;"">
                                <a href=""{{reset_link}}"" style=""display: inline-block; background: linear-gradient(135deg, #4caf50 0%, #45a049 100%); color: #ffffff; text-decoration: none; padding: 15px 30px; border-radius: 50px; font-size: 16px; font-weight: 600; box-shadow: 0 4px 15px rgba(76, 175, 80, 0.3); transition: all 0.3s ease;"">
                                    إعادة تعيين كلمة المرور
                                </a>
                            </div>
                            
                            <p style=""color: #666666; margin: 25px 0 0 0; font-size: 14px; line-height: 1.6; background-color: #f9f9f9; padding: 15px; border-radius: 8px; border-right: 4px solid #ff9800;"">
                                <strong>تنبيه:</strong> إذا لم تطلب إعادة تعيين كلمة المرور، يمكنك تجاهل هذه الرسالة، ولن يتم تغيير أي شيء في حسابك.
                            </p>
                            
                            <p style=""color: #999999; margin: 20px 0 0 0; font-size: 14px; line-height: 1.6;"">
                                في حالة عدم عمل الزر أعلاه، يمكنك نسخ الرابط التالي ولصقه في متصفحك:
                                <br>
                                <a href=""{{reset_link}}"" style=""color: #1976d2; word-break: break-all;"">{{reset_link}}</a>
                            </p>
                        </td>
                    </tr>
                    <tr>
                        <td style=""background-color: #fafafa; padding: 30px; text-align: center; border-top: 1px solid #eeeeee;"">
                            <p style=""color: #666666; margin: 0 0 10px 0; font-size: 14px;"">
                                إذا واجهتك أي مشكلة، يرجى التواصل مع فريق الدعم
                            </p>
                            <p style=""color: #1976d2; margin: 0; font-size: 16px; font-weight: 600;"">
                                مع تحيات فريق <strong>{{app_name}}</strong>
                            </p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

            return template
                .Replace("{{reset_link}}", resetLink)
                .Replace("{{app_name}}", appName);
        }
    }
}