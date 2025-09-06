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

            // Apply filters
            if (!string.IsNullOrEmpty(specialization))
                query = query.Where(d => d.Specialization == specialization);
            if (!string.IsNullOrEmpty(location))
                query = query.Where(d => d.Location == location);
            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => d.Name.Contains(search) || d.Specialization.Contains(search));

            // Apply language filtering - doctor must speak ALL selected languages
            if (languages != null && languages.Any())
            {
                foreach (var language in languages)
                {
                    if (!string.IsNullOrEmpty(language))
                    {
                        query = query.Where(d => d.Languages != null && d.Languages.Contains(language));
                    }
                }
            }

            // Get total count before sorting to avoid EF translation issues with ParseExperience
            var totalCount = await query.CountAsync();

            // Fetch filtered data without sorting to avoid EF translation issues with ParseExperience
            var allFilteredDoctors = await query.ToListAsync();

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
                Data = doctors,
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
    }
}