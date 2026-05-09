namespace SpendSmart.Auth.API.DTOs
{
    /// <summary>Payload for POST /api/admin/notifications/broadcast.</summary>
    public class SendBulkDto
    {
        public List<int> UserIds { get; set; } = new();
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "INFO";
    }
}
