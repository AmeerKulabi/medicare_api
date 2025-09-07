using MedicareApi.Data;
using MedicareApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace MedicareApi.Tests
{
    public class TestFixture : WebApplicationFactory<Program>
    {
        public ApplicationDbContext DbContext { get; private set; }
        public UserManager<ApplicationUser> UserManager { get; private set; }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the app's ApplicationDbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add ApplicationDbContext using an in-memory database for testing
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context and user manager
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<ApplicationDbContext>();
                    var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();
                    var logger = scopedServices.GetRequiredService<ILogger<TestFixture>>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    try
                    {
                        // Seed the database with test data
                        SeedDatabase(db, userManager).Wait();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the database with test data.");
                    }

                    DbContext = db;
                    UserManager = userManager;
                }
            });
        }

        private async Task SeedDatabase(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Create test users
            var doctorUser = new ApplicationUser
            {
                Id = "doctor-user-id",
                UserName = "doctor@test.com",
                Email = "doctor@test.com",
                FullName = "Dr. Test Doctor",
                IsDoctor = true,
                Phone = "+96470123456789",
                EmailConfirmed = true
            };

            var patientUser = new ApplicationUser
            {
                Id = "patient-user-id", 
                UserName = "patient@test.com",
                Email = "patient@test.com",
                FullName = "Test Patient",
                IsDoctor = false,
                Phone = "+96470987654321",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(doctorUser, "TestPass123!");
            await userManager.CreateAsync(patientUser, "TestPass123!");

            // Create test doctor
            var doctor = new Doctor
            {
                Id = "test-doctor-id",
                UserId = doctorUser.Id,
                Name = "Dr. Test Doctor",
                Email = "doctor@test.com",
                IsActive = true,
                RegistrationCompleted = true,
                Specialization = "General Medicine",
                Location = "Baghdad",
                ClinicName = "Test Clinic",
                ConsultationFee = "50"
            };

            context.Doctors.Add(doctor);

            // Create test availability slots
            var slot1 = new AvailabilitySlot
            {
                Id = "slot-1",
                DoctorId = doctor.Id,
                day = "الأحد",
                Start = "8.00",
                End = "16.00",
                IsAvailable = true
            };

            var slot2 = new AvailabilitySlot
            {
                Id = "slot-2",
                DoctorId = doctor.Id,
                day = "الإثنين",
                Start = "8.00",
                End = "16.00",
                IsAvailable = false
            };

            context.AvailabilitySlots.AddRange(slot1, slot2);

            // Create test appointments
            var appointment = new Appointment
            {
                Id = "test-appointment-id",
                PatientId = patientUser.Id,
                DoctorId = doctor.Id,
                Status = "confirmed",
                ScheduledAt = DateTime.Now.AddDays(1),
                Reason = "Test appointment"
            };

            context.Appointments.Add(appointment);

            await context.SaveChangesAsync();
        }

        public string GenerateJwtToken(string userId, bool isDoctor, string? email = null)
        {
            var claims = new List<Claim>
            {
                new Claim("uid", userId),
                new Claim("isDoctor", isDoctor.ToString())
            };

            if (!string.IsNullOrEmpty(email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourTestJwtSecretKeyForTestingPurposesOnly123!"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "medicare.app",
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DbContext?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}