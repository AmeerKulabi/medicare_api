namespace MedicareApi.Models
{
    public class PaymentRequest
    {
        public string AppointmentId { get; set; } = string.Empty;
        public PaymentMethod PaymentMethod { get; set; }
        public string? CardNumber { get; set; }
        public string? CardHolderName { get; set; }
        public string? ExpiryMonth { get; set; }
        public string? ExpiryYear { get; set; }
        public string? CVV { get; set; }
        public string? ZeinCashPhone { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public string? TransactionReference { get; set; }
    }
}