using System.Text.RegularExpressions;

namespace MedicareApi.Utils
{
    public static class FileUploadSecurityHelper
    {
        // Dangerous file extensions that should never be allowed
        private static readonly string[] DangerousExtensions = 
        {
            ".exe", ".bat", ".cmd", ".com", ".scr", ".vbs", ".vbe", ".js", ".jse", 
            ".wsf", ".wsh", ".msi", ".msp", ".hta", ".jar", ".pif", ".application",
            ".gadget", ".msc", ".ps1", ".ps1xml", ".ps2", ".ps2xml", ".psc1", ".psc2",
            ".msh", ".msh1", ".msh2", ".mshxml", ".msh1xml", ".msh2xml", ".reg", ".asp",
            ".aspx", ".php", ".jsp", ".py", ".rb", ".pl", ".cgi", ".sh", ".bash"
        };

        /// <summary>
        /// Sanitizes a filename by removing unsafe characters and path traversal sequences
        /// </summary>
        /// <param name="filename">Original filename to sanitize</param>
        /// <returns>Sanitized filename safe for filesystem use</returns>
        public static string SanitizeFilename(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return "file";

            // Remove path separators and dangerous characters
            var sanitized = filename;
            
            // Remove path traversal patterns
            sanitized = sanitized.Replace("..", "");
            sanitized = sanitized.Replace("./", "/");
            sanitized = sanitized.Replace(".\\", "\\");
            
            // Remove or replace unsafe characters for filenames
            // Keep only alphanumeric, periods, hyphens, underscores
            sanitized = Regex.Replace(sanitized, @"[^a-zA-Z0-9._-]", "_");
            
            // Remove any leading/trailing periods or spaces
            sanitized = sanitized.Trim().Trim('.');
            
            // Ensure filename is not empty after sanitization
            if (string.IsNullOrWhiteSpace(sanitized))
                return "file";
                
            // Limit filename length
            if (sanitized.Length > 100)
                sanitized = sanitized.Substring(0, 100);
                
            return sanitized;
        }

        /// <summary>
        /// Checks if a file extension is considered dangerous/executable
        /// </summary>
        /// <param name="extension">File extension (with or without leading dot)</param>
        /// <returns>True if the extension is dangerous, false otherwise</returns>
        public static bool IsDangerousExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension))
                return false;

            var normalizedExtension = extension.ToLowerInvariant();
            if (!normalizedExtension.StartsWith("."))
                normalizedExtension = "." + normalizedExtension;

            return DangerousExtensions.Contains(normalizedExtension);
        }

        /// <summary>
        /// Validates that a filename is safe for upload
        /// </summary>
        /// <param name="filename">Original filename</param>
        /// <param name="allowedExtensions">Array of allowed extensions (e.g., [".jpg", ".png"])</param>
        /// <returns>Validation result with success status and error message</returns>
        public static (bool isValid, string? errorMessage) ValidateFilename(string filename, string[] allowedExtensions)
        {
            if (string.IsNullOrWhiteSpace(filename))
                return (false, "Filename cannot be empty");

            var extension = Path.GetExtension(filename)?.ToLowerInvariant();
            
            if (string.IsNullOrEmpty(extension))
                return (false, "File must have an extension");

            // Check for dangerous extensions first
            if (IsDangerousExtension(extension))
                return (false, $"File extension '{extension}' is not allowed for security reasons");

            // Check if extension is in allowed list
            if (allowedExtensions?.Length > 0 && !allowedExtensions.Contains(extension))
            {
                var allowedList = string.Join(", ", allowedExtensions);
                return (false, $"File extension '{extension}' is not allowed. Allowed extensions: {allowedList}");
            }

            return (true, null);
        }

        /// <summary>
        /// Generates a secure filename by combining sanitized components
        /// </summary>
        /// <param name="originalFilename">Original filename from upload</param>
        /// <param name="prefix">Prefix to add (e.g., user ID)</param>
        /// <param name="addGuid">Whether to add a GUID for uniqueness</param>
        /// <returns>Secure filename ready for filesystem use</returns>
        public static string GenerateSecureFilename(string originalFilename, string? prefix = null, bool addGuid = true)
        {
            var extension = Path.GetExtension(originalFilename)?.ToLowerInvariant() ?? ".tmp";
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalFilename);
            
            var sanitizedName = SanitizeFilename(nameWithoutExtension);
            var sanitizedPrefix = !string.IsNullOrEmpty(prefix) ? SanitizeFilename(prefix) : null;
            
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(sanitizedPrefix))
                parts.Add(sanitizedPrefix);
                
            if (addGuid)
                parts.Add(Guid.NewGuid().ToString("N")[..8]); // Use first 8 chars of GUID
                
            parts.Add(sanitizedName);
            
            var finalName = string.Join("_", parts.Where(p => !string.IsNullOrEmpty(p)));
            
            return finalName + extension;
        }
    }
}