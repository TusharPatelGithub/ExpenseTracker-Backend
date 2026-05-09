using Microsoft.EntityFrameworkCore;
using SpendSmart.Income.API.Data;

namespace SpendSmart.Income.API.BackgroundServices
{
    /// <summary>
    /// IHostedService that runs daily at 08:00 UTC.
    /// Checks all recurring income entries whose next occurrence falls within
    /// the next 3 days and sends a RECURRING_REMINDER notification for each
    /// by calling the Notification microservice (POST /api/notifications/send).
    /// RecurrenceType: MONTHLY → next date = Date + 1 month
    ///                 WEEKLY  → next date = Date + 7 days
    ///                 YEARLY  → next date = Date + 1 year
    /// </summary>
    public class RecurringReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RecurringReminderService> _logger;

        private const int ReminderWindowDays = 3;   // remind if due within 3 days

        public RecurringReminderService(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            ILogger<RecurringReminderService> logger)
        {
            _scopeFactory     = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _logger           = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // Schedule next run at 08:00 UTC today (or tomorrow if already past)
                var now     = DateTime.UtcNow;
                var nextRun = new DateTime(now.Year, now.Month, now.Day, 8, 0, 0, DateTimeKind.Utc);
                if (nextRun <= now) nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation(
                    "RecurringReminderService: next run at {NextRun} (in {Hours:F1}h).",
                    nextRun, delay.TotalHours);

                await Task.Delay(delay, stoppingToken);
                if (stoppingToken.IsCancellationRequested) break;

                await SendRemindersAsync(stoppingToken);
            }
        }

        private async Task SendRemindersAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<IncomeDbContext>();

                var today      = DateTime.UtcNow.Date;
                var windowEnd  = today.AddDays(ReminderWindowDays);

                // Load all active recurring incomes
                var recurringIncomes = await db.Incomes
                    .Where(i => i.IsRecurring && i.RecurrenceType != null)
                    .ToListAsync(ct);

                var client = _httpClientFactory.CreateClient("NotificationService");

                foreach (var income in recurringIncomes)
                {
                    var nextDate = ComputeNextOccurrence(income.Date, income.RecurrenceType!);

                    // Only remind if the next occurrence is within the reminder window
                    if (nextDate.Date < today || nextDate.Date > windowEnd) continue;

                    var payload = new
                    {
                        UserId    = income.UserId,
                        Type      = "RECURRING_REMINDER",
                        Title     = $"Upcoming Income: {income.Source}",
                        Message   = $"Your recurring {income.RecurrenceType!.ToLower()} income " +
                                    $"'{income.Source}' of {income.Currency} {income.Amount:N2} " +
                                    $"is due on {nextDate:dd MMM yyyy}.",
                        RelatedId = income.IncomeId
                    };

                    try
                    {
                        var response = await client.PostAsJsonAsync("/api/notifications/send-internal", payload, ct);
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning(
                                "RecurringReminderService: notification API returned {Status} for IncomeId={Id}.",
                                response.StatusCode, income.IncomeId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "RecurringReminderService: failed to send reminder for IncomeId={Id}.", income.IncomeId);
                    }
                }

                _logger.LogInformation(
                    "RecurringReminderService: reminder check complete. {Count} income(s) evaluated.",
                    recurringIncomes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RecurringReminderService: unhandled error during reminder scan.");
            }
        }

        /// <summary>
        /// Computes the next scheduled occurrence after the original entry date.
        /// </summary>
        private static DateTime ComputeNextOccurrence(DateTime originalDate, string recurrenceType)
        {
            var now = DateTime.UtcNow.Date;
            var next = originalDate.Date;

            return recurrenceType.ToUpperInvariant() switch
            {
                "WEEKLY"  => AdvanceUntilFuture(next, d => d.AddDays(7), now),
                "MONTHLY" => AdvanceUntilFuture(next, d => d.AddMonths(1), now),
                "YEARLY"  => AdvanceUntilFuture(next, d => d.AddYears(1), now),
                _         => originalDate
            };
        }

        private static DateTime AdvanceUntilFuture(DateTime start, Func<DateTime, DateTime> advance, DateTime today)
        {
            while (start <= today)
                start = advance(start);
            return start;
        }
    }
}
