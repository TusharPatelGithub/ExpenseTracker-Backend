using SpendSmart.Notification.API.DTOs;
using SpendSmart.Notification.API.Entities;

namespace SpendSmart.Notification.API.Services
{
    public interface INotificationService
    {
        Task SendAsync(NotificationEntity notification);
        Task SendBudgetAlertAsync(int userId, string alertType, decimal spentPercent, int budgetId, string budgetName);
        Task SendBulkAsync(List<int> userIds, string title, string message, string type);
        Task<List<NotificationResponseDto>> GetByUserAsync(int userId);
        Task<List<NotificationResponseDto>> GetUnreadAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task MarkAsReadAsync(int notificationId, int userId);
        Task MarkAllReadAsync(int userId);
        Task DeleteNotificationAsync(int notificationId, int userId);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
