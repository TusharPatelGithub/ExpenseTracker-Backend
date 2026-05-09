using Microsoft.EntityFrameworkCore;
using SpendSmart.Notification.API.Data;
using SpendSmart.Notification.API.Entities;

namespace SpendSmart.Notification.API.BackgroundServices
{
    /// <summary>
    /// IHostedService that runs on the 1st of every month.
    /// Queries all distinct active UserIds from the notification table
    /// and creates a MONTHLY_SUMMARY notification for each, prompting them
    /// to view their monthly financial summary in the app.
    /// </summary>
    public class MonthlySummaryNotificationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MonthlySummaryNotificationService> _logger;

        public MonthlySummaryNotificationService(
            IServiceScopeFactory scopeFactory,
            ILogger<MonthlySummaryNotificationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;

                // Schedule to fire at 00:01 UTC on the 1st of next month
                var nextRun = new DateTime(now.Year, now.Month, 1, 0, 1, 0, DateTimeKind.Utc)
                    .AddMonths(1);

                var delay = nextRun - now;
                _logger.LogInformation(
                    "MonthlySummaryNotificationService: next run at {NextRun} (in {Hours:F1}h).",
                    nextRun, delay.TotalHours);

                await Task.Delay(delay, stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;

                await SendMonthlySummaryNotificationsAsync(stoppingToken);
            }
        }

        private async Task SendMonthlySummaryNotificationsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();

                // Derive the month that just ended
                var lastMonth = DateTime.UtcNow.AddMonths(-1);
                var monthName = lastMonth.ToString("MMMM yyyy");

                // Get all distinct user IDs that have ever received a notification
                // (used as a proxy for registered/active users in this service)
                var userIds = await db.Notifications
                    .Select(n => n.UserId)
                    .Distinct()
                    .ToListAsync(ct);

                if (!userIds.Any())
                {
                    _logger.LogInformation(
                        "MonthlySummaryNotificationService: no users found, skipping.");
                    return;
                }

                var notifications = userIds.Select(uid => new NotificationEntity
                {
                    UserId    = uid,
                    Type      = "MONTHLY_SUMMARY",
                    Title     = $"Your {monthName} Summary is Ready",
                    Message   = $"Your financial summary for {monthName} is now available. " +
                                 "Open the app to review your income, expenses, and savings rate.",
                    RelatedId = null,
                    IsRead    = false,
                    SentAt    = DateTime.UtcNow
                }).ToList();

                await db.Notifications.AddRangeAsync(notifications, ct);
                await db.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "MonthlySummaryNotificationService: sent {Count} MONTHLY_SUMMARY notifications for {Month}.",
                    notifications.Count, monthName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "MonthlySummaryNotificationService: failed to send monthly summary notifications.");
            }
        }
    }
}
