using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/patient/appointments")]
    [Authorize]
    public class PatientAppointmentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPaymentService _paymentService;

        public PatientAppointmentController(ApplicationDbContext db, IPaymentService paymentService)
        {
            _db = db;
            _paymentService = paymentService;
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

            // Check if appointment can be canceled by patient (not within 24 hours and not confirmed)
            var hoursUntilAppointment = (appt.ScheduledAt - DateTime.UtcNow).TotalHours;
            if (hoursUntilAppointment <= 24 || appt.Status == AppointmentStatus.Confirmed)
            {
                return BadRequest("Cannot cancel appointment within 24 hours or when confirmed");
            }

            if (appt.Status == AppointmentStatus.Done || appt.Status == AppointmentStatus.Canceled)
            {
                return BadRequest("Cannot cancel completed or already canceled appointment");
            }

            appt.Status = AppointmentStatus.Canceled;
            appt.CanceledAt = DateTime.UtcNow;
            appt.CanceledBy = userId;
            appt.CancellationReason = "Canceled by patient";

            // Refund payment if it was processed
            if (!string.IsNullOrEmpty(appt.PaymentId))
            {
                await _paymentService.RefundPaymentAsync(appt.PaymentId);
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientAppointment([FromRoute] string id, [FromBody] UpdateAppointment updates)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (isDoctor) return Unauthorized();

            var appt = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == id && a.PatientId == userId);
            if (appt == null) return NotFound();

            // Check if appointment can be modified by patient (not within 24 hours and not confirmed)
            var hoursUntilAppointment = (appt.ScheduledAt - DateTime.UtcNow).TotalHours;
            if (hoursUntilAppointment <= 24 || appt.Status == AppointmentStatus.Confirmed)
            {
                return BadRequest("Cannot modify appointment within 24 hours or when confirmed");
            }

            if (appt.Status == AppointmentStatus.Done || appt.Status == AppointmentStatus.Canceled)
            {
                return BadRequest("Cannot modify completed or canceled appointment");
            }

            // Patients can only update certain fields
            if (updates.ScheduledAt != null)
                appt.ScheduledAt = (DateTime)updates.ScheduledAt;

            if (updates.Reason != null)
                appt.Reason = updates.Reason;

            // Patients cannot directly change status except for cancellation (which should use DELETE)
            if (updates.Status.HasValue && updates.Status != AppointmentStatus.Canceled)
            {
                return BadRequest("Patients cannot change appointment status");
            }

            await _db.SaveChangesAsync();
            return Ok(appt);
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
                    status = appointment.Status.ToString().ToLower(),
                    type = "consultation",
                    address = doctor.ClinicAddress,
                    phone = doctor.Phone,
                    consultationFee = int.Parse(doctor.ConsultationFee),
                });
            }
            
            return Ok(patientAppointments);


        }
    }
}
