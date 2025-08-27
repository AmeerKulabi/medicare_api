using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Numerics;
using System.Security.Claims;
using System.Text;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _db;

        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IConfiguration configuration,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(MedicareApi.ViewModels.RegisterRequest model)
        {
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
            // [TODO] Change secret to secure one and store it somewhere secure.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DEMO_SUPER_SECRET_KEY"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "medicare.app",
            audience: null,
            claims: claims,
            expires: DateTime.Now.AddMinutes(10),
            signingCredentials: creds
            );

            // Additional doctor profile creation logic can go here
            return Ok(new RegisterResponse { UserId = user.Id, IsActive = false, RegistrationCompleted = false,
                Token = new JwtSecurityTokenHandler().WriteToken(token), IsDoctor = user.IsDoctor });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(MedicareApi.ViewModels.LoginRequest model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null) return Unauthorized();

                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
                if (!result.Succeeded) return Unauthorized();

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim("uid", user.Id),
                    new Claim("isDoctor", user.IsDoctor.ToString())
                };
                // [TODO] Change secret to secure one and store it somewhere secure.
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "DEMO_SUPER_SECRET_KEY"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"] ?? "medicare.app",
                    audience: null,
                    claims: claims,
                    expires: DateTime.Now.AddDays(7),
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
                    IsActive = isActive,
                    RegistrationCompleted = registrationCompleted

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