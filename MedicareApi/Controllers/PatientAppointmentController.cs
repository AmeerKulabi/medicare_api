using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicareApi.Services;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/patient/appointments")]
    [Authorize]
    public class PatientAppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        /// <summary>
        /// Email service.
        /// </summary>
        private readonly IEmailService _emailService;

        public PatientAppointmentController(ApplicationDbContext db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatientAppointment([FromRoute] string id)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (isDoctor) return Unauthorized();

            var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == userId);
            if (appt == null) return NotFound();

            Doctor doctor = doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.Id == appt.DoctorId);

            _db.Appointments.Remove(appt);
            await _db.SaveChangesAsync();
            if(_emailService != null )
                _emailService.SendAppointmentDeleted(User.Identity.Name, appt.ScheduledAt, doctor.Name, doctor.ClinicAddress, doctor.Phone);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientAppointments([FromQuery] String type)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (isDoctor) return Unauthorized();

            var appointments = new List<Appointment>();
            if(type == "past")
                appointments = await _db.Appointments.Where(a => a.PatientId == userId && a.ScheduledAt < DateTime.Now).ToListAsync();
            else
                appointments = await _db.Appointments.Where(a => a.PatientId == userId && a.ScheduledAt > DateTime.Now).ToListAsync();
            var patientAppointments = new List<PatientAppointment>();
            foreach( Appointment appointment in appointments )
            {
                var doctor = await _db.Doctors.FirstAsync(a => a.Id == appointment.DoctorId);
                patientAppointments.Add(new PatientAppointment
                {
                    id = appointment.Id,
                    doctorName = doctor.Name,
                    doctorId = doctor.Id,
                    doctorSpecialization = doctor.Specialization,
                    clinicName = doctor.ClinicName,
                    date = appointment.ScheduledAt.Date.ToShortDateString(),
                    time = appointment.ScheduledAt.TimeOfDay.ToString(),
                    duration = 30,
                    reason = appointment.Reason ?? "",
                    status = appointment.Status,
                    type = "consultation",
                    address = null, // Removed deprecated ClinicAddress field
                    phone = null, // Removed deprecated Phone field
                    consultationFee = int.Parse(doctor.ConsultationFee),
                });
            }
            
            return Ok(patientAppointments);


        }
    }
}
