namespace SpendSmart.Notification.API.DTOs
{
    /// <summary>
    /// DTO used by internal background services (RecurringReminderService, etc.)
    /// to post notifications directly without a user JWT.
    /// </summary>
    public class InternalNotificationDto
    {
        public int UserId { get; set; }
        public string Type { get; set; } = "RECURRING_REMINDER";
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
    }
}
