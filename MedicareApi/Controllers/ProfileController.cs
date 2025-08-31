using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Utils;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ApplicationDbContext db, ILogger<ProfileController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private IActionResult CheckDoctorAuthorization(out string userId)
        {
            userId = User.FindFirst("uid")?.Value ?? "";

            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt: Missing user ID in JWT token");
                return Unauthorized(new { error = "Unauthorized: Valid user token required." });
            }

            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor)
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} is not a doctor accessing profile", userId);
                return Unauthorized(new { error = "Unauthorized: doctor access required." });
            }

            return Ok(); // Will be ignored, just used for method signature
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var authResult = CheckDoctorAuthorization(out string userId);
            if (authResult is UnauthorizedObjectResult)
                return authResult;

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                _logger.LogWarning("Unauthorized access attempt: Doctor record not found for user {UserId}", userId);
                return Unauthorized(new { error = "Unauthorized: doctor profile not found." });
            }

            // Ensure profile picture URL includes default if none set
            doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);

            return Ok(doctor);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto updateDto)
        {
            var authResult = CheckDoctorAuthorization(out string userId);
            if (authResult is UnauthorizedObjectResult)
                return authResult;

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                _logger.LogWarning("Unauthorized access attempt: Doctor record not found for user {UserId}", userId);
                return Unauthorized(new { error = "Unauthorized: doctor profile not found." });
            }

            // Only update allowed fields - restrict to specific properties only
            if (updateDto.Email != null) doctor.Email = updateDto.Email;
            if (updateDto.Phone != null) doctor.Phone = updateDto.Phone;
            if (updateDto.ProfessionalBiography != null) doctor.ProfessionalBiography = updateDto.ProfessionalBiography;
            if (updateDto.Languages != null) doctor.Languages = updateDto.Languages;
            
            // Education & Training fields
            if (updateDto.MedicalSchool != null) doctor.MedicalSchool = updateDto.MedicalSchool;
            if (updateDto.GraduationYear != null) doctor.GraduationYear = updateDto.GraduationYear;
            if (updateDto.ResidencyProgram != null) doctor.ResidencyProgram = updateDto.ResidencyProgram;
            if (updateDto.ResidencyHospital != null) doctor.ResidencyHospital = updateDto.ResidencyHospital;
            if (updateDto.FellowshipProgram != null) doctor.FellowshipProgram = updateDto.FellowshipProgram;
            
            // Hospital and clinic information
            if (updateDto.HospitalAffiliations != null) doctor.HospitalAffiliations = updateDto.HospitalAffiliations;
            if (updateDto.ClinicName != null) doctor.ClinicName = updateDto.ClinicName;
            if (updateDto.ClinicAddress != null) doctor.ClinicAddress = updateDto.ClinicAddress;
            if (updateDto.ClinicPhone != null) doctor.ClinicPhone = updateDto.ClinicPhone;
            if (updateDto.ConsultationFee != null) doctor.ConsultationFee = updateDto.ConsultationFee;
            if(!doctor.RegistrationCompleted) doctor.RegistrationCompleted = true;

            #if DEBUG
            if(!doctor.IsActive) doctor.IsActive = true;
            #endif

            await _db.SaveChangesAsync();
            
            // Ensure profile picture URL includes default if none set
            doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);
            
            return Ok(doctor);
        }

        [HttpPost("upload-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            var authResult = CheckDoctorAuthorization(out string userId);
            if (authResult is UnauthorizedObjectResult)
                return authResult;

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                _logger.LogWarning("Unauthorized access attempt: Doctor record not found for user {UserId}", userId);
                return Unauthorized(new { error = "Unauthorized: doctor profile not found." });
            }

            if (profilePicture == null || profilePicture.Length == 0)
                return BadRequest("No file uploaded");

            var result = await ProfilePictureHelper.SaveProfilePicture(profilePicture, doctor.Id);
            if (!result.success)
                return BadRequest(result.error);

            // Delete old profile picture if exists
            ProfilePictureHelper.DeleteProfilePicture(doctor.ProfilePictureUrl);

            // Update doctor profile with new image URL
            doctor.ProfilePictureUrl = result.url;
            await _db.SaveChangesAsync();

            UploadPictureResponse response = new UploadPictureResponse();
            response.profilePictureUrl = doctor.ProfilePictureUrl;

            return Ok(response);
        }
    }
}