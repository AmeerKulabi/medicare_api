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

        public ProfileController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            
            if (!isDoctor)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS,
                    Message = "Access denied",
                    Details = "Only doctors can access profile information"
                });
            }

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.NOT_FOUND,
                    Message = "Doctor profile not found",
                    Details = "No doctor profile associated with this account"
                });
            }

            // Ensure profile picture URL includes default if none set
            doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);

            return Ok(doctor);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto updateDto)
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            
            if (!isDoctor)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS,
                    Message = "Access denied",
                    Details = "Only doctors can update profile information"
                });
            }

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.NOT_FOUND,
                    Message = "Doctor profile not found",
                    Details = "No doctor profile associated with this account"
                });
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
            if (updateDto.Location != null) doctor.Location = updateDto.Location;
            
            // Hospital and clinic information
            if (updateDto.HospitalAffiliations != null) doctor.HospitalAffiliations = updateDto.HospitalAffiliations;
            if (updateDto.ClinicName != null) doctor.ClinicName = updateDto.ClinicName;
            if (updateDto.ClinicAddress != null) doctor.ClinicAddress = updateDto.ClinicAddress;
            if (updateDto.ClinicPhone != null) doctor.ClinicPhone = updateDto.ClinicPhone;
            if (updateDto.ConsultationFee != null) doctor.ConsultationFee = updateDto.ConsultationFee;
            if (updateDto.YearsOfExperience != null) doctor.YearsOfExperience = updateDto.YearsOfExperience;
            if (updateDto.Specialization != null) doctor.Specialization = updateDto.Specialization;
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
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            
            if (!isDoctor)
            {
                return Unauthorized(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS,
                    Message = "Access denied",
                    Details = "Only doctors can upload profile pictures"
                });
            }

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null)
            {
                return NotFound(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.NOT_FOUND,
                    Message = "Doctor profile not found",
                    Details = "No doctor profile associated with this account"
                });
            }

            if (profilePicture == null || profilePicture.Length == 0)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.INVALID_REQUEST,
                    Message = "No file uploaded",
                    Details = "Please select a file to upload"
                });
            }

            var result = await ProfilePictureHelper.SaveProfilePicture(profilePicture, doctor.Id);
            if (!result.success)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = ErrorCodes.BAD_REQUEST,
                    Message = "Failed to save profile picture",
                    Details = result.error
                });
            }

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