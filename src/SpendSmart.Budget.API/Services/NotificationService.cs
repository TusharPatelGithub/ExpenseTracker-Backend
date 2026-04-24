namespace SpendSmart.Budget.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendBudgetAlertAsync(int userId, int budgetId, string budgetName, decimal utilization, string alertType)
        {
            _logger.LogWarning("BUDGET ALERT [{AlertType}] UserId={UserId} BudgetId={BudgetId} Name={Name} Utilization={Utilization}%",
                alertType, userId, budgetId, budgetName, Math.Round(utilization, 2));
            return Task.CompletedTask;
        }
    }
}
