using MedicareApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/upload")]
    [Authorize]
    public class FileUploadController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _environment;
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private const long _maxFileSize = 5 * 1024 * 1024; // 5MB

        public FileUploadController(ApplicationDbContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        [HttpPost("profile-picture")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadProfilePicture(IFormFile file)
        {
            try
            {
                Console.WriteLine($"File upload request received. File: {file?.FileName}, Size: {file?.Length}");
                
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "No file uploaded" });

                // Validate file size
                if (file.Length > _maxFileSize)
                    return BadRequest(new { message = "File size exceeds the limit of 5MB" });

                // Validate file extension
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed" });

                // Get the current user
                var userId = User.FindFirst("uid")?.Value;
                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                
                Console.WriteLine($"User authentication - UserId: {userId}, IsDoctor: {isDoctor}");
                
                if (!isDoctor || string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Only doctors can upload profile pictures" });

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                Console.WriteLine($"Doctor found: {doctor.Id}, Current ProfilePictureUrl: {doctor.ProfilePictureUrl}");

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath ?? "wwwroot", "profile_pictures");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                    Console.WriteLine($"Created uploads directory: {uploadsPath}");
                }

                // Log the upload path for debugging
                Console.WriteLine($"Upload path: {uploadsPath}");

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);
                
                Console.WriteLine($"Saving file as: {fileName} at {filePath}");

                // Delete old profile picture if exists
                if (!string.IsNullOrEmpty(doctor.ProfilePictureUrl))
                {
                    var oldFileName = Path.GetFileName(doctor.ProfilePictureUrl);
                    if (oldFileName != "default-silhouette.svg")
                    {
                        var oldFilePath = Path.Combine(uploadsPath, oldFileName);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                            Console.WriteLine($"Deleted old profile picture: {oldFilePath}");
                        }
                    }
                }

                // Save the new file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Verify file was created
                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"ERROR: File was not saved at {filePath}");
                    return StatusCode(500, new { message = "File was not saved successfully" });
                }

                // Log successful file creation
                Console.WriteLine($"File saved successfully at: {filePath}");

                // Update doctor's profile picture URL
                var fileUrl = $"/profile_pictures/{fileName}";
                doctor.ProfilePictureUrl = fileUrl;
                
                _db.Doctors.Update(doctor);
                await _db.SaveChangesAsync();

                Console.WriteLine($"Database updated with new ProfilePictureUrl: {fileUrl}");

                return Ok(new { 
                    message = "Profile picture uploaded successfully", 
                    profilePictureUrl = fileUrl 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in UploadProfilePicture: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "Error uploading file", error = ex.Message });
            }
        }

        [HttpDelete("profile-picture")]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            try
            {
                var userId = User.FindFirst("uid")?.Value;
                var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
                
                if (!isDoctor || string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Only doctors can delete profile pictures" });

                var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor == null)
                    return NotFound(new { message = "Doctor not found" });

                // Delete physical file if exists
                if (!string.IsNullOrEmpty(doctor.ProfilePictureUrl))
                {
                    var fileName = Path.GetFileName(doctor.ProfilePictureUrl);
                    if (fileName != "default-silhouette.svg")
                    {
                        var uploadsPath = Path.Combine(_environment.WebRootPath, "profile_pictures");
                        var filePath = Path.Combine(uploadsPath, fileName);
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                }

                // Reset to null (will show default silhouette)
                doctor.ProfilePictureUrl = null;
                _db.Doctors.Update(doctor);
                await _db.SaveChangesAsync();

                return Ok(new { message = "Profile picture deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting file", error = ex.Message });
            }
        }
    }
}