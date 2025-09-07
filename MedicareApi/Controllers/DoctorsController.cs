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

            // Get total count and data
            var totalCount = await query.CountAsync();
            
            // Apply pagination - simplified sorting since YearsOfExperience is removed
            var doctors = await query
                .OrderBy(d => d.Name) // Simple name-based ordering
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

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