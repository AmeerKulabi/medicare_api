using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/doctors")]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DoctorsController> _logger;

        public DoctorsController(ApplicationDbContext db, ILogger<DoctorsController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors([FromQuery] string? specialization, [FromQuery] string? location, [FromQuery] string? sortBy, [FromQuery] string? search)
        {
            var query = _db.Doctors.AsQueryable();
            if (!string.IsNullOrEmpty(specialization))
                query = query.Where(d => d.Specialization == specialization);
            if (!string.IsNullOrEmpty(location))
                query = query.Where(d => d.Location == location);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search) || d.Specialization.Contains(search));
            // sortBy not implemented, but you can add as needed

            var doctors = await query.ToListAsync();
            
            // Ensure all doctors have profile picture URLs (default if none set)
            foreach (var doctor in doctors)
            {
                doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);
            }
            
            return Ok(doctors);
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateDoctorInfo([FromForm] DoctorRegistrationInfo formData, IFormFile? profilePicture)
        {
            // Do validation and save logic here
            // Example: Save files to disk or database, insert data, etc.
            string userId = User.FindFirstValue("uid");
            string isDoctor = User.FindFirstValue("isDoctor");
            
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt: Missing user ID in JWT token");
                return Unauthorized(new { error = "Unauthorized: Valid user token required." });
            }
            
            if (string.IsNullOrEmpty(isDoctor) || isDoctor == "False")
            {
                _logger.LogWarning("Unauthorized access attempt: User {UserId} is not a doctor accessing doctor info update", userId);
                return Unauthorized(new { error = "Unauthorized: doctor access required." });
            }
            
            Doctor doctor = _db.Doctors.FirstOrDefault(d => d.UserId == userId);
            if (doctor == null)
            {
                _logger.LogWarning("Unauthorized access attempt: Doctor record not found for user {UserId}", userId);
                return Unauthorized(new { error = "Unauthorized: doctor profile not found." });
            }

            // Handle profile picture upload if provided
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var profilePictureResult = await ProfilePictureHelper.SaveProfilePicture(profilePicture, doctor.Id);
                if (profilePictureResult.success)
                {
                    ProfilePictureHelper.DeleteProfilePicture(doctor.ProfilePictureUrl);
                    doctor.ProfilePictureUrl = profilePictureResult.url;
                }
                else
                {
                    return BadRequest(profilePictureResult.error);
                }
            }

            doctor.Specialization = formData.Specialization;
            doctor.Phone = formData.Phone;
            doctor.DateOfBirth = formData.DateOfBirth;
            doctor.Gender = formData.Gender;
            doctor.MedicalLicense = formData.MedicalLicense;
            doctor.LicenseState = formData.LicenseState;
            doctor.LicenseExpiry = formData.LicenseExpiry;
            doctor.Specialization = formData.Specialization;
            if(formData.SubSpecialization != null)
                doctor.SubSpecialization = formData.SubSpecialization;
            doctor.BoardCertification = formData.BoardCertification;
            doctor.YearsOfExperience= formData.YearsOfExperience;
            doctor.MedicalSchool = formData.MedicalSchool;
            doctor.GraduationYear = formData.GraduationYear;
            doctor.ResidencyProgram = formData.ResidencyProgram;
            doctor.ResidencyHospital = formData.ResidencyHospital;
            if (formData.FellowshipProgram != null)
                doctor.FellowshipProgram = formData.FellowshipProgram;
            doctor.ClinicName = formData.ClinicName;
            doctor.ClinicAddress = formData.ClinicAddress;
            doctor.ClinicCity = formData.ClinicCity;
            doctor.ClinicState = formData.ClinicState;
            doctor.ClinicZip = formData.ClinicZip;
            doctor.ClinicPhone = formData.ClinicPhone;
            doctor.PracticeType = formData.PracticeType;
            if (formData.HospitalAffiliations != null)
                doctor.HospitalAffiliations = formData.HospitalAffiliations;
            if (formData.ServicesOffered != null)
                doctor.ServicesOffered = formData.ServicesOffered;
            if (formData.ConsultationFee != null)
                doctor.ConsultationFee = formData.ConsultationFee;
            if (formData.Availability != null)
                doctor.Availability = formData.Availability;
            if (formData.Languages != null)
                doctor.Languages = formData.Languages;
            if (formData.TermsAccepted != null)
                doctor.TermsAccepted = formData.TermsAccepted;
            if (formData.PrivacyAccepted != null)
                doctor.PrivacyAccepted = formData.PrivacyAccepted;

            doctor.RegistrationCompleted = true;
            _db.Doctors.Update(doctor);
            await _db.SaveChangesAsync();

            return Ok(doctor);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorById([FromRoute] string id)
        {
            var doctor = await _db.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();
            
            // Ensure profile picture URL includes default if none set
            doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);
            
            return Ok(doctor);
        }
    }
}