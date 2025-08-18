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
            return string.IsNullOrEmpty(currentUrl) ? DefaultProfilePictureUrl : currentUrl;
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

            try
            {
                // Generate unique filename
                var extension = Path.GetExtension(file.FileName);
                var fileName = $"{doctorId}_{Guid.NewGuid()}{extension}";
                var directoryPath = Path.Combine("wwwroot", ProfilePicturesDirectory);
                var filePath = Path.Combine(directoryPath, fileName);

                // Ensure directory exists
                Directory.CreateDirectory(directoryPath);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return (true, $"/{ProfilePicturesDirectory}/{fileName}", null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
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