namespace MedicareApi.Services
{
    /// <summary>
    /// Interface for email service.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends a password reset email to the specified email address.
        /// </summary>
        /// <param name="email">Recipient email address</param>
        /// <param name="resetLink">Password reset link</param>
        /// <param name="appName">Application name</param>
        /// <returns>Task representing the async operation</returns>
        Task SendPasswordResetEmailAsync(string email, string resetLink, string appName);
    }
}