namespace SpendSmart.Notification.API.DTOs
{
    public class NotificationResponseDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int? RelatedId { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }
}
