using MedicareApi.Data;
using MedicareApi.Models;
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
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound();

            return Ok(doctor);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] Doctor update)
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound();

            doctor.Name = update.Name;
            doctor.Specialization = update.Specialization;
            doctor.Location = update.Location;
            // update more fields as needed
            await _db.SaveChangesAsync();
            return Ok(doctor);
        }
    }
}