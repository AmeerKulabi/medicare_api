using Humanizer;
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

        public AvailabilityController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailability()
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized();

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
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized();

            var slot = await _db.AvailabilitySlots.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctor.Id);
            if (slot == null) return NotFound();

            // Convert to TimeSpan
            if (TimeSpan.Parse(update.Start) > TimeSpan.Parse(update.End))
                return BadRequest("وقت البداية يجب يكون قبل وقت النهاية");

            slot.Start = update.Start;
            slot.End = update.End;
            slot.IsAvailable = update.IsAvailable;
            await _db.SaveChangesAsync();
            return Ok(slot);
        }
    }
}