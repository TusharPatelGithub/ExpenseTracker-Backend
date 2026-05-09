namespace SpendSmart.Auth.API.DTOs
{
    /// <summary>Safe, serializable view of an AuditLog row (no EF navigation properties).</summary>
    public class AuditLogDto
    {
        public int AuditLogId { get; set; }
        public int ActorUserId { get; set; }
        public string ActorEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int? TargetUserId { get; set; }
        public string? BeforeValue { get; set; }
        public string? AfterValue { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
