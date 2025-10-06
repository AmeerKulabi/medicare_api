namespace MedicareApi.Utils
{
    public static class ProfilePictureHelper
    {
        private static readonly string[] AllowedTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif" };
        private const int MaxFileSizeInBytes = 5 * 1024 * 1024; // 5MB
        private const string DefaultProfilePictureUrl = "/profile_pictures/default-silhouette.svg";
        private const string ProfilePicturesDirectory = "profile_pictures";

        public static string GetProfilePictureUrl(string? currentUrl)
        {
            return string.IsNullOrEmpty(currentUrl) ? null : currentUrl;
        }

        public static async Task<(bool success, string? url, string? error)> SaveProfilePicture(IFormFile file, string doctorId)
        {
            if (file == null || file.Length == 0)
                return (false, null, "No file uploaded");

            // Validate file type
            if (!AllowedTypes.Contains(file.ContentType.ToLower()))
                return (false, null, "Invalid file type. Only JPEG, JPG, PNG, and GIF files are allowed.");

            // Validate file size
            if (file.Length > MaxFileSizeInBytes)
                return (false, null, "File size too large. Maximum size is 5MB.");

            // Enhanced security: validate filename using FileUploadSecurityHelper
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var filenameValidation = FileUploadSecurityHelper.ValidateFilename(file.FileName, allowedExtensions);
            if (!filenameValidation.isValid)
                return (false, null, filenameValidation.errorMessage);

            // Sanitize doctor ID to prevent path traversal
            var sanitizedDoctorId = FileUploadSecurityHelper.SanitizeFilename(doctorId);
            if (string.IsNullOrEmpty(sanitizedDoctorId))
                return (false, null, "Invalid doctor ID");

            try
            {
                // Generate secure filename using the security helper
                var fileName = FileUploadSecurityHelper.GenerateSecureFilename(file.FileName, sanitizedDoctorId, true);
                var directoryPath = Path.Combine("wwwroot", ProfilePicturesDirectory);
                var filePath = Path.Combine(directoryPath, fileName);

                // Additional security: ensure the file path is within expected directory
                var fullDirectoryPath = Path.GetFullPath(directoryPath);
                var fullFilePath = Path.GetFullPath(filePath);
                if (!fullFilePath.StartsWith(fullDirectoryPath))
                    return (false, null, "Invalid file path");

                // Ensure directory exists
                Directory.CreateDirectory(directoryPath);

                // Save file to disk with additional validation
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Verify file was written correctly
                if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
                    return (false, null, "Failed to save file");

                return (true, $"/{ProfilePicturesDirectory}/{fileName}", null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error saving file: {ex.Message}");
            }
        }

        public static void DeleteProfilePicture(string? profilePictureUrl)
        {
            if (string.IsNullOrEmpty(profilePictureUrl) || profilePictureUrl.Contains("default-silhouette"))
                return;

            try
            {
                var fileName = Path.GetFileName(profilePictureUrl);
                var filePath = Path.Combine("wwwroot", ProfilePicturesDirectory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch
            {
                // Ignore deletion errors
            }
        }
    }
}