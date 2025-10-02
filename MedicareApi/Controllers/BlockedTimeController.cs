using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Utils;
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
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrors.UserDoesNotExist);

                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                var blockedSlots = await _db.BlockedTimeSlots
                    .Where(b => b.DoctorId == doctor.Id && b.EndTime > DateTime.Now)
                    .OrderBy(b => b.StartTime)
                    .ToListAsync();

                var response = blockedSlots.Select(b => new BlockedTimeSlotResponse
                {
                    id = b.Id,
                    doctorId = b.DoctorId,
                    date = b.StartTime.ToString("yyyy-MM-dd"),
                    startTime = b.IsWholeDay ? null : b.StartTime.ToString("HH:mm"),
                    endTime = b.IsWholeDay ? null : b.EndTime.ToString("HH:mm"),
                    blockWholeDay = b.IsWholeDay,
                    reason = b.Reason
                }).ToList();

                return Ok(response);
            }
            catch
            {
                return BadRequest(ApiErrors.BlockedSlotsCouldNotBeRetrieved);
            }
        }

        /// <summary>
        /// Get blocked time slots for a specific doctor by doctor ID (for patients/frontend to check availability)
        /// </summary>
        [HttpGet("doctor/{doctorId}")]
        public async Task<IActionResult> GetDoctorBlockedTimeSlots([FromRoute] string doctorId)
        {
            try
            {
                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == doctorId);
                if (doctor == null) return NotFound(ApiErrors.UserDoesNotExist);

                var blockedSlots = await _db.BlockedTimeSlots
                    .Where(b => b.DoctorId == doctorId)
                    .OrderBy(b => b.StartTime)
                    .Select(b => new BlockedTimeSlotResponse
                    {
                        id = b.Id,
                        doctorId = b.DoctorId,
                        date = b.StartTime.ToString("yyyy-MM-dd"),
                        startTime = b.IsWholeDay ? null : b.StartTime.ToString("HH:mm"),
                        endTime = b.IsWholeDay ? null : b.EndTime.ToString("HH:mm"),
                        blockWholeDay = b.IsWholeDay,
                        reason = null // Don't expose reason to patients for privacy
                    })
                    .ToListAsync();

                return Ok(blockedSlots);
            }
            catch
            {
                return BadRequest(ApiErrors.BlockedSlotsCouldNotBeRetrieved);
            }
        }

        /// <summary>
        /// Create a new blocked time slot
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateBlockedTimeSlot([FromBody] CreateBlockedTimeSlotRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiErrors.ModelNotValid);
                }

                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrors.UserDoesNotExist);

                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                // Parse date
                if (!DateTime.TryParse(request.Date, out var date))
                {
                    return BadRequest(ApiErrors.InvalidDateTimeFormat);
                }

                DateTime startTime, endTime;

                if (request.IsWholeDay)
                {
                    // For whole day, set to start and end of the day
                    startTime = date.Date;
                    endTime = date.Date.AddDays(1).AddTicks(-1); // End of day
                }
                else
                {
                    // Parse start and end times
                    if (string.IsNullOrEmpty(request.StartTime) || string.IsNullOrEmpty(request.EndTime))
                    {
                        return BadRequest(ApiErrors.EndAndSrartTimeRequired);
                    }

                    if (!TimeSpan.TryParse(request.StartTime, out var startTimeSpan) ||
                        !TimeSpan.TryParse(request.EndTime, out var endTimeSpan))
                    {
                        return BadRequest(ApiErrors.InvalidTimeSpanFormat);
                    }

                    startTime = date.Date.Add(startTimeSpan);
                    endTime = date.Date.Add(endTimeSpan);
                }

                // Validate time range
                if (startTime >= endTime)
                {
                    return BadRequest(ApiErrors.EndTimeBeforeStartTime);
                }

                // Check for existing appointments in this time range
                var conflictingAppointments = await _db.Appointments
                    .Where(a => a.DoctorId == doctor.Id &&
                               a.ScheduledAt >= startTime &&
                               a.ScheduledAt < endTime)
                    .ToListAsync();

                if (conflictingAppointments.Any())
                {
                    return BadRequest(ApiErrors.BlockFailedDueToExistingAppointments);
                }

                // Check for overlapping blocked time slots
                var overlappingBlocks = await _db.BlockedTimeSlots
                    .Where(b => b.DoctorId == doctor.Id &&
                               ((b.StartTime <= startTime && b.EndTime > startTime) ||
                                (b.StartTime < endTime && b.EndTime >= endTime) ||
                                (b.StartTime >= startTime && b.EndTime <= endTime)))
                    .ToListAsync();

                if (overlappingBlocks.Any())
                {
                    return BadRequest(ApiErrors.BlockFailedDueToOverlappingBlockedSlots);
                }

                var blockedTimeSlot = new BlockedTimeSlot
                {
                    DoctorId = doctor.Id,
                    StartTime = startTime,
                    EndTime = endTime,
                    IsWholeDay = request.IsWholeDay,
                    Reason = request.Reason,
                    IsRecurring = false, // Frontend doesn't support this yet
                    RecurrencePattern = null
                };

                _db.BlockedTimeSlots.Add(blockedTimeSlot);
                await _db.SaveChangesAsync();

                var response = new BlockedTimeSlotResponse
                {
                    id = blockedTimeSlot.Id,
                    doctorId = blockedTimeSlot.DoctorId,
                    date = blockedTimeSlot.StartTime.ToString("yyyy-MM-dd"),
                    startTime = blockedTimeSlot.IsWholeDay ? null : blockedTimeSlot.StartTime.ToString("HH:mm"),
                    endTime = blockedTimeSlot.IsWholeDay ? null : blockedTimeSlot.EndTime.ToString("HH:mm"),
                    blockWholeDay = blockedTimeSlot.IsWholeDay,
                    reason = blockedTimeSlot.Reason
                };

                return Ok(response);
            }
            catch
            {
                return BadRequest(ApiErrors.BlockFailed);
            }
        }

        /// <summary>
        /// Update a blocked time slot
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlockedTimeSlot([FromRoute] string id, [FromBody] UpdateBlockedTimeSlotRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiErrors.ModelNotValid);
                }

                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrors.UserDoesNotExist);

                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                var blockedSlot = await _db.BlockedTimeSlots.FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctor.Id);
                if (blockedSlot == null) return NotFound(ApiErrors.BlockedTimeslotNotFound);

                // Update fields if provided
                if (!string.IsNullOrEmpty(request.Date))
                {
                    if (!DateTime.TryParse(request.Date, out var date))
                    {
                        return BadRequest(ApiErrors.InvalidDateTimeFormat);
                    }

                    if (request.IsWholeDay == true)
                    {
                        // For whole day, set to start and end of the day
                        blockedSlot.StartTime = date.Date;
                        blockedSlot.EndTime = date.Date.AddDays(1).AddTicks(-1);
                    }
                    else
                    {
                        // Update date part while keeping time part
                        var currentStartTime = blockedSlot.StartTime.TimeOfDay;
                        var currentEndTime = blockedSlot.EndTime.TimeOfDay;
                        blockedSlot.StartTime = date.Date.Add(currentStartTime);
                        blockedSlot.EndTime = date.Date.Add(currentEndTime);
                    }
                }

                if (!string.IsNullOrEmpty(request.StartTime) && request.IsWholeDay != true)
                {
                    if (!TimeSpan.TryParse(request.StartTime, out var startTimeSpan))
                    {
                        return BadRequest(ApiErrors.InvalidTimeSpanFormat);
                    }
                    blockedSlot.StartTime = blockedSlot.StartTime.Date.Add(startTimeSpan);
                }

                if (!string.IsNullOrEmpty(request.EndTime) && request.IsWholeDay != true)
                {
                    if (!TimeSpan.TryParse(request.EndTime, out var endTimeSpan))
                    {
                        return BadRequest(ApiErrors.InvalidTimeSpanFormat);
                    }
                    blockedSlot.EndTime = blockedSlot.EndTime.Date.Add(endTimeSpan);
                }

                if (request.IsWholeDay.HasValue)
                {
                    blockedSlot.IsWholeDay = request.IsWholeDay.Value;

                    if (request.IsWholeDay.Value)
                    {
                        // Convert to whole day
                        var date = blockedSlot.StartTime.Date;
                        blockedSlot.StartTime = date;
                        blockedSlot.EndTime = date.AddDays(1).AddTicks(-1);
                    }
                }

                if (request.Reason != null)
                    blockedSlot.Reason = request.Reason;

                // Validate updated time range
                if (blockedSlot.StartTime >= blockedSlot.EndTime)
                {
                    return BadRequest(ApiErrors.EndTimeBeforeStartTime);
                }

                await _db.SaveChangesAsync();

                var response = new BlockedTimeSlotResponse
                {
                    id = blockedSlot.Id,
                    doctorId = blockedSlot.DoctorId,
                    date = blockedSlot.StartTime.ToString("yyyy-MM-dd"),
                    startTime = blockedSlot.IsWholeDay ? null : blockedSlot.StartTime.ToString("HH:mm"),
                    endTime = blockedSlot.IsWholeDay ? null : blockedSlot.EndTime.ToString("HH:mm"),
                    blockWholeDay = blockedSlot.IsWholeDay,
                    reason = blockedSlot.Reason
                };

                return Ok(response);
            }
            catch
            {
                return BadRequest(ApiErrors.UpdatingBlockedTimeSlotFailed);
            }
        }

        /// <summary>
        /// Delete a blocked time slot
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlockedTimeSlot([FromRoute] string id)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrors.UserDoesNotExist);

                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                var blockedSlot = await _db.BlockedTimeSlots.FirstOrDefaultAsync(b => b.Id == id && b.DoctorId == doctor.Id);
                if (blockedSlot == null) return NotFound(ApiErrors.BlockedTimeslotNotFound);

                _db.BlockedTimeSlots.Remove(blockedSlot);
                await _db.SaveChangesAsync();

                return NoContent();
            }
            catch
            {
                return BadRequest(ApiErrors.DeletingBlockedTimeSlotFailed);
            }
        }
    }
}