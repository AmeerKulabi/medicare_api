using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    public class ForgotPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Current password is required")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class EmailConfirmationRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;
    }

    public class ResendConfirmationRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(256, ErrorMessage = "Email must not exceed 256 characters")]
        public string Email { get; set; } = string.Empty;
    }
}