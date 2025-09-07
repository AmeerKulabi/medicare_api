using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;

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
        /// Constructor.
        /// </summary>
        /// <param name="userManager">Identity framework, user manager.</param>
        /// <param name="signInManager">Identity framework, sign in manager.</param>
        /// <param name="configuration">Configuration mananger.</param>
        /// <param name="db">Access db.</param>
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _db = db;
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
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.VALIDATION_ERROR,
                    Message = "Invalid request data",
                    Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                return Conflict(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.EMAIL_ALREADY_EXISTS,
                    Message = "An account with this email address already exists",
                    Details = "Please use a different email address or try logging in instead"
                });
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                IsDoctor = model.IsDoctor,
                Phone = model.Phone
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                var errorCode = errors.Any(e => e.Contains("password", StringComparison.OrdinalIgnoreCase)) 
                    ? ErrorCodes.WEAK_PASSWORD 
                    : ErrorCodes.VALIDATION_ERROR;
                
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = errorCode,
                    Message = "Registration failed",
                    Details = string.Join("; ", errors)
                });
            }
            if (user.IsDoctor)
            {
                Doctor newDoctor = new Doctor()
                {
                    UserId = user.Id,
                    Email = model.Email,
                    Name = model.FullName,
                    IsActive = false,
                    RegistrationCompleted = false
                };
                _db.Doctors.Add(newDoctor);
                _db.SaveChanges();
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

            // Additional doctor profile creation logic can go here
            return Ok(new RegisterResponse { UserId = user.Id, IsActive = false, RegistrationCompleted = false,
                Token = new JwtSecurityTokenHandler().WriteToken(token), IsDoctor = user.IsDoctor });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(MedicareApi.ViewModels.LoginRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.VALIDATION_ERROR,
                    Message = "Invalid request data",
                    Details = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = ErrorCodes.USER_NOT_FOUND,
                        Message = "No account found with this email address",
                        Details = "Please check your email address or register for a new account"
                    });
                }

                // Check if account is locked
                if (await _userManager.IsLockedOutAsync(user))
                {
                    return Unauthorized(new ApiErrorResponse
                    {
                        ErrorCode = ErrorCodes.ACCOUNT_LOCKED,
                        Message = "Account is temporarily locked due to too many failed login attempts",
                        Details = $"Please try again after {user.LockoutEnd?.ToString("yyyy-MM-dd HH:mm:ss")} UTC"
                    });
                }

                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!passwordValid)
                {
                    // Record failed login attempt for lockout functionality
                    await _userManager.AccessFailedAsync(user);
                    
                    return Unauthorized(new ApiErrorResponse
                    {
                        ErrorCode = ErrorCodes.INVALID_PASSWORD,
                        Message = "Incorrect password",
                        Details = "Please check your password and try again"
                    });
                }

                // Reset failed login count on successful login
                await _userManager.ResetAccessFailedCountAsync(user);

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
                if(user.IsDoctor)
                {
                    Doctor doctor = _db.Doctors.FirstOrDefault(d => d.UserId == user.Id);
                    if (doctor == null)
                    {
                        return NotFound(new ApiErrorResponse
                        {
                            ErrorCode = ErrorCodes.NOT_FOUND,
                            Message = "Doctor profile not found",
                            Details = "Doctor account exists but profile is missing. Please contact support."
                        });
                    }
                    registrationCompleted = doctor.RegistrationCompleted;
                    isActive = doctor.IsActive;
                }
                else
                {
                    registrationCompleted = true;
                    isActive = false;
                }
                return Ok(new LoginResponse
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    UserId = user.Id,
                    IsDoctor = user.IsDoctor,
                    IsActive = isActive,
                    RegistrationCompleted = registrationCompleted

                });
            }
            catch (Exception e)
            {
                // Log the exception (in production, use proper logging)
                Console.WriteLine($"Login error: {e.Message}");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.INTERNAL_ERROR,
                    Message = "An internal error occurred during login",
                    Details = "Please try again later or contact support if the problem persists"
                });
            }
        }
    }
}