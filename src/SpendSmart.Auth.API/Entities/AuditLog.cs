using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Auth.API.Entities
{
    /// <summary>
    /// Persistent audit trail for every admin action (suspend, delete, role change).
    /// Satisfies: "All admin actions are logged to an AuditLog EF Core entity via ILogger<T>
    /// with actor, timestamp, and before/after values."
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        /// <summary>UserId of the admin who performed the action.</summary>
        [Required]
        public int ActorUserId { get; set; }

        /// <summary>Email of the admin for display purposes.</summary>
        [MaxLength(200)]
        public string ActorEmail { get; set; } = string.Empty;

        /// <summary>e.g. SUSPEND_USER, DELETE_USER, ROLE_CHANGE</summary>
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>UserId of the account being acted on (nullable for non-user actions).</summary>
        public int? TargetUserId { get; set; }

        /// <summary>JSON-serialised snapshot of the entity state BEFORE the action.</summary>
        public string? BeforeValue { get; set; }

        /// <summary>JSON-serialised snapshot of the entity state AFTER the action.</summary>
        public string? AfterValue { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
