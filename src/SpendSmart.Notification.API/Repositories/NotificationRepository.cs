using Microsoft.EntityFrameworkCore;
using SpendSmart.Notification.API.Data;
using SpendSmart.Notification.API.Entities;

namespace SpendSmart.Notification.API.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _context;

        public NotificationRepository(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<List<NotificationEntity>> FindByUserIdAsync(int userId)
            => await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

        public async Task<List<NotificationEntity>> FindUnreadByUserIdAsync(int userId)
            => await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

        public async Task<int> CountUnreadByUserIdAsync(int userId)
            => await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

        public async Task<List<NotificationEntity>> FindByTypeAsync(int userId, string type)
            => await _context.Notifications
                .Where(n => n.UserId == userId && n.Type == type)
                .OrderByDescending(n => n.SentAt)
                .ToListAsync();

        public async Task MarkAllReadAsync(int userId)
            => await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(n => n.SetProperty(p => p.IsRead, true));

        public async Task<NotificationEntity?> FindByNotificationIdAsync(int notificationId)
            => await _context.Notifications.FindAsync(notificationId);

        public async Task AddAsync(NotificationEntity notification)
            => await _context.Notifications.AddAsync(notification);

        public async Task AddRangeAsync(List<NotificationEntity> notifications)
            => await _context.Notifications.AddRangeAsync(notifications);

        public async Task DeleteByNotificationIdAsync(int notificationId)
            => await _context.Notifications
                .Where(n => n.NotificationId == notificationId)
                .ExecuteDeleteAsync();

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
