using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IPaymentService _paymentService;

        public PaymentController(ApplicationDbContext db, IPaymentService paymentService)
        {
            _db = db;
            _paymentService = paymentService;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (isDoctor) return Forbid("Doctors cannot make payments");

            // Verify appointment exists and belongs to the patient
            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId && a.PatientId == userId);
            
            if (appointment == null)
                return NotFound("Appointment not found");

            if (appointment.Status != AppointmentStatus.Booked)
                return BadRequest("Payment can only be processed for booked appointments");

            if (!string.IsNullOrEmpty(appointment.PaymentId))
                return BadRequest("Payment has already been processed for this appointment");

            try
            {
                var paymentResult = await _paymentService.ProcessPaymentAsync(request, appointment.ConsultationFee);
                
                if (paymentResult.Success)
                {
                    appointment.PaymentId = paymentResult.PaymentId;
                    await _db.SaveChangesAsync();
                }

                return Ok(paymentResult);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Payment processing failed", error = ex.Message });
            }
        }

        [HttpGet("appointment/{appointmentId}")]
        public async Task<IActionResult> GetPaymentStatus([FromRoute] string appointmentId)
        {
            var userId = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.Id == appointmentId && 
                    (a.PatientId == userId || _db.Doctors.Any(d => d.Id == a.DoctorId && d.UserId == userId)));
            
            if (appointment == null)
                return NotFound("Appointment not found");

            if (string.IsNullOrEmpty(appointment.PaymentId))
                return Ok(new { paymentStatus = "No payment required" });

            var payment = await _db.Payments.FindAsync(appointment.PaymentId);
            return Ok(new 
            { 
                paymentId = payment?.Id,
                paymentStatus = payment?.Status.ToString(),
                amount = payment?.Amount,
                paymentMethod = payment?.PaymentMethod.ToString(),
                transactionReference = payment?.TransactionReference
            });
        }
    }
}