using MailKit.Net.Smtp;
using MimeKit;
using SpendSmart.Notification.API.DTOs;
using SpendSmart.Notification.API.Entities;
using SpendSmart.Notification.API.Repositories;

namespace SpendSmart.Notification.API.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IConfiguration configuration,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendAsync(NotificationEntity notification)
        {
            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task SendBudgetAlertAsync(int userId, string alertType, decimal spentPercent, int budgetId, string budgetName)
        {
            var title = alertType == "LIMIT_REACHED"
                ? $"Budget Exceeded: {budgetName}"
                : $"Budget Warning: {budgetName}";

            var message = alertType == "LIMIT_REACHED"
                ? $"You have exceeded your budget '{budgetName}'. Spent: {spentPercent:F1}% of limit."
                : $"You have used {spentPercent:F1}% of your budget '{budgetName}'. Consider reducing spending.";

            var notification = new NotificationEntity
            {
                UserId = userId,
                Type = alertType == "LIMIT_REACHED" ? "BUDGET_EXCEEDED" : "BUDGET_WARNING",
                Title = title,
                Message = message,
                RelatedId = budgetId,
                IsRead = false
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            if (alertType == "LIMIT_REACHED")
            {
                var emailConfig = _configuration.GetSection("EmailSettings");
                var adminEmail = emailConfig["AdminEmail"];
                if (!string.IsNullOrEmpty(adminEmail))
                {
                    try
                    {
                        await SendEmailAsync(adminEmail, title, message);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Email send failed: {Error}", ex.Message);
                    }
                }
            }
        }

        public async Task SendBulkAsync(List<int> userIds, string title, string message, string type)
        {
            var notifications = userIds.Select(uid => new NotificationEntity
            {
                UserId = uid,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false
            }).ToList();

            await _notificationRepository.AddRangeAsync(notifications);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<List<NotificationResponseDto>> GetByUserAsync(int userId)
        {
            var notifications = await _notificationRepository.FindByUserIdAsync(userId);
            return notifications.Select(MapToDto).ToList();
        }

        public async Task<List<NotificationResponseDto>> GetUnreadAsync(int userId)
        {
            var notifications = await _notificationRepository.FindUnreadByUserIdAsync(userId);
            return notifications.Select(MapToDto).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
            => await _notificationRepository.CountUnreadByUserIdAsync(userId);

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.FindByNotificationIdAsync(notificationId)
                ?? throw new KeyNotFoundException("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized.");

            notification.IsRead = true;
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task MarkAllReadAsync(int userId)
            => await _notificationRepository.MarkAllReadAsync(userId);

        public async Task DeleteNotificationAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.FindByNotificationIdAsync(notificationId)
                ?? throw new KeyNotFoundException("Notification not found.");

            if (notification.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized.");

            await _notificationRepository.DeleteByNotificationIdAsync(notificationId);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var emailConfig = _configuration.GetSection("EmailSettings");

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailConfig["FromEmail"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart("plain") { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(emailConfig["SmtpHost"], int.Parse(emailConfig["SmtpPort"]!), false);
            await smtp.AuthenticateAsync(emailConfig["SmtpUser"], emailConfig["SmtpPass"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}", toEmail);
        }

        private static NotificationResponseDto MapToDto(NotificationEntity n) => new()
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            RelatedId = n.RelatedId,
            IsRead = n.IsRead,
            SentAt = n.SentAt
        };
    }
}
