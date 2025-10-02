using Humanizer;
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
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

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
            catch
            {
                return BadRequest(ApiErrors.AvailabilitySlotsCouldNotBeRetrieved);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorAvailability([FromRoute] string id)
        {
            try
            {
                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == id);
                if (doctor == null) return NotFound(ApiErrors.UserDoesNotExist);

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
            catch
            {
                return BadRequest(ApiErrors.AvailabilitySlotsCouldNotBeRetrieved);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAvailabilitySlot([FromRoute] string id, [FromBody] AvailabilitySlot update)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                var slot = await _db.AvailabilitySlots.FirstOrDefaultAsync(s => s.Id == id && s.DoctorId == doctor.Id);
                if (slot == null) return NotFound(ApiErrors.AvailabilitySlotNotFound);

                // Convert to TimeSpan
                if (TimeSpan.Parse(update.Start) > TimeSpan.Parse(update.End))
                    return BadRequest(ApiErrors.EndTimeBeforeStartTime);

                slot.Start = update.Start;
                slot.End = update.End;
                slot.IsAvailable = update.IsAvailable;
                await _db.SaveChangesAsync();
                return Ok(slot);
            }
            catch
            {
                return BadRequest(ApiErrors.AvailabilitySlotsCouldNotBeRetrieved);
            }
        }
    }
}