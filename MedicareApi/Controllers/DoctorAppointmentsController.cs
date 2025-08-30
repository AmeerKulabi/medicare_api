using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class DoctorAppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorAppointmentsController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointments()
        {

            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return Unauthorized();

            var appointments = await _db.Appointments.Where(a => a.DoctorId == doctor.Id).ToListAsync();
            var doctorAppointments = new List<DoctorAppointment>();
            foreach(var appointment in appointments)
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

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";

            // Non-doctor users can only create appointments for themselves
            if (!isDoctor && appointment.PatientId != userId)
                return Unauthorized();

            // If user is a doctor, verify they can create appointments
            if (isDoctor)
            {
                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null) return Unauthorized();
                
                // Doctor can create appointments for any patient, but must be for their own doctor record
                if (appointment.DoctorId != doctor.Id)
                    return Unauthorized();
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
            return Ok(newAppointment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment([FromRoute] string id, [FromBody] UpdateAppointment updates)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null) return NotFound();

            if(updates.ScheduledAt != null)
                appt.ScheduledAt = (DateTime)updates.ScheduledAt;

            if(updates.Status != null)
                appt.Status = updates.Status;

            if(updates.Reason != null)
                appt.Reason = updates.Reason;
            // Update more fields as needed
            await _db.SaveChangesAsync();
            return Ok(appt);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment([FromRoute] string id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id);
            if (appt == null) return NotFound();

            _db.Appointments.Remove(appt);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            var jan4 = new DateTime(year, 1, 4); // ISO 8601: Week 1 always contains Jan 4
            var dayOfWeek = (int)jan4.DayOfWeek;
            dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek; // Convert Sunday (0) to 7

            var mondayOfWeek1 = jan4.AddDays(-1 * (dayOfWeek - 1)); // Move to Monday of week 1
            return mondayOfWeek1.AddDays((weekOfYear - 1) * 7);
        }

        private static int GetCurrentWeekNumber()
        {
            // Get the current date
            DateTime currentDate = DateTime.Now;

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
        [FromQuery] int week)
        {

            var year = DateTime.UtcNow.Year; // or allow it to be passed too
            var weekNum = GetCurrentWeekNumber() + week;
            var startOfWeek = FirstDateOfWeekISO8601(year, weekNum);
            var endOfWeek = startOfWeek.AddDays(7);
            var blockedAppointments = _db.Appointments
                .Where(a =>
                    a.DoctorId == id &&
                    a.ScheduledAt >= startOfWeek &&
                    a.ScheduledAt < endOfWeek)
                .ToList();
            var weekBookedSlots = new List<List<string>>();
            weekBookedSlots.Add(new List<string>());
            weekBookedSlots.Add(new List<string>());
            weekBookedSlots.Add(new List<string>());
            weekBookedSlots.Add(new List<string>());
            weekBookedSlots.Add(new List<string>());
            weekBookedSlots.Add(new List<string>());
            weekBookedSlots.Add(new List<string>());

            foreach( var b in blockedAppointments)
            {
                if (b.ScheduledAt.DayOfWeek == DayOfWeek.Sunday)
                    weekBookedSlots[0].Add(b.ScheduledAt.TimeOfDay.ToString());
                else if (b.ScheduledAt.DayOfWeek == DayOfWeek.Monday)
                    weekBookedSlots[1].Add(b.ScheduledAt.TimeOfDay.ToString());
                else if (b.ScheduledAt.DayOfWeek == DayOfWeek.Tuesday)
                    weekBookedSlots[2].Add(b.ScheduledAt.TimeOfDay.ToString());
                else if (b.ScheduledAt.DayOfWeek == DayOfWeek.Wednesday)
                    weekBookedSlots[3].Add(b.ScheduledAt.TimeOfDay.ToString());
                else if (b.ScheduledAt.DayOfWeek == DayOfWeek.Thursday)
                    weekBookedSlots[4].Add(b.ScheduledAt.TimeOfDay.ToString());
                else if (b.ScheduledAt.DayOfWeek == DayOfWeek.Friday)
                    weekBookedSlots[5].Add(b.ScheduledAt.TimeOfDay.ToString());
                else if (b.ScheduledAt.DayOfWeek == DayOfWeek.Saturday)
                    weekBookedSlots[6].Add(b.ScheduledAt.TimeOfDay.ToString());
                else
                    throw new Exception("Invalid date for an appointment");
            }

            return Ok(weekBookedSlots);
        }
    }
}