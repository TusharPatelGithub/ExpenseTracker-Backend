using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Auth.API.Clients;
using SpendSmart.Auth.API.DTOs;
using SpendSmart.Auth.API.Repositories;
using SpendSmart.Auth.API.Services;
using System.Security.Claims;

namespace SpendSmart.Auth.API.Controllers
{
    /// <summary>
    /// Admin-only endpoints living inside Auth.API so they can directly access
    /// the User entity, AuditLog entity, and IUserRepository without an extra hop.
    /// All endpoints require the caller to carry an [Authorize(Roles="Admin")] JWT.
    /// Cross-service data (expenses / incomes / notifications) is fetched via typed
    /// HTTP clients that forward the admin's Bearer token.
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository         _userRepo;
        private readonly IUserService            _userService;
        private readonly IAuditLogService        _auditLog;
        private readonly IExpenseServiceClient   _expenseClient;
        private readonly IIncomeServiceClient    _incomeClient;
        private readonly INotificationServiceClient _notificationClient;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IUserRepository userRepo,
            IUserService userService,
            IAuditLogService auditLog,
            IExpenseServiceClient expenseClient,
            IIncomeServiceClient incomeClient,
            INotificationServiceClient notificationClient,
            ILogger<AdminController> logger)
        {
            _userRepo           = userRepo;
            _userService        = userService;
            _auditLog           = auditLog;
            _expenseClient      = expenseClient;
            _incomeClient       = incomeClient;
            _notificationClient = notificationClient;
            _logger             = logger;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        private int    ActorId    => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private string ActorEmail => User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

        /// <summary>Extracts the raw Bearer token from the Authorization header to forward it.</summary>
        private string BearerToken()
        {
            var header = Request.Headers.Authorization.ToString();
            return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? header["Bearer ".Length..].Trim()
                : string.Empty;
        }

        // ─── 1. Manage All Users ──────────────────────────────────────────────────

        /// <summary>GET /api/admin/users — returns all users (including suspended).</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userRepo.FindAllAsync();

            var dtos = users.Select(u => new UserProfileDto
            {
                UserId      = u.UserId,
                FullName    = u.FullName,
                Email       = u.Email,
                Currency    = u.Currency,
                AvatarUrl   = u.AvatarUrl,
                Role        = u.Role,
                CreatedAt   = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                // Include suspension status as an extra field via property reuse:
                // We rely on IsActive — the front-end checks u.isActive, not u.isSuspended
            }).ToList();

            return Ok(dtos);
        }

        // ─── 2. Suspend User Account ──────────────────────────────────────────────

        /// <summary>PUT /api/admin/users/{id}/suspend — sets IsActive = false.</summary>
        [HttpPut("users/{id:int}/suspend")]
        public async Task<IActionResult> SuspendUser(int id)
        {
            var target = await _userRepo.FindByUserIdAsync(id);
            if (target is null)
                return NotFound(new { message = $"User {id} not found." });

            if (!target.IsActive)
                return Conflict(new { message = "User is already suspended." });

            // Snapshot before state
            var before = new { target.UserId, target.IsActive };

            // Suspend via ExecuteUpdateAsync
            await _userRepo.SuspendUserAsync(id);

            // Snapshot after state
            var after = new { UserId = id, IsActive = false };

            // Audit log
            await _auditLog.LogAsync(
                actorUserId  : ActorId,
                actorEmail   : ActorEmail,
                action       : "SUSPEND_USER",
                targetUserId : id,
                before       : before,
                after        : after);

            return Ok(new { message = $"User {id} has been suspended." });
        }

        // ─── 3. Delete User Account ───────────────────────────────────────────────

        /// <summary>DELETE /api/admin/users/{id} — permanently removes the user row.</summary>
        [HttpDelete("users/{id:int}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var target = await _userRepo.FindByUserIdAsync(id);
            if (target is null)
                return NotFound(new { message = $"User {id} not found." });

            // Snapshot before deletion
            var before = new
            {
                target.UserId,
                target.Email,
                target.FullName,
                target.Role,
                target.IsActive
            };

            // Write audit log BEFORE deletion so TargetId still exists
            await _auditLog.LogAsync(
                actorUserId  : ActorId,
                actorEmail   : ActorEmail,
                action       : "DELETE_USER",
                targetUserId : id,
                before       : before,
                after        : null);

            // Hard delete
            await _userRepo.DeleteByIdAsync(id);

            return Ok(new { message = $"User {id} has been permanently deleted." });
        }

        // ─── 4. Platform Analytics ────────────────────────────────────────────────

        /// <summary>GET /api/admin/analytics — aggregate platform stats.</summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var token = BearerToken();

            var usersTask      = _userRepo.FindAllAsync();
            var expTotalTask   = _expenseClient.GetPlatformTotalAsync(token);
            var incTotalTask   = _incomeClient.GetPlatformTotalAsync(token);
            var topCatsTask    = _expenseClient.GetTopCategoriesAsync(token);

            await Task.WhenAll(usersTask, expTotalTask, incTotalTask, topCatsTask);

            var analytics = new PlatformAnalyticsDto
            {
                TotalUsers             = usersTask.Result.Count,
                TotalExpenses          = expTotalTask.Result,
                TotalIncome            = incTotalTask.Result,
                TopSpendingCategories  = topCatsTask.Result
            };

            return Ok(analytics);
        }

        // ─── 5. View Audit Logs ───────────────────────────────────────────────────

        /// <summary>GET /api/admin/audit-logs?page=1&pageSize=50 — paginated audit trail.</summary>
        [HttpGet("audit-logs")]
        public async Task<IActionResult> GetAuditLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
        {
            var logs = await _auditLog.GetAuditLogsPagedAsync(page, pageSize);

            var dtos = logs.Select(l => new AuditLogDto
            {
                AuditLogId   = l.AuditLogId,
                ActorUserId  = l.ActorUserId,
                ActorEmail   = l.ActorEmail,
                Action       = l.Action,
                TargetUserId = l.TargetUserId,
                BeforeValue  = l.BeforeValue,
                AfterValue   = l.AfterValue,
                Timestamp    = l.Timestamp
            }).ToList();

            return Ok(dtos);
        }

        // ─── 6. Broadcast Notification ────────────────────────────────────────────

        /// <summary>POST /api/admin/notifications/broadcast — sends bulk notification via Notification service.</summary>
        [HttpPost("notifications/broadcast")]
        public async Task<IActionResult> BroadcastNotification([FromBody] SendBulkDto dto)
        {
            try
            {
                await _notificationClient.BroadcastAsync(dto, BearerToken());

                await _auditLog.LogAsync(
                    actorUserId : ActorId,
                    actorEmail  : ActorEmail,
                    action      : "BROADCAST_NOTIFICATION",
                    targetUserId: null,
                    before      : null,
                    after       : new { dto.UserIds, dto.Title, dto.Message, dto.Type });

                return Ok(new { message = $"Broadcast sent to {dto.UserIds.Count} users." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AdminController: broadcast notification failed.");
                return StatusCode(502, new { message = "Notification service unavailable.", detail = ex.Message });
            }
        }
    }
}
