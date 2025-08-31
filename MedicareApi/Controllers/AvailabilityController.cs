using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/availability")]
    [Authorize]
    public class AvailabilityController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<AvailabilityController> _logger;

        public AvailabilityController(ApplicationDbContext db, ILogger<AvailabilityController> logger)
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
                _logger.LogWarning("Unauthorized access attempt: User {UserId} is not a doctor accessing availability", userId);
                return Unauthorized(new { error = "Unauthorized: doctor access required." });
            }

            return Ok(); // Will be ignored, just used for method signature
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailability()
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

            var slots = await _db.AvailabilitySlots.Where(s => s.DoctorId == doctor.Id).ToListAsync();
            string[] days = ["الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"];
            if (slots.Count == 0)
            {
                foreach (string day in days)
                {
                    AvailabilitySlot availabilitySlot = new AvailabilitySlot()
                    {
                        day = day,
                        DoctorId = doctor.Id,
                        End = "16.00",
                        Start = "8.00",
                        IsAvailable = false

                    };
                    slots.Add(availabilitySlot);
                    _db.AvailabilitySlots.Add(availabilitySlot);
                    _db.SaveChanges();
                }
            }
            return Ok(slots);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorAvailability([FromRoute] string id)
        {

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();

            var slots = await _db.AvailabilitySlots.Where(s => s.DoctorId == doctor.Id).ToListAsync();
            string[] days = ["الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت"];
            if (slots.Count == 0)
            {
                foreach (string day in days)
                {
                    AvailabilitySlot availabilitySlot = new AvailabilitySlot()
                    {
                        day = day,
                        DoctorId = doctor.Id,
                        End = "16.00",
                        Start = "8.00",
                        IsAvailable = false

                    };
                    slots.Add(availabilitySlot);
                    _db.AvailabilitySlots.Add(availabilitySlot);
                    _db.SaveChanges();
                }
            }
            return Ok(slots);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAvailabilitySlot([FromRoute] string id, [FromBody] AvailabilitySlot update)
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

            var slot = await _db.AvailabilitySlots.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctor.Id);
            if (slot == null) return NotFound();

            slot.Start = update.Start;
            slot.End = update.End;
            slot.IsAvailable = update.IsAvailable;
            await _db.SaveChangesAsync();
            return Ok(slot);
        }
    }
}