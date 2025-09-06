using MedicareApi.Models;
using MedicareApi.Data;
using Microsoft.EntityFrameworkCore;

namespace MedicareApi.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, decimal amount);
        Task<bool> RefundPaymentAsync(string paymentId);
        Task<bool> CompletePaymentTransferAsync(string paymentId);
    }

    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _db;

        public PaymentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, decimal amount)
        {
            // Create payment record
            var payment = new Payment
            {
                AppointmentId = request.AppointmentId,
                Amount = amount,
                PaymentMethod = request.PaymentMethod,
                Status = PaymentStatus.Pending
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            // Simulate payment processing based on method
            try
            {
                string transactionRef = await SimulatePaymentProcessing(request, amount);
                
                payment.Status = PaymentStatus.Completed;
                payment.ProcessedAt = DateTime.UtcNow;
                payment.TransactionReference = transactionRef;
                
                await _db.SaveChangesAsync();

                return new PaymentResponse
                {
                    Success = true,
                    PaymentId = payment.Id,
                    TransactionReference = transactionRef
                };
            }
            catch (Exception ex)
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = ex.Message;
                await _db.SaveChangesAsync();

                return new PaymentResponse
                {
                    Success = false,
                    PaymentId = payment.Id,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<bool> RefundPaymentAsync(string paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null || payment.Status != PaymentStatus.Completed)
                return false;

            // Simulate refund processing
            payment.Status = PaymentStatus.Refunded;
            await _db.SaveChangesAsync();
            
            return true;
        }

        public async Task<bool> CompletePaymentTransferAsync(string paymentId)
        {
            var payment = await _db.Payments.FindAsync(paymentId);
            if (payment == null || payment.Status != PaymentStatus.Completed)
                return false;

            // In a real implementation, this would transfer money to the doctor
            // For now, we just mark it as completed (already done)
            return true;
        }

        private async Task<string> SimulatePaymentProcessing(PaymentRequest request, decimal amount)
        {
            // Simulate API call delay
            await Task.Delay(500);

            // Basic validation
            switch (request.PaymentMethod)
            {
                case PaymentMethod.Mastercard:
                case PaymentMethod.Visa:
                    if (string.IsNullOrEmpty(request.CardNumber) || 
                        string.IsNullOrEmpty(request.CVV) || 
                        string.IsNullOrEmpty(request.ExpiryMonth) || 
                        string.IsNullOrEmpty(request.ExpiryYear))
                        throw new ArgumentException("Missing required card information");
                    break;
                    
                case PaymentMethod.ZeinCash:
                    if (string.IsNullOrEmpty(request.ZeinCashPhone))
                        throw new ArgumentException("Missing ZeinCash phone number");
                    break;
            }

            // Generate mock transaction reference
            return $"TXN_{request.PaymentMethod}_{DateTime.UtcNow.Ticks}";
        }
    }
}