using SpendSmart.Notification.API.Entities;

namespace SpendSmart.Notification.API.Repositories
{
    public interface INotificationRepository
    {
        Task<List<NotificationEntity>> FindByUserIdAsync(int userId);
        Task<List<NotificationEntity>> FindUnreadByUserIdAsync(int userId);
        Task<int> CountUnreadByUserIdAsync(int userId);
        Task<List<NotificationEntity>> FindByTypeAsync(int userId, string type);
        Task MarkAllReadAsync(int userId);
        Task<NotificationEntity?> FindByNotificationIdAsync(int notificationId);
        Task AddAsync(NotificationEntity notification);
        Task AddRangeAsync(List<NotificationEntity> notifications);
        Task DeleteByNotificationIdAsync(int notificationId);
        Task SaveChangesAsync();
    }
}
