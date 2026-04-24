namespace SpendSmart.Budget.API.Services
{
    public interface INotificationService
    {
        Task SendBudgetAlertAsync(int userId, int budgetId, string budgetName, decimal utilization, string alertType);
    }
}
