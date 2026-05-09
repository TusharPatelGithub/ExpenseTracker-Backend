using System.Text;
using System.Text.Json;

namespace SpendSmart.Budget.API.Services
{
    /// <summary>
    /// Replaces the logger-only stub. Now makes a real HTTP call to the
    /// Notification microservice (POST /api/notifications/send-internal)
    /// so budget alerts are persisted and (for BUDGET_EXCEEDED) trigger emails.
    /// Falls back to a warning log if the Notification service is unreachable
    /// so budget processing is never blocked.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHttpClientFactory httpClientFactory,
            ILogger<NotificationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger            = logger;
        }

        public async Task SendBudgetAlertAsync(
            int userId, int budgetId, string budgetName,
            decimal utilization, string alertType)
        {
            // Map Budget alert types to Notification entity types
            var notificationType = alertType == "LIMIT_REACHED"
                ? "BUDGET_EXCEEDED"
                : "BUDGET_WARNING";

            var title = alertType == "LIMIT_REACHED"
                ? $"Budget Exceeded: {budgetName}"
                : $"Budget Warning: {budgetName}";

            var message = alertType == "LIMIT_REACHED"
                ? $"You have exceeded your budget '{budgetName}'. Spent: {utilization:F1}% of limit."
                : $"You have used {utilization:F1}% of your budget '{budgetName}'. Consider reducing spending.";

            var payload = new
            {
                UserId    = userId,
                Type      = notificationType,
                Title     = title,
                Message   = message,
                RelatedId = budgetId
            };

            try
            {
                var client  = _httpClientFactory.CreateClient("NotificationService");
                var json    = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("/api/notifications/send-internal", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation(
                        "Budget alert [{AlertType}] sent to Notification service for UserId={UserId} BudgetId={BudgetId}.",
                        alertType, userId, budgetId);
                }
                else
                {
                    _logger.LogWarning(
                        "Notification service returned {Status} for budget alert UserId={UserId} BudgetId={BudgetId}.",
                        response.StatusCode, userId, budgetId);
                }
            }
            catch (Exception ex)
            {
                // Log and swallow — budget check must not fail because Notification service is down
                _logger.LogError(ex,
                    "Failed to send budget alert to Notification service for UserId={UserId} BudgetId={BudgetId}. " +
                    "Alert type: {AlertType}.", userId, budgetId, alertType);
            }
        }
    }
}
