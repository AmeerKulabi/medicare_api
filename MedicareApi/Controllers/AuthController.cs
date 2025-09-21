using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using MedicareApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace MedicareApi.Controllers
{
    /// <summary>
    /// Controller for adding new user and signing in.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// Identity framework, user manager.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Identity framework, sign in manager.
        /// </summary>
        private readonly SignInManager<ApplicationUser> _signInManager;

        /// <summary>
        /// Configuration mananger.
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// Access db.
        /// </summary>
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Email service.
        /// </summary>
        private readonly IEmailService _emailService;

        /// <summary>
        /// Web host environment.
        /// </summary>
        private readonly IWebHostEnvironment _webHostEnvironment;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="userManager">Identity framework, user manager.</param>
        /// <param name="signInManager">Identity framework, sign in manager.</param>
        /// <param name="configuration">Configuration mananger.</param>
        /// <param name="db">Access db.</param>
        /// <param name="emailService">Email service.</param>
        /// <param name="webHostEnvironment">Web host environment.</param>
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration,
            ApplicationDbContext db, IEmailService emailService, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _db = db;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
        }

        /// <summary>
        /// Registers new user.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPost("register")]
        public async Task<IActionResult> Register(MedicareApi.ViewModels.RegisterRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IsDoctor = model.IsDoctor,
                Phone = model.Phone,
                EmailConfirmed = false
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            if (user.IsDoctor)
            {
                Doctor newDoctor = new Doctor()
                {
                    UserId = user.Id,
                    Email = model.Email,
                    Name = model.FullName,
                    IsActive = false,
                    RegistrationCompleted = false,
                    IsProfileCompleted = false,
                    IsVerified = false
                };
                _db.Doctors.Add(newDoctor);
                await _db.SaveChangesAsync();
            }
            // Generate email confirmation token
            var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(emailToken);
            var confirmationLink = Url.Action(
                "ConfirmEmail", "Auth",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme);

            // Send confirmation email
            await _emailService.SendEmailConfirmationAsync(model.Email, confirmationLink!);

            return Ok(new RegisterResponse 
            { 
                Message = "Registration successful! Please check your email to confirm your account before logging in.",
                UserId = user.Id, 
                IsActive = user.IsDoctor ? false : true, 
                RegistrationCompleted = user.IsDoctor ? false : true,
                IsDoctor = user.IsDoctor 
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(MedicareApi.ViewModels.LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) return new NotFoundObjectResult("User does not exist");

                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!passwordValid) return new UnauthorizedObjectResult("Invalid password");

                // Check if email is confirmed
                if (!await _userManager.IsEmailConfirmedAsync(user))
                {
                    return BadRequest(new { message = "Email not confirmed. Please check your email for confirmation link." });
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("uid", user.Id),
                    new Claim("isDoctor", user.IsDoctor.ToString())
                };
                
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] 
                    ?? throw new InvalidOperationException("JWT key not configured")));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var minutesStr = _configuration["Jwt:AccessTokenExpirationMinutes"];
                var minutes = int.TryParse(minutesStr, out var m) ? m : 10;
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"] ?? "medicare.app",
                    audience: null,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(minutes),
                    signingCredentials: creds
                );
                Boolean registrationCompleted = false;
                Boolean isActive = false;
                Boolean isProfileCompleted = false;
                Boolean isVerified = false;
                string status = "active";

                if(user.IsDoctor)
                {
                    Doctor doctor = _db.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                    if (doctor == null)
                    {
                        return NotFound("User not found!");
                    }
                    registrationCompleted = doctor.RegistrationCompleted;
                    isActive = doctor.IsActive;
                    isProfileCompleted = doctor.IsProfileCompleted;
                    isVerified = doctor.IsVerified;
                    
                    // Determine status for doctors
                    if (!doctor.RegistrationCompleted)
                    {
                        status = "pending_registration";
                    }
                    else if (!doctor.IsProfileCompleted)
                    {
                        status = "pending_profile_completion";
                    }
                    else if (!doctor.IsVerified)
                    {
                        status = "waiting_for_approval";
                    }
                    else if (doctor.IsVerified && doctor.IsActive)
                    {
                        status = "active";
                    }
                    else
                    {
                        status = "inactive";
                    }
                }
                else
                {
                    registrationCompleted = true;
                    isActive = true;
                    isProfileCompleted = true;
                    isVerified = true;
                    status = "active";
                }

                return Ok(new LoginResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    UserId = user.Id,
                    IsDoctor = user.IsDoctor,
                    IsActive = user.IsDoctor ? isActive : true,
                    RegistrationCompleted = user.IsDoctor ? registrationCompleted : true,
                    IsProfileCompleted = user.IsDoctor ? isProfileCompleted : true,
                    IsVerified = user.IsDoctor ? isVerified : true,
                    Status = status
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }
        }

        /// <summary>
        /// Confirms user email address.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(EmailConfirmationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            var result = await _userManager.ConfirmEmailAsync(user, request.Token);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }

            // Return HTML page for successful confirmation
            var htmlContent = await GetEmailConfirmationHtmlAsync();
            return Content(htmlContent, "text/html; charset=utf-8");
        }

        /// <summary>
        /// Confirms user email address via GET (for email links).
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Invalid confirmation link" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid user ID" });
            }

            var decodedToken = HttpUtility.UrlDecode(token);
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Invalid or expired token" });
            }

            // Return HTML page for successful confirmation
            var htmlContent = await GetEmailConfirmationHtmlAsync();
            return Content(htmlContent, "text/html; charset=utf-8");
        }

        /// <summary>
        /// Resends email confirmation.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation(ResendConfirmationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist for security
                return Ok(new { message = "If a matching account was found, a confirmation email has been sent." });
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new { message = "Email is already confirmed." });
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var confirmationLink = Url.Action(
                "ConfirmEmail", "Auth",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme);

            await _emailService.SendEmailConfirmationAsync(request.Email, confirmationLink!);

            return Ok(new { message = "If a matching account was found, a confirmation email has been sent." });
        }

        /// <summary>
        /// Initiates password reset process.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user doesn't exist or is not confirmed for security
                return Ok(new { message = "If a matching account was found, a password reset email has been sent." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = HttpUtility.UrlEncode(token);
            var resetLink = $"http://localhost:8081/reset-password?email={HttpUtility.UrlEncode(user.Email)}&token={encodedToken}";

            await _emailService.SendPasswordResetAsync(request.Email, resetLink);

            return Ok(new { message = "If a matching account was found, a password reset email has been sent." });
        }

        /// <summary>
        /// Resets user password using reset token.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid reset request" });
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { 
                    message = "Failed to reset password", 
                    errors = result.Errors.Select(e => e.Description) 
                });
            }

            return Ok(new { message = "Password reset successfully." });
        }

        /// <summary>
        /// Changes user password (requires authentication).
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new { 
                    message = "Failed to change password", 
                    errors = result.Errors.Select(e => e.Description) 
                });
            }

            return Ok(new { message = "Password changed successfully." });
        }

        /// <summary>
        /// Gets the email confirmation success HTML page content.
        /// </summary>
        /// <returns>HTML content as string</returns>
        private async Task<string> GetEmailConfirmationHtmlAsync()
        {
            try
            {
                var htmlPath = Path.Combine(_webHostEnvironment.WebRootPath, "email-confirmed.html");
                if (System.IO.File.Exists(htmlPath))
                {
                    return await System.IO.File.ReadAllTextAsync(htmlPath);
                }
            }
            catch (Exception)
            {
                // If reading file fails, return a simple HTML fallback
            }

            // Fallback HTML in case file is not found
            return @"
<!DOCTYPE html>
<html lang=""ar"" dir=""rtl"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>تأكيد البريد الإلكتروني</title>
    <style>
        body { font-family: Arial, sans-serif; text-align: center; padding: 50px; direction: rtl; }
        .container { max-width: 600px; margin: 0 auto; background: #f9f9f9; padding: 40px; border-radius: 10px; }
        .success { color: #4CAF50; font-size: 24px; margin-bottom: 20px; }
        .btn { background: #4CAF50; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 20px; }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""success"">✓ تم تأكيد البريد الإلكتروني بنجاح!</div>
        <p>مرحباً بك في Medicare API. لقد تم تأكيد بريدك الإلكتروني بنجاح.</p>
        <a href=""http://localhost:8080/"" class=""btn"">العودة إلى الصفحة الرئيسية</a>
    </div>
</body>
</html>";
        }
    }
}