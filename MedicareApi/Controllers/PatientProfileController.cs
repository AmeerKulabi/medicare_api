using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/patient/profile")]
    [Authorize]
    public class PatientProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PatientProfileController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePatientProfile([FromBody] ProfileUpdateDto updateDto)
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            
            // Only patients (non-doctors) can use this endpoint
            if (isDoctor || string.IsNullOrEmpty(userId)) 
                return Unauthorized("This endpoint is for patients only.");

            // Get the user from ApplicationUser table
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) 
                return NotFound("User not found.");

            // Update phone number if provided
            if (updateDto.Phone != null)
            {
                user.Phone = updateDto.Phone;
            }

            try
            {
                await _db.SaveChangesAsync();
                
                return Ok(new { 
                    message = "Profile updated successfully",
                    phone = user.Phone 
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating profile: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPatientProfile()
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            
            // Only patients (non-doctors) can use this endpoint
            if (isDoctor || string.IsNullOrEmpty(userId)) 
                return Unauthorized("This endpoint is for patients only.");

            // Get the user from ApplicationUser table
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) 
                return NotFound("User not found.");

            return Ok(new { 
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phone = user.Phone 
            });
        }
    }
}