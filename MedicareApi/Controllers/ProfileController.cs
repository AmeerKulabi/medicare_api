using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicareApi.Controllers
{
    [ApiController]
    [Route("api/profile")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProfileController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound();

            return Ok(doctor);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateDto updateDto)
        {
            var userId = User.FindFirst("uid")?.Value;
            var isDoctor = User.FindFirst("isDoctor")?.Value == "True";
            if (!isDoctor) return Unauthorized();

            var doctor = await _db.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
            if (doctor == null) return NotFound();

            // Only update allowed fields - restrict to specific properties only
            if (updateDto.Email != null) doctor.Email = updateDto.Email;
            if (updateDto.Phone != null) doctor.Phone = updateDto.Phone;
            if (updateDto.ProfessionalBiography != null) doctor.ProfessionalBiography = updateDto.ProfessionalBiography;
            if (updateDto.Languages != null) doctor.Languages = updateDto.Languages;
            
            // Professional Information fields
            if (updateDto.Specialization != null) doctor.Specialization = updateDto.Specialization;
            if (updateDto.SubSpecialization != null) doctor.SubSpecialization = updateDto.SubSpecialization;
            if (updateDto.BoardCertification != null) doctor.BoardCertification = updateDto.BoardCertification;
            if (updateDto.YearsOfExperience != null) doctor.YearsOfExperience = updateDto.YearsOfExperience;
            
            // Education & Training fields
            if (updateDto.MedicalSchool != null) doctor.MedicalSchool = updateDto.MedicalSchool;
            if (updateDto.GraduationYear != null) doctor.GraduationYear = updateDto.GraduationYear;
            if (updateDto.ResidencyProgram != null) doctor.ResidencyProgram = updateDto.ResidencyProgram;
            if (updateDto.ResidencyHospital != null) doctor.ResidencyHospital = updateDto.ResidencyHospital;
            if (updateDto.FellowshipProgram != null) doctor.FellowshipProgram = updateDto.FellowshipProgram;
            
            // Hospital and clinic information
            if (updateDto.HospitalAffiliations != null) doctor.HospitalAffiliations = updateDto.HospitalAffiliations;
            if (updateDto.ClinicName != null) doctor.ClinicName = updateDto.ClinicName;
            if (updateDto.ClinicAddress != null) doctor.ClinicAddress = updateDto.ClinicAddress;
            if (updateDto.ClinicCity != null) doctor.ClinicCity = updateDto.ClinicCity;
            if (updateDto.ClinicState != null) doctor.ClinicState = updateDto.ClinicState;
            if (updateDto.ClinicZip != null) doctor.ClinicZip = updateDto.ClinicZip;
            if (updateDto.ClinicPhone != null) doctor.ClinicPhone = updateDto.ClinicPhone;
            if (updateDto.PracticeType != null) doctor.PracticeType = updateDto.PracticeType;
            if (updateDto.ConsultationFee != null) doctor.ConsultationFee = updateDto.ConsultationFee;
            
            // Services & Availability
            if (updateDto.ServicesOffered != null) doctor.ServicesOffered = updateDto.ServicesOffered;
            if (updateDto.Availability != null) doctor.Availability = updateDto.Availability;

            await _db.SaveChangesAsync();
            return Ok(doctor);
        }
    }
}