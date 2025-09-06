using MedicareApi.Data;
using MedicareApi.Models;
using MedicareApi.Services;
using Microsoft.EntityFrameworkCore;

namespace MedicareApi.Services
{
    public class AppointmentStatusService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentStatusService> _logger;

        public AppointmentStatusService(IServiceProvider serviceProvider, ILogger<AppointmentStatusService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessAppointmentStatusUpdates();
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken); // Check every 10 minutes
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in appointment status service");
                }
            }
        }

        private async Task ProcessAppointmentStatusUpdates()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var paymentService = scope.ServiceProvider.GetRequiredService<IPaymentService>();

            var now = DateTime.UtcNow;
            
            // Find appointments that should be confirmed (24 hours before)
            var appointmentsToConfirm = await dbContext.Appointments
                .Where(a => a.Status == AppointmentStatus.Booked && 
                           a.ScheduledAt <= now.AddHours(24) &&
                           a.ScheduledAt > now)
                .ToListAsync();

            foreach (var appointment in appointmentsToConfirm)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.ConfirmedAt = now;
                _logger.LogInformation($"Confirmed appointment {appointment.Id}");
            }

            // Find appointments that should be marked as done (past scheduled time)
            var appointmentsToComplete = await dbContext.Appointments
                .Where(a => (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Booked) && 
                           a.ScheduledAt < now)
                .ToListAsync();

            foreach (var appointment in appointmentsToComplete)
            {
                appointment.Status = AppointmentStatus.Done;
                appointment.CompletedAt = now;
                
                // Process payment transfer when appointment is completed
                if (!string.IsNullOrEmpty(appointment.PaymentId))
                {
                    await paymentService.CompletePaymentTransferAsync(appointment.PaymentId);
                }
                
                _logger.LogInformation($"Completed appointment {appointment.Id}");
            }

            if (appointmentsToConfirm.Count > 0 || appointmentsToComplete.Count > 0)
            {
                await dbContext.SaveChangesAsync();
            }
        }
    }
}