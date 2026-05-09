using Microsoft.EntityFrameworkCore;
using SpendSmart.Auth.API.Data;
using SpendSmart.Auth.API.Entities;
using System.Text.Json;

namespace SpendSmart.Auth.API.Services
{
    public interface IAuditLogService
    {
        Task LogAsync(int actorUserId, string actorEmail, string action,
                      int? targetUserId = null, object? before = null, object? after = null);

        Task<List<AuditLog>> GetAuditLogsPagedAsync(int page = 1, int pageSize = 50);
    }

    /// <summary>
    /// Persists every admin action to the AuditLogs table with actor,
    /// timestamp, and before/after JSON snapshots.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly AuthDbContext _db;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(AuthDbContext db, ILogger<AuditLogService> logger)
        {
            _db     = db;
            _logger = logger;
        }

        public async Task LogAsync(
            int actorUserId, string actorEmail, string action,
            int? targetUserId = null, object? before = null, object? after = null)
        {
            try
            {
                var entry = new AuditLog
                {
                    ActorUserId  = actorUserId,
                    ActorEmail   = actorEmail,
                    Action       = action,
                    TargetUserId = targetUserId,
                    BeforeValue  = before is null ? null : JsonSerializer.Serialize(before),
                    AfterValue   = after  is null ? null : JsonSerializer.Serialize(after),
                    Timestamp    = DateTime.UtcNow
                };

                await _db.AuditLogs.AddAsync(entry);
                await _db.SaveChangesAsync();

                _logger.LogInformation(
                    "AUDIT [{Action}] Actor={Actor} Target={Target} At={At}",
                    action, actorEmail, targetUserId, entry.Timestamp);
            }
            catch (Exception ex)
            {
                // Audit failure must never break the main operation
                _logger.LogError(ex,
                    "AuditLogService: failed to persist audit log for action={Action}.", action);
            }
        }

        public async Task<List<AuditLog>> GetAuditLogsPagedAsync(int page = 1, int pageSize = 50)
        {
            return await _db.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
