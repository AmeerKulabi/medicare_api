using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/blocked-time-slots")]
    [Authorize]
    public class BlockedTimeController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public BlockedTimeController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all blocked time slots for the authenticated doctor
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetBlockedTimeSlots()
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized();

            var blockedSlots = await _db.BlockedTimeSlots
                .Where(b => b.DoctorId == doctor.Id)
                .OrderBy(b => b.StartTime)
                .ToListAsync();

            var response = blockedSlots.Select(b => new BlockedTimeSlotResponse
            {
                Id = b.Id,
                DoctorId = b.DoctorId,
                StartTime = b.StartTime,
                EndTime = b.EndTime,
                IsWholeDay = b.IsWholeDay,
                Reason = b.Reason,
                CreatedAt = b.CreatedAt,
                IsRecurring = b.IsRecurring,
                RecurrencePattern = b.RecurrencePattern
            }).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Get blocked time slots for a specific doctor by doctor ID (for patients/frontend to check availability)
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorBlockedTimeSlots([FromRoute] string doctorId)
        {
            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
            if (doctor == null) return NotFound("Doctor not found");

            var blockedSlots = await _db.BlockedTimeSlots
                .Where(b => b.DoctorId == doctorId)
                .OrderBy(b => b.StartTime)
                .Select(b => new BlockedTimeSlotResponse
                {
                    Id = b.Id,
                    DoctorId = b.DoctorId,
                    StartTime = b.StartTime,
                    EndTime = b.EndTime,
                    IsWholeDay = b.IsWholeDay,
                    Reason = null, // Don't expose reason to patients for privacy
                    CreatedAt = b.CreatedAt,
                    IsRecurring = b.IsRecurring,
                    RecurrencePattern = b.RecurrencePattern
                })
                .ToListAsync();

            return Ok(blockedSlots);
        }

        /// <summary>
        /// Create a new blocked time slot
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBlockedTimeSlot([FromBody] CreateBlockedTimeSlotRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized();

            // Validate time range
            if (request.StartTime >= request.EndTime)
            {
                return BadRequest("End time must be after start time");
            }

            // Check for existing appointments in this time range
            var conflictingAppointments = await _db.Appointments
                .Where(a => a.DoctorId == doctor.Id &&
                           a.ScheduledAt >= request.StartTime &&
                           a.ScheduledAt < request.EndTime)
                .ToListAsync();

            if (conflictingAppointments.Any())
            {
                return BadRequest($"Cannot block time range - there are {conflictingAppointments.Count} existing appointments in this period");
            }

            // Check for overlapping blocked time slots
            var overlappingBlocks = await _db.BlockedTimeSlots
                .Where(b => b.DoctorId == doctor.Id &&
                           ((b.StartTime <= request.StartTime && b.EndTime > request.StartTime) ||
                            (b.StartTime < request.EndTime && b.EndTime >= request.EndTime) ||
                            (b.StartTime >= request.StartTime && b.EndTime <= request.EndTime)))
                .ToListAsync();

            if (overlappingBlocks.Any())
            {
                return BadRequest("This time range overlaps with existing blocked time slots");
            }

            var blockedTimeSlot = new BlockedTimeSlot
            {
                DoctorId = doctor.Id,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                IsWholeDay = request.IsWholeDay,
                Reason = request.Reason,
                IsRecurring = request.IsRecurring,
                RecurrencePattern = request.RecurrencePattern
            };

            _db.BlockedTimeSlots.Add(blockedTimeSlot);
            await _db.SaveChangesAsync();

            var response = new BlockedTimeSlotResponse
            {
                Id = blockedTimeSlot.Id,
                DoctorId = blockedTimeSlot.DoctorId,
                StartTime = blockedTimeSlot.StartTime,
                EndTime = blockedTimeSlot.EndTime,
                IsWholeDay = blockedTimeSlot.IsWholeDay,
                Reason = blockedTimeSlot.Reason,
                CreatedAt = blockedTimeSlot.CreatedAt,
                IsRecurring = blockedTimeSlot.IsRecurring,
                RecurrencePattern = blockedTimeSlot.RecurrencePattern
            };

            return Ok(response);
        }

        /// <summary>
        /// Update a blocked time slot
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlockedTimeSlot([FromRoute] string id, [FromBody] UpdateBlockedTimeSlotRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized();

            var blockedSlot = await _db.BlockedTimeSlots.FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctor.Id);
            if (blockedSlot == null) return NotFound();

            // Update fields if provided
            if (request.StartTime.HasValue)
                blockedSlot.StartTime = request.StartTime.Value;
            
            if (request.EndTime.HasValue)
                blockedSlot.EndTime = request.EndTime.Value;

            if (request.IsWholeDay.HasValue)
                blockedSlot.IsWholeDay = request.IsWholeDay.Value;

            if (request.Reason != null)
                blockedSlot.Reason = request.Reason;

            if (request.IsRecurring.HasValue)
                blockedSlot.IsRecurring = request.IsRecurring.Value;

            if (request.RecurrencePattern != null)
                blockedSlot.RecurrencePattern = request.RecurrencePattern;

            // Validate updated time range
            if (blockedSlot.StartTime >= blockedSlot.EndTime)
            {
                return BadRequest("End time must be after start time");
            }

            await _db.SaveChangesAsync();

            var response = new BlockedTimeSlotResponse
            {
                Id = blockedSlot.Id,
                DoctorId = blockedSlot.DoctorId,
                StartTime = blockedSlot.StartTime,
                EndTime = blockedSlot.EndTime,
                IsWholeDay = blockedSlot.IsWholeDay,
                Reason = blockedSlot.Reason,
                CreatedAt = blockedSlot.CreatedAt,
                IsRecurring = blockedSlot.IsRecurring,
                RecurrencePattern = blockedSlot.RecurrencePattern
            };

            return Ok(response);
        }

        /// <summary>
        /// Delete a blocked time slot
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlockedTimeSlot([FromRoute] string id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized();

            var blockedSlot = await _db.BlockedTimeSlots.FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctor.Id);
            if (blockedSlot == null) return NotFound();

            _db.BlockedTimeSlots.Remove(blockedSlot);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}