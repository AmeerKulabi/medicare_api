using System.ComponentModel.DataAnnotations;

namespace MedicareApi.Models
{
    /// <summary>
    /// Standardized error response model for API errors.
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>
        /// Specific error code that frontend can use programmatically.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Additional details about the error (optional).
        /// </summary>
        public string? Details { get; set; }

        /// <summary>
        /// Timestamp when the error occurred.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Predefined error codes for consistent error handling.
    /// </summary>
    public static class ErrorCodes
    {
        // Authentication errors
        public const string INVALID_EMAIL = "INVALID_EMAIL";
        public const string INVALID_PASSWORD = "INVALID_PASSWORD";
        public const string USER_NOT_FOUND = "USER_NOT_FOUND";
        public const string ACCOUNT_LOCKED = "ACCOUNT_LOCKED";
        
        // Authorization errors
        public const string UNAUTHORIZED_ACCESS = "UNAUTHORIZED_ACCESS";
        public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
        public const string TOKEN_INVALID = "TOKEN_INVALID";
        public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";
        
        // Validation errors
        public const string VALIDATION_ERROR = "VALIDATION_ERROR";
        public const string INVALID_REQUEST = "INVALID_REQUEST";
        
        // Registration errors
        public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
        public const string WEAK_PASSWORD = "WEAK_PASSWORD";
        
        // General errors
        public const string INTERNAL_ERROR = "INTERNAL_ERROR";
        public const string NOT_FOUND = "NOT_FOUND";
        public const string BAD_REQUEST = "BAD_REQUEST";
    }
}