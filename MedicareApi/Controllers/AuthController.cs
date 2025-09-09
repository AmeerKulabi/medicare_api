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
                return BadRequest(ModelState);
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
            return Ok(new RegisterResponse { UserId = user.Id, IsActive = user.IsDoctor ? false : true, RegistrationCompleted = user.IsDoctor ? false : true,
                Token = new JwtSecurityTokenHandler().WriteToken(token), IsDoctor = user.IsDoctor });
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
                        return NotFound("User not found!");
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
                    IsActive = user.IsDoctor ? isActive : true,
                    RegistrationCompleted = user.IsDoctor ? registrationCompleted : true

                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest();
            }
        }
    }
}