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

        public DoctorsController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors(
            [FromQuery] string? specialization, 
            [FromQuery] string? location, 
            [FromQuery] string? sortBy, 
            [FromQuery] string? search,
            [FromQuery] List<string>? languages,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 20) pageSize = 20;

            var query = _db.Doctors.AsQueryable();

            // Apply filters (excluding language filter for now due to EF Core limitation)
            if (!string.IsNullOrEmpty(specialization))
                query = query.Where(d => d.Specialization == specialization);
            if (!string.IsNullOrEmpty(location))
                query = query.Where(d => d.Location == location);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search) || d.Specialization.Contains(search));

            // Fetch filtered data without language filter and without sorting to avoid EF translation issues
            var allFilteredDoctors = await query.ToListAsync();

            // Apply language filter in memory due to EF Core in-memory provider limitations
            if (languages != null && languages.Count > 0)
            {
                allFilteredDoctors = allFilteredDoctors
                    .Where(d => d.Languages != null && languages.All(lang => d.Languages.Contains(lang)))
                    .ToList();
            }

            // Get total count after all filters are applied
            var totalCount = allFilteredDoctors.Count;

            // Apply sorting in memory - only by experience (remove rating/distance sorting)
            IEnumerable<Doctor> sortedDoctors;
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "experience":
                    case "experience_desc":
                        sortedDoctors = allFilteredDoctors.OrderByDescending(d => ParseExperience(d.YearsOfExperience));
                        break;
                    case "experience_asc":
                        sortedDoctors = allFilteredDoctors.OrderBy(d => ParseExperience(d.YearsOfExperience));
                        break;
                    default:
                        // Default sort by experience descending
                        sortedDoctors = allFilteredDoctors.OrderByDescending(d => ParseExperience(d.YearsOfExperience));
                        break;
                }
            }
            else
            {
                // Default sort by experience descending
                sortedDoctors = allFilteredDoctors.OrderByDescending(d => ParseExperience(d.YearsOfExperience));
            }

            // Apply pagination to sorted data
            var doctors = sortedDoctors
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Ensure all doctors have profile picture URLs (default if none set)
            foreach (var doctor in doctors)
            {
                doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);
            }

            // Create paginated response
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            var response = new PaginatedResponse<Doctor>
            {
                Doctors = doctors,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page,
                TotalPages = totalPages,
                HasPreviousPage = page > 1,
                HasNextPage = page < totalPages
            };

            return Ok(response);
        }

        private static int ParseExperience(string? experienceString)
        {
            if (string.IsNullOrEmpty(experienceString))
                return 0;

            // Try to extract numeric value from experience string
            var digits = string.Concat(experienceString.Where(char.IsDigit));
            return int.TryParse(digits, out var experience) ? experience : 0;
        }

        [HttpGet("languages")]
        public async Task<IActionResult> GetLanguages()
        {
            var doctors = await _db.Doctors
                .Where(d => d.Languages != null && d.Languages.Any())
                .ToListAsync();

            var allLanguages = doctors
                .SelectMany(d => d.Languages!)
                .Distinct()
                .OrderBy(l => l)
                .ToList();

            return Ok(allLanguages);
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

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateDoctorInfo([FromForm] DoctorRegistrationInfo formData, [FromForm] IFormFile? profilePicture)
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound();

            // Update doctor information from form data
            doctor.Phone = formData.Phone;
            doctor.DateOfBirth = formData.DateOfBirth;
            doctor.Gender = formData.Gender;
            doctor.MedicalLicense = formData.MedicalLicense;
            doctor.LicenseState = formData.LicenseState;
            doctor.LicenseExpiry = formData.LicenseExpiry;
            doctor.Specialization = formData.Specialization;
            doctor.SubSpecialization = formData.SubSpecialization;
            doctor.BoardCertification = formData.BoardCertification;
            doctor.YearsOfExperience = formData.YearsOfExperience;
            doctor.ProfessionalBiography = formData.ProfessionalBiography;
            doctor.MedicalSchool = formData.MedicalSchool;
            doctor.GraduationYear = formData.GraduationYear;
            doctor.ResidencyProgram = formData.ResidencyProgram;
            doctor.ResidencyHospital = formData.ResidencyHospital;
            doctor.FellowshipProgram = formData.FellowshipProgram;
            doctor.ClinicName = formData.ClinicName;
            doctor.ClinicAddress = formData.ClinicAddress;
            doctor.ClinicCity = formData.ClinicCity;
            doctor.ClinicState = formData.ClinicState;
            doctor.ClinicZip = formData.ClinicZip;
            // Set Location as a combination of clinic city and state for search purposes
            if (!string.IsNullOrEmpty(formData.ClinicCity) && !string.IsNullOrEmpty(formData.ClinicState))
            {
                doctor.Location = $"{formData.ClinicCity}, {formData.ClinicState}";
            }
            else if (!string.IsNullOrEmpty(formData.ClinicCity))
            {
                doctor.Location = formData.ClinicCity;
            }
            else if (!string.IsNullOrEmpty(formData.ClinicState))
            {
                doctor.Location = formData.ClinicState;
            }
            doctor.ClinicPhone = formData.ClinicPhone;
            doctor.PracticeType = formData.PracticeType;
            doctor.HospitalAffiliations = formData.HospitalAffiliations;
            doctor.ServicesOffered = formData.ServicesOffered;
            doctor.ConsultationFee = formData.ConsultationFee;
            doctor.Availability = formData.Availability;
            doctor.Languages = formData.Languages;
            doctor.TermsAccepted = formData.TermsAccepted;
            doctor.PrivacyAccepted = formData.PrivacyAccepted;

            // Mark registration as completed
            doctor.RegistrationCompleted = true;

            // Handle profile picture upload if provided
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var result = await ProfilePictureHelper.SaveProfilePicture(profilePicture, doctor.Id);
                if (result.success)
                {
                    // Delete old profile picture if exists
                    ProfilePictureHelper.DeleteProfilePicture(doctor.ProfilePictureUrl);
                    doctor.ProfilePictureUrl = result.url;
                }
            }

            await _db.SaveChangesAsync();

            // Ensure profile picture URL includes default if none set
            doctor.ProfilePictureUrl = ProfilePictureHelper.GetProfilePictureUrl(doctor.ProfilePictureUrl);

            return Ok(doctor);
        }
    }
}