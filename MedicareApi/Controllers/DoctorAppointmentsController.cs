using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using MedicareApi.Utils;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class DoctorAppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        /// <summary>
        /// Email service.
        /// </summary>
        private readonly IEmailService _emailService;

        public DoctorAppointmentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager,
            IEmailService emailService)
        {
            _db = db;
            _userManager = userManager;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrors.UserDoesNotExist);

                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);
#if DEBUG
                var appointments = await _db.Appointments.Where(a => a.DoctorId == doctor.Id && a.ScheduledAt > DateTime.Now).ToListAsync();
#else
                // Get Iraq's timezone (Baghdad)
                        TimeZoneInfo iraqTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabic Standard Time");
        
                        // Get the current UTC time
                        DateTime utcNow = DateTime.UtcNow;
        
                        // Convert to Iraq local time
                        DateTime iraqTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, iraqTimeZone);
        
                        Console.WriteLine("Current time in Iraq (Baghdad): " + iraqTime);
                        var appointments = await _db.Appointments.Where(a => a.DoctorId == doctor.Id a.ScheduledAt > iraqTime).ToListAsync();
#endif

                var doctorAppointments = new List<DoctorAppointment>();
                foreach (var appointment in appointments)
                {
                    // Get patient information from Identity Framework
                    var patient = await _userManager.FindByIdAsync(appointment.PatientId);

                    var doctorAppointment = new DoctorAppointment()
                    {
                        id = appointment.Id,
                        patientName = patient?.FullName ?? "Unknown",
                        patientEmail = patient?.Email ?? "Unknown",
                        patientPhone = patient?.Phone ?? "Unknown",
                        date = appointment.ScheduledAt.Date.ToShortDateString(),
                        time = appointment.ScheduledAt.TimeOfDay.ToString(),
                        duration = "30",
                        reason = appointment.Reason ?? "",
                        status = "confirmed",
                        type = "consultation"

                    };
                    doctorAppointments.Add(doctorAppointment);
                }
                return Ok(doctorAppointments);
            }
            catch
            {
                return BadRequest(ApiErrors.RetrievingAppointmentsFailed);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
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

                // Non-doctor users can only create appointments for themselves
                if (!isDoctor && appointment.PatientId != userId)
                    return Unauthorized(ApiErrors.CannotCraeteAppointmentsForOthers);

                // If user is a doctor, verify they can create appointments
                Doctor doctor;
                if (isDoctor)
                {
                    doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                    if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                    // Doctor can create appointments for any patient, but must be for their own doctor record
                    if (appointment.DoctorId != doctor.Id)
                        return Unauthorized(ApiErrors.CannotCreateAppointmentsForOtherDoctors);
                }
                else
                {
                    doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == appointment.DoctorId);
                }

                // Check for existing appointments at the same time
                var existingAppointment = await _db.Appointments
                    .FirstOrDefaultAsync(a => a.DoctorId == appointment.DoctorId &&
                                            a.ScheduledAt == appointment.ScheduledAt);

                if (existingAppointment != null)
                {
                    return BadRequest(ApiErrors.AppointmentAlreadyBooked);
                }

                // Check for blocked time slots
                var blockedTimeConflict = await _db.BlockedTimeSlots
                    .AnyAsync(b => b.DoctorId == appointment.DoctorId &&
                                 b.StartTime <= appointment.ScheduledAt &&
                                 b.EndTime > appointment.ScheduledAt);

                if (blockedTimeConflict)
                {
                    return BadRequest(ApiErrors.TimeSlotNotAvailableForBooking);
                }

                Appointment newAppointment = new Appointment()
                {
                    PatientId = appointment.PatientId,
                    DoctorId = appointment.DoctorId,
                    Status = appointment.Status,
                    ScheduledAt = appointment.ScheduledAt,
                    Reason = appointment.Reason,
                };
                _db.Appointments.Add(newAppointment);
                await _db.SaveChangesAsync();
                if (_emailService != null)
                    _emailService.SendAppointmentBooked(User.Identity.Name, newAppointment.ScheduledAt, doctor.Name, doctor.ClinicAddress, "");

                // Return formatted response for calendar display
                var response = new
                {
                    id = newAppointment.Id,
                    patientId = newAppointment.PatientId,
                    doctorId = newAppointment.DoctorId,
                    status = newAppointment.Status,
                    scheduledAt = newAppointment.ScheduledAt,
                    date = newAppointment.ScheduledAt.ToString("yyyy-MM-dd"),
                    time = newAppointment.ScheduledAt.ToString("HH:mm"),
                    reason = newAppointment.Reason
                };
                return Ok(response);
            }
            catch
            {
                return BadRequest(ApiErrors.TimeSlotCouldNotBeBooked);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment([FromRoute] string id, [FromBody] UpdateAppointment updates)
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


                var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
                Appointment oldApt = appt;
                if (appt == null) return NotFound(ApiErrors.AppointmentNotFound);

                if (updates.ScheduledAt != null)
                    appt.ScheduledAt = (DateTime)updates.ScheduledAt;

                if (updates.Status != null)
                    appt.Status = updates.Status;

                if (updates.Reason != null)
                    appt.Reason = updates.Reason;

                // If user is a doctor, verify they can create appointments
                Doctor doctor;
                if (isDoctor)
                {
                    doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                    if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                    // Doctor can create appointments for any patient, but must be for their own doctor record
                    if (appt.DoctorId != doctor.Id)
                        return Unauthorized(ApiErrors.CannotUpdateAppointmentsOfOtherDoctors);
                }
                else
                {
                    doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == appt.DoctorId);
                }

                // Update more fields as needed
                await _db.SaveChangesAsync();
                if (_emailService != null)
                    _emailService.SendAppointmentChanged(User.Identity.Name, oldApt.ScheduledAt, appt.ScheduledAt, doctor.Name, doctor.ClinicAddress, "");
                return Ok(appt);
            }
            catch
            {
                return BadRequest(ApiErrors.AppointmentsUpdateFailed);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment([FromRoute] string id)
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                if (string.IsNullOrEmpty(userId)) return Unauthorized(ApiErrors.UserDoesNotExist);

                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                if (!isDoctor) return Unauthorized(ApiErrors.FunctionalityAvailableOnlyForDoctors);

                var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
                if (appt == null) return NotFound(ApiErrors.AppointmentNotFound);

                _db.Appointments.Remove(appt);
                await _db.SaveChangesAsync();

                // If user is a doctor, verify they can create appointments
                Doctor doctor;
                if (isDoctor)
                {
                    doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                    if (doctor == null) return Unauthorized(ApiErrors.UserDoesNotExist);

                    // Doctor can create appointments for any patient, but must be for their own doctor record
                    if (appt.DoctorId != doctor.Id)
                        return Unauthorized(ApiErrors.CannotDeleteAppointmentsOfOthers);
                }
                else
                {
                    doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == appt.DoctorId);
                }

                if (_emailService != null)
                    _emailService.SendAppointmentDeleted(User.Identity.Name, appt.ScheduledAt, doctor.Name, doctor.ClinicAddress, "");
                return NoContent();
            }
            catch
            {
                return BadRequest(ApiErrors.AppointmentsDeletionFailed);
            }
        }

        public static DateTime FirstDateOfWeekSundayStart(int year, int weekOfYear)
        {
            // In Sunday-based weeks, Week 1 is the week containing Jan 1
            var jan1 = new DateTime(year, 1, 1);

            // Find the Sunday on or before Jan 1
            int offset = (int)jan1.DayOfWeek; // Sunday = 0, Monday = 1, ..., Saturday = 6
            var sundayOfWeek1 = jan1.AddDays(-offset);

            // Add weeks
            return sundayOfWeek1.AddDays((weekOfYear - 1) * 7);
        }

        private static int GetCurrentWeekNumber()
        {
#if DEBUG
            // Get the current date
            DateTime currentDate = DateTime.Now;
#else
                                    TimeZoneInfo iraqTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Arabic Standard Time");
        
                        // Get the current UTC time
                        DateTime utcNow = DateTime.UtcNow;
        
                        // Convert to Iraq local time
                        DateTime currentDate = TimeZoneInfo.ConvertTimeFromUtc(utcNow, iraqTimeZone);
#endif


            // Get the Calendar instance from the current culture
            Calendar calendar = CultureInfo.CurrentCulture.Calendar;

            // Get the CalendarWeekRule and the first day of the week from the current culture
            CalendarWeekRule calendarWeekRule = CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule;
            DayOfWeek firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

            // Get the week number
            return calendar.GetWeekOfYear(currentDate, calendarWeekRule, firstDayOfWeek);
        }


        [HttpGet("doctor/{id}")]
        public IActionResult GetBlockedAvailability(
        [FromRoute] string id,
        [FromQuery] int? year,
        [FromQuery] int? month)
        {
            try
            {
                // Default to current month if not specified
                var targetYear = year ?? DateTime.UtcNow.Year;
                var targetMonth = month ?? DateTime.UtcNow.Month;
                targetMonth++;

                // Calculate date range for the month
                var startOfMonth = new DateTime(targetYear, targetMonth, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                // Get booked appointments
                var bookedAppointments = _db.Appointments
                        .Where(a =>
                            a.DoctorId == id &&
                        a.ScheduledAt >= startOfMonth &&
                        a.ScheduledAt < endOfMonth)
                    .ToList();

                // Get blocked time slots
                var blockedTimeSlots = _db.BlockedTimeSlots
                    .Where(b =>
                        b.DoctorId == id &&
                        b.StartTime < endOfMonth &&
                        b.EndTime >= startOfMonth)
                    .ToList();

                // Create flat array of slots
                var slots = new List<object>();

                // Add booked appointments
                foreach (var appointment in bookedAppointments)
                {
                    slots.Add(new
                    {
                        date = appointment.ScheduledAt.ToString("yyyy-MM-dd"),
                        time = appointment.ScheduledAt.ToString("HH:mm"),
                        type = "booked"
                    });
                }

                // Add blocked time slots
                foreach (var blocked in blockedTimeSlots)
                {
                    if (blocked.IsWholeDay)
                    {
                        // For whole day blocks, add a single entry for the date
                        slots.Add(new
                        {
                            date = blocked.StartTime.ToString("yyyy-MM-dd"),
                            time = (string)null,
                            type = "blocked",
                            wholeDay = true
                        });
                    }
                    else
                    {
                        // For time-specific blocks, add entry with time range
                        slots.Add(new
                        {
                            date = blocked.StartTime.ToString("yyyy-MM-dd"),
                            time = blocked.StartTime.ToString("HH:mm"),
                            endTime = blocked.EndTime.ToString("HH:mm"),
                            type = "blocked",
                            wholeDay = false
                        });
                    }
                }
                return Ok(slots);
            }
            catch
            {
                return BadRequest(ApiErrors.BlockedSlotsCouldNotBeRetrieved);
            }
        }
    }
}