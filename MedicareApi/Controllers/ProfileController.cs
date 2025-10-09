using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Utils;
using MedicareApi.ViewModels;
using MedicareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAnalyticsService _analyticsService;

        public ProfileController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IAnalyticsService analyticsService)
        {
            _db = db;
            _userManager = userManager;
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (userId == null)
                {
                    return Unauthorized(ApiErrors.UserDoesNotExist);
                }
                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return NotFound(ApiErrors.UserDoesNotExist);
                var user = await _userManager.FindByIdAsync(doctor.UserId);

                // Ensure profile picture URL includes default if none set
                doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);

                return Ok(DoctorHelper.FromDoctorToDoctorProfileDto(doctor, user));
            }
            catch
            {
                return BadRequest(ApiErrors.ProfileRetrievingFailed);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto updateDto)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (userId == null)
                {
                    return Unauthorized(ApiErrors.UserDoesNotExist);
                }
                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return NotFound(ApiErrors.UserDoesNotExist);

                // Map fields from DTO to Doctor entity
                if (updateDto.DateOfBirth.HasValue) doctor.DateOfBirth = updateDto.DateOfBirth;
                if (updateDto.Gender != null) doctor.Gender = updateDto.Gender;
                if (updateDto.MedicalLicense != null) doctor.MedicalLicense = updateDto.MedicalLicense;
                if (updateDto.LicenseExpiry.HasValue) doctor.LicenseExpiry = updateDto.LicenseExpiry;

                if (updateDto.Specialization != null) doctor.Specialization = updateDto.Specialization;
                if (updateDto.SubSpecialization != null) doctor.SubSpecialization = updateDto.SubSpecialization;
                if (updateDto.YearsOfExperience != null) doctor.YearsOfExperience = updateDto.YearsOfExperience;

                if (updateDto.MedicalSchool != null) doctor.MedicalSchool = updateDto.MedicalSchool;
                if (updateDto.GraduationYear != null) doctor.GraduationYear = updateDto.GraduationYear;
                if (updateDto.ProfessionalBiography != null) doctor.ProfessionalBiography = updateDto.ProfessionalBiography;

                if (updateDto.ClinicName != null) doctor.ClinicName = updateDto.ClinicName;
                if (updateDto.ClinicType != null) doctor.ClinicType = updateDto.ClinicType;
                if (updateDto.ClinicAddress != null) doctor.ClinicAddress = updateDto.ClinicAddress;

                if (updateDto.ConsultationFee != null) doctor.ConsultationFee = updateDto.ConsultationFee;
                if (updateDto.Languages != null) doctor.Languages = updateDto.Languages;

                if (updateDto.TermsAccepted.HasValue) doctor.TermsAccepted = updateDto.TermsAccepted.Value;
                if (updateDto.PrivavyAccepted.HasValue) doctor.PrivacyAccepted = updateDto.PrivavyAccepted;
                if (updateDto.City != null) doctor.City = updateDto.City;

                if (updateDto.Phone != null)
                {
                    var user = await _userManager.FindByIdAsync(doctor.UserId);
                    user.Phone = updateDto.Phone;
                    await _userManager.UpdateAsync(user);
                }

                if (!doctor.RegistrationCompleted) doctor.RegistrationCompleted = true;

#if DEBUG
                if (!doctor.IsActive) doctor.IsActive = true;
#endif

                await _db.SaveChangesAsync();

                doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);

                // Track doctor profile update
                _analyticsService.TrackDoctorProfileUpdated(doctor.Id);

                return Ok(doctor);
            }
            catch
            {
                return BadRequest(ApiErrors.ProfileUpdateFailed);
            }
        }



        [HttpPost("upload-picture")]
        public async Task<IActionResult> UploadProfilePicture(IFormFile profilePicture)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (userId == null)
                {
                    return Unauthorized(ApiErrors.UserDoesNotExist);
                }
                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return NotFound(ApiErrors.UserDoesNotExist);

                if (profilePicture == null || profilePicture.Length == 0)
                    return BadRequest(ApiErrors.FileNotFound);

                var result = await ProfilePictureHelper.SaveProfilePicture(profilePicture, doctor.Id);
                if (!result.success)
                    return BadRequest(ApiErrors.PictureUploadFailed);

                // Delete old profile picture if exists
                ProfilePictureHelper.DeleteProfilePicture(doctor.ProfilePictureUrl);

                // Update doctor profile with new image URL
                doctor.ProfilePictureUrl = result.url;
                await _db.SaveChangesAsync();

                // Track doctor profile picture update
                _analyticsService.TrackDoctorProfilePictureUpdated(doctor.Id);

                UploadPictureResponse response = new UploadPictureResponse();
                response.profilePictureUrl = doctor.ProfilePictureUrl;

                return Ok(response);
            }
            catch
            {
                return BadRequest(ApiErrors.PictureUploadFailed);
            }
        }
    }
}