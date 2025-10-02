using MedicareApi.Models;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MedicareApi.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly IEmailTemplateService _templateService;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger, IEmailTemplateService templateService)
        {
            _configuration = configuration;
            _logger = logger;
            _templateService = templateService;
        }

        public async Task SendAppointmentBooked(string email, DateTime aptTime, string doctorName, string doctorAddress, string DoctorPhoneNumber)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("ar-IQ"); // may still throw if not installed
            var dateStr = aptTime.ToLocalTime().ToString("dd MMMM yyyy", culture);
            var timeStr = aptTime.ToLocalTime().ToString("hh:mm tt", new System.Globalization.CultureInfo("ar-IQ")) ?? "غير متوفر";
            var doctor = doctorName ?? "غير متوفر";
            var address = doctorAddress ?? "غير متوفر";
            var phone = DoctorPhoneNumber ?? "غير متوفر";
            var subject = "New appointment booked";
            var body = $@"
                <!DOCTYPE html>
                <html lang=""ar"" dir=""rtl"" style=""background: #f8fafb;"">
                <head>
                    <meta charset=""UTF-8"">
                    <title>تأكيد الموعد</title>
                    <!-- Tajawal font for Arabic, fallback to system fonts -->
                    <link rel=""stylesheet"" href=""https://fonts.googleapis.com/css?family=Tajawal:wght@400;700&display=swap"">
                </head>
                <body style=""font-family: 'Tajawal', Arial, Helvetica, sans-serif; background-color: #f8fafb; margin: 0; padding: 0;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8fafb; padding: 40px 0;"">
                        <tr>
                            <td align=""center"">
                                <table width=""420"" cellpadding=""0"" cellspacing=""0"" style=""background: #fff; border-radius: 18px; box-shadow: 0 2px 8px #e0e4e9; overflow: hidden;"">
                                    <!-- Header / Brand -->
                                    <tr>
                                        <td align=""center"" style=""background: #eafaf0; padding: 24px 0;"">
                                            <div style=""display:inline-block;box-shadow:0 4px 12px rgba(46,196,146,0.3);border-radius:12px;"">
                                                <svg width=""64"" height=""64"" viewBox=""0 0 64 64"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                                                    <rect x=""4"" y=""4"" width=""56"" height=""56"" rx=""12"" ry=""12"" fill=""url(#gradient1)"" />
                                                    <path d=""M32 50c0 0-16-10.5-16-20.5 0-5.5 4.5-10 10-10 3 0 6 1.5 6 1.5s3-1.5 6-1.5c5.5 0 10 4.5 10 10 0 10-16 20.5-16 20.5z"" 
                                                          stroke=""#fff"" 
                                                          stroke-width=""2.5"" 
                                                          fill=""none"" 
                                                          stroke-linecap=""round"" 
                                                          stroke-linejoin=""round""/>
                                                    <defs>
                                                        <linearGradient id=""gradient1"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
                                                            <stop offset=""0%"" style=""stop-color:#2EC492;stop-opacity:1"" />
                                                            <stop offset=""100%"" style=""stop-color:#26b47f;stop-opacity:1"" />
                                                        </linearGradient>
                                                    </defs>
                                                </svg>
                                            </div>
                                            <div style=""font-size: 20px; font-weight: bold; color: #222; margin-top: 12px;"">صحتك</div>
                                        </td>
                                    </tr>
                                    <!-- Main Content -->
                                    <tr>
                                        <td style=""padding: 32px 24px;"">
                                            <h2 style=""margin: 0 0 12px 0; color: #222; font-size: 22px; font-weight: bold; text-align: right;"">
                                                تم حجز موعد جديد بنجاح
                                            </h2>
                                            <p style=""margin: 0 0 8px 0; color: #555; font-size: 16px;"">شكرًا لاختياركم منصة صحتك. تفاصيل موعدكم كالتالي:</p>
                                            <div style=""background: #eafaf0; border-radius: 10px; padding: 18px 15px; margin: 12px 0 18px 0; color: #258f62; font-size: 16px;"">
                                                <strong>التاريخ:</strong>
                                                <span>
                                                    {dateStr}
                                                </span>
                                                <br>
                                                <strong>الوقت:</strong>
                                                <span>
                                                    {timeStr}
                                                </span>
                                                <br>
                                                <strong>الطبيب:</strong>
                                                <span>
                                                    {doctor}
                                                </span>
                                                <br>
                                                <strong>العنوان:</strong>
                                                <span>
                                                    {address}
                                                </span>
                                                <br>
                                                <strong>هاتف الطبيب:</strong>
                                                <span>
                                                    {phone}
                                                </span>
                                            </div>
                                            <!-- Guidance section -->
                                            <div style=""background: #f4f7fa; border-radius: 10px; padding: 16px 12px; color: #31786a; font-size: 15px; margin-bottom: 18px;"">
                                                <strong>إرشادات لتغيير أو إلغاء الموعد:</strong>
                                                <ul style=""padding-right: 20px; margin: 8px 0 0 0; color: #31786a;"">
                                                    <li>قم بتسجيل الدخول إلى منصة صحتك.</li>
                                                    <li>اختر ""مواعيدي"" من القائمة.</li>
                                                    <li>حدد الموعد الذي ترغب في تعديله أو إلغائه.</li>
                                                    <li>يمكنك إلغاء أو إعادة جدولة الموعد من صفحة التفاصيل.</li>
                                                </ul>
                                            </div>
                                            <hr style=""border: none; border-top: 1px solid #e0e4e9; margin: 18px 0;"">
                                            <p style=""color: #999; font-size: 14px; text-align: right;"">
                                                مع أطيب التحيات،<br>
                                                فريق منصة صحتك
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                                <div style=""margin-top: 18px; color: #b3b3b3; font-size: 12px; text-align: center;"">
                                    جميع الحقوق محفوظة © صحتك {DateTime.Now.Year}
                                </div>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAppointmentChanged(string email, DateTime aptTimeOld, DateTime aptTimeNew, string doctorName, string doctorAddress, string DoctorPhoneNumber)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("ar-IQ"); // may still throw if not installed
            var dateStrOld = aptTimeOld.ToLocalTime().ToString("dd MMMM yyyy", culture);
            var dateStrNew = aptTimeNew.ToLocalTime().ToString("dd MMMM yyyy", culture);
            var timeStrOld = aptTimeOld.ToLocalTime().ToString("hh:mm tt", new System.Globalization.CultureInfo("ar-IQ")) ?? "غير متوفر";
            var timeStrNew = aptTimeNew.ToLocalTime().ToString("hh:mm tt", new System.Globalization.CultureInfo("ar-IQ")) ?? "غير متوفر";
            var doctor = doctorName ?? "غير متوفر";
            var address = doctorAddress ?? "غير متوفر";
            var phone = DoctorPhoneNumber ?? "غير متوفر";
            var subject = "Appointment changed";
            var body = $@"
                <!DOCTYPE html>
                <html lang=""ar"" dir=""rtl"" style=""background: #f8fafb;"">
                <head>
                    <meta charset=""UTF-8"">
                    <title>تغيير وقت الموعد</title>
                    <link rel=""stylesheet"" href=""https://fonts.googleapis.com/css?family=Tajawal:wght@400;700&display=swap"">
                </head>
                <body style=""font-family: 'Tajawal', Arial, Helvetica, sans-serif; background-color: #f8fafb; margin: 0; padding: 0;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8fafb; padding: 40px 0;"">
                        <tr>
                            <td align=""center"">
                                <table width=""520"" cellpadding=""0"" cellspacing=""0"" style=""background: #fff; border-radius: 18px; box-shadow: 0 2px 8px #e0e4e9; overflow: hidden;"">
                                    <tr>
                                        <td align=""center"" style=""background: #eafaf0; padding: 24px 0;"">
                                            <div style=""display:inline-block;box-shadow:0 4px 12px rgba(46,196,146,0.3);border-radius:12px;"">
                                                <svg width=""64"" height=""64"" viewBox=""0 0 64 64"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                                                    <rect x=""4"" y=""4"" width=""56"" height=""56"" rx=""12"" ry=""12"" fill=""url(#gradient2)"" />
                                                    <path d=""M32 50c0 0-16-10.5-16-20.5 0-5.5 4.5-10 10-10 3 0 6 1.5 6 1.5s3-1.5 6-1.5c5.5 0 10 4.5 10 10 0 10-16 20.5-16 20.5z"" 
                                                          stroke=""#fff"" 
                                                          stroke-width=""2.5"" 
                                                          fill=""none"" 
                                                          stroke-linecap=""round"" 
                                                          stroke-linejoin=""round""/>
                                                    <defs>
                                                        <linearGradient id=""gradient2"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
                                                            <stop offset=""0%"" style=""stop-color:#2EC492;stop-opacity:1"" />
                                                            <stop offset=""100%"" style=""stop-color:#26b47f;stop-opacity:1"" />
                                                        </linearGradient>
                                                    </defs>
                                                </svg>
                                            </div>
                                            <div style=""font-size: 20px; font-weight: bold; color: #222; margin-top: 12px;"">صحتك</div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 32px 24px;"">
                                            <h2 style=""margin: 0 0 12px 0; color: #222; font-size: 22px; font-weight: bold; text-align: right;"">
                                                تم تغيير وقت الموعد بنجاح
                                            </h2>
                                            <p style=""margin: 0 0 8px 0; color: #555; font-size: 16px;"">تم تغيير وقت موعدك. التفاصيل كالتالي:</p>
                                            <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom: 18px;"">
                                                <tr>
                                                    <td valign=""top"" style=""width:48%;background:#f4f7fa;border-radius:10px;padding:12px 10px;margin:0 1%;color:#31786a;font-size:15px;"">
                                                        <strong>الوقت القديم</strong><br>
                                                        <strong>التاريخ:</strong>
                                                        <span>{dateStrOld}</span><br>
                                                        <strong>الوقت:</strong>
                                                        <span>{timeStrOld}</span><br>
                                                        <strong>الطبيب:</strong>
                                                        <span>{doctor}</span><br>
                                                        <strong>العنوان:</strong>
                                                        <span>{address}</span><br>
                                                        <strong>هاتف الطبيب:</strong>
                                                        <span>{phone}</span>
                                                    </td>
                                                    <td style=""width:4%;""></td>
                                                    <td valign=""top"" style=""width:48%;background:#eafaf0;border-radius:10px;padding:12px 10px;margin:0 1%;color:#258f62;font-size:15px;"">
                                                        <strong>الوقت الجديد</strong><br>
                                                        <strong>التاريخ:</strong>
                                                        <span>{dateStrNew}</span><br>
                                                        <strong>الوقت:</strong>
                                                        <span>{timeStrNew}</span><br>
                                                        <strong>الطبيب:</strong>
                                                        <span>{doctor}</span><br>
                                                        <strong>العنوان:</strong>
                                                        <span>{address}</span><br>
                                                        <strong>هاتف الطبيب:</strong>
                                                        <span>{phone}</span>
                                                    </td>
                                                </tr>
                                            </table>
                                            <div style=""background: #f4f7fa; border-radius: 10px; padding: 16px 12px; color: #31786a; font-size: 15px; margin-bottom: 18px;"">
                                                <strong>لإلغاء أو إعادة جدولة الموعد:</strong>
                                                <ul style=""padding-right: 20px; margin: 8px 0 0 0; color: #31786a;"">
                                                    <li>سجّل الدخول إلى منصة صحتك.</li>
                                                    <li>انتقل إلى ""مواعيدي"".</li>
                                                    <li>اختر الموعد المطلوب.</li>
                                                    <li>يمكنك الإلغاء أو إعادة الجدولة من صفحة التفاصيل.</li>
                                                </ul>
                                            </div>
                                            <hr style=""border: none; border-top: 1px solid #e0e4e9; margin: 18px 0;"">
                                            <p style=""color: #999; font-size: 14px; text-align: right;"">
                                                مع أطيب التحيات،<br>
                                                فريق منصة صحتك
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                                <div style=""margin-top: 18px; color: #b3b3b3; font-size: 12px; text-align: center;"">
                                    جميع الحقوق محفوظة © صحتك {{DateTime.Now.Year}}
                                </div>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAppointmentDeleted(string email, DateTime aptTime, string doctorName, string doctorAddress, string DoctorPhoneNumber)
        {
            var culture = System.Globalization.CultureInfo.GetCultureInfo("ar-IQ"); // may still throw if not installed
            var dateStr = aptTime.ToLocalTime().ToString("dd MMMM yyyy", culture);
            var timeStr = aptTime.ToLocalTime().ToString("hh:mm tt", new System.Globalization.CultureInfo("ar-IQ")) ?? "غير متوفر";
            var doctor = doctorName ?? "غير متوفر";
            var address = doctorAddress ?? "غير متوفر";
            var phone = DoctorPhoneNumber ?? "غير متوفر";
            var subject = "Appointment deleted";
            var body = $@"
                <!DOCTYPE html>
                <html lang=""ar"" dir=""rtl"" style=""background: #f8fafb;"">
                <head>
                    <meta charset=""UTF-8"">
                    <title>إلغاء الموعد</title>
                    <link rel=""stylesheet"" href=""https://fonts.googleapis.com/css?family=Tajawal:wght@400;700&display=swap"">
                </head>
                <body style=""font-family: 'Tajawal', Arial, Helvetica, sans-serif; background-color: #f8fafb; margin: 0; padding: 0;"">
                    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color: #f8fafb; padding: 40px 0;"">
                        <tr>
                            <td align=""center"">
                                <table width=""420"" cellpadding=""0"" cellspacing=""0"" style=""background: #fff; border-radius: 18px; box-shadow: 0 2px 8px #e0e4e9; overflow: hidden;"">
                                    <tr>
                                        <td align=""center"" style=""background: #eafaf0; padding: 24px 0;"">
                                            <div style=""display:inline-block;box-shadow:0 4px 12px rgba(46,196,146,0.3);border-radius:12px;"">
                                                <svg width=""64"" height=""64"" viewBox=""0 0 64 64"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"">
                                                    <rect x=""4"" y=""4"" width=""56"" height=""56"" rx=""12"" ry=""12"" fill=""url(#gradient3)"" />
                                                    <path d=""M32 50c0 0-16-10.5-16-20.5 0-5.5 4.5-10 10-10 3 0 6 1.5 6 1.5s3-1.5 6-1.5c5.5 0 10 4.5 10 10 0 10-16 20.5-16 20.5z"" 
                                                          stroke=""#fff"" 
                                                          stroke-width=""2.5"" 
                                                          fill=""none"" 
                                                          stroke-linecap=""round"" 
                                                          stroke-linejoin=""round""/>
                                                    <defs>
                                                        <linearGradient id=""gradient3"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""100%"">
                                                            <stop offset=""0%"" style=""stop-color:#2EC492;stop-opacity:1"" />
                                                            <stop offset=""100%"" style=""stop-color:#26b47f;stop-opacity:1"" />
                                                        </linearGradient>
                                                    </defs>
                                                </svg>
                                            </dev>
                                            <div style=""font-size: 20px; font-weight: bold; color: #222; margin-top: 12px;"">صحتك</div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style=""padding: 32px 24px;"">
                                            <h2 style=""margin: 0 0 12px 0; color: #d83a3a; font-size: 22px; font-weight: bold; text-align: right;"">
                                                تم إلغاء الموعد بنجاح
                                            </h2>
                                            <p style=""margin: 0 0 8px 0; color: #555; font-size: 16px;"">تم إلغاء موعدك بنجاح. نأمل أن نراك قريباً في منصة صحتك.</p>
                                            <div style=""background: #ffeaea; border-radius: 10px; padding: 18px 15px; margin: 12px 0 18px 0; color: #d83a3a; font-size: 16px;"">
                                                <strong>التفاصيل الملغاة:</strong><br>
                                                <strong>التاريخ:</strong>
                                                <span>
                                                    {dateStr}
                                                </span>
                                                <br>
                                                <strong>الوقت:</strong>
                                                <span>
                                                    {timeStr}
                                                </span>
                                                <br>
                                                <strong>الطبيب:</strong>
                                                <span>{doctor}</span>
                                                <br>
                                                <strong>العنوان:</strong>
                                                <span>{address}</span>
                                                <br>
                                                <strong>هاتف الطبيب:</strong>
                                                <span>{phone}</span>
                                            </div>
                                            <div style=""background: #f4f7fa; border-radius: 10px; padding: 16px 12px; color: #31786a; font-size: 15px; margin-bottom: 18px;"">
                                                <strong>لإعادة حجز موعد جديد:</strong>
                                                <ul style=""padding-right: 20px; margin: 8px 0 0 0; color: #31786a;"">
                                                    <li>توجه إلى منصة صحتك وسجّل الدخول.</li>
                                                    <li>ابحث عن الطبيب الذي ترغب في الحجز لديه.</li>
                                                    <li>احجز موعداً جديداً بسهولة.</li>
                                                </ul>
                                            </div>
                                            <hr style=""border: none; border-top: 1px solid #e0e4e9; margin: 18px 0;"">
                                            <p style=""color: #999; font-size: 14px; text-align: right;"">
                                                مع أطيب التحيات،<br>
                                                فريق منصة صحتك
                                            </p>
                                        </td>
                                    </tr>
                                </table>
                                <div style=""margin-top: 18px; color: #b3b3b3; font-size: 12px; text-align: center;"">
                                    جميع الحقوق محفوظة © صحتك {{DateTime.Now.Year}}
                                </div>
                            </td>
                        </tr>
                    </table>
                </body>
                </html>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
        {
            try
            {
                var variables = new Dictionary<string, string>
                {
                    ["confirmation_link"] = confirmationLink
                };

                var body = await _templateService.RenderTemplateAsync("EmailConfirmation", variables);
                var subject = "تأكيد حسابك في منصة صحتك";

                await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email confirmation to {Email}", email);
                throw;
            }
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            try
            {
                var variables = new Dictionary<string, string>
                {
                    ["reset_link"] = resetLink
                };

                var body = await _templateService.RenderTemplateAsync("PasswordReset", variables);
                var subject = "إعادة تعيين كلمة المرور - منصة صحتك";

                await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send password reset email to {Email}", email);
                throw;
            }
        }

        public async Task SendWelcomeAsync(string email, string websiteUrl)
        {
            try
            {
                var variables = new Dictionary<string, string>
                {
                    ["website_url"] = websiteUrl
                };

                var body = await _templateService.RenderTemplateAsync("Welcome", variables);
                var subject = "أهلاً وسهلاً بك في منصة صحتك";

                await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
                throw;
            }
        }

        public async Task SendAppointmentReminderAsync(string email, DateTime aptTime, string doctorName, string doctorAddress, string doctorPhone, string? reason = null)
        {
            try
            {
                var culture = System.Globalization.CultureInfo.GetCultureInfo("ar-IQ");
                var dateStr = aptTime.ToLocalTime().ToString("dd MMMM yyyy", culture);
                var timeStr = aptTime.ToLocalTime().ToString("hh:mm tt", new System.Globalization.CultureInfo("ar-IQ")) ?? "غير متوفر";

                var variables = new Dictionary<string, string>
                {
                    ["appointment_date"] = dateStr,
                    ["appointment_time"] = timeStr,
                    ["doctor_name"] = doctorName ?? "غير متوفر",
                    ["doctor_address"] = doctorAddress ?? "غير متوفر",
                    ["doctor_phone"] = doctorPhone ?? "غير متوفر",
                    ["appointment_reason"] = reason ?? string.Empty,
                    ["manage_appointment_url"] = _configuration["Website:BaseUrl"] + "/appointments" ?? "#"
                };

                var body = await _templateService.RenderTemplateAsync("AppointmentReminder", variables);
                var subject = "تذكير بموعدك الطبي القادم - صحتك";

                await SendEmailAsync(email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send appointment reminder email to {Email}", email);
                throw;
            }
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