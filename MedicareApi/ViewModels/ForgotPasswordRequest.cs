using System.ComponentModel.DataAnnotations;

namespace MedicareApi.ViewModels
{
    /// <summary>
    /// Model for forgot password request.
    /// </summary>
    public class ForgotPasswordRequest
    {
        /// <summary>
        /// User's email address for password reset.
        /// </summary>
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;
    }
}