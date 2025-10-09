using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicareApi.BackgroundServices
{
    /// <summary>
    /// Background service that sends daily metrics at midnight (00:00).
    /// </summary>
    public class DailyMetricsService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DailyMetricsService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="serviceProvider">Service provider for scoped services</param>
        /// <param name="logger">Logger</param>
        public DailyMetricsService(IServiceProvider serviceProvider, ILogger<DailyMetricsService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Executes the background task.
        /// </summary>
        /// <param name="stoppingToken">Cancellation token</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Daily Metrics Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calculate time until next midnight
                    var now = DateTime.UtcNow;
                    var nextMidnight = now.Date.AddDays(1);
                    var delay = nextMidnight - now;

                    _logger.LogInformation("Next daily metrics will be sent at {NextMidnight} UTC (in {Delay})", nextMidnight, delay);

                    // Wait until midnight
                    await Task.Delay(delay, stoppingToken);

                    // Send daily metrics
                    await SendDailyMetrics(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Service is stopping
                    _logger.LogInformation("Daily Metrics Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Daily Metrics Service");
                    // Wait a bit before retrying to avoid tight loop on persistent errors
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }

            _logger.LogInformation("Daily Metrics Service stopped");
        }

        /// <summary>
        /// Sends daily metrics to analytics.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        private async Task SendDailyMetrics(CancellationToken cancellationToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Count doctors and patients
                var doctorCount = await dbContext.Doctors.CountAsync(cancellationToken);
                var allUsers = await userManager.Users.ToListAsync(cancellationToken);
                var patientCount = allUsers.Count(u => !u.IsDoctor);

                // Track metrics
                analyticsService.TrackDailyMetrics(doctorCount, patientCount);

                _logger.LogInformation("Daily metrics sent successfully: Doctors={DoctorCount}, Patients={PatientCount}", doctorCount, patientCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send daily metrics");
            }
        }
    }
}
