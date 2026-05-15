using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart.Auth.API.DTOs;
using SpendSmart.Auth.API.Services;
using System.Security.Claims;

namespace SpendSmart.Auth.API.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenBlacklistService _tokenBlacklist;
        private readonly IAuditLogService _auditLog;
        private readonly IConfiguration _configuration;

        public UserController(
            IUserService userService,
            ITokenBlacklistService tokenBlacklist,
            IAuditLogService auditLog,
            IConfiguration configuration)
        {
            _userService    = userService;
            _tokenBlacklist = tokenBlacklist;
            _auditLog       = auditLog;
            _configuration  = configuration;
        }

        // ─── Registration & Login ─────────────────────────────────────────────────

        /// <summary>POST api/auth/register — register with full name, email, password, and currency.</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var response = await _userService.RegisterAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>POST api/auth/login — email + password login, returns JWT.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var response = await _userService.LoginAsync(dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>POST api/auth/logout — revokes the current JWT (adds to blacklist).</summary>
        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Extract the raw token from the Authorization header: "Bearer <token>"
            var rawHeader = Request.Headers.Authorization.ToString();
            if (rawHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = rawHeader["Bearer ".Length..].Trim();
                _tokenBlacklist.RevokeToken(token);
            }

            return Ok(new { message = "Logged out successfully." });
        }

        // ─── Google OAuth ─────────────────────────────────────────────────────────

        /// <summary>
        /// GET api/auth/google-login
        /// Redirects the browser to Google's consent screen.
        /// The frontend should navigate the user to this URL.
        /// </summary>
        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(GoogleCallback))
            };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// GET api/auth/google-callback
        /// Google redirects here after the user consents.
        /// Finds or creates the user account, then returns a JWT.
        /// </summary>
        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            try
            {
                var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                if (result?.Principal == null)
                {
                    var fe = _configuration["GoogleOAuth:FrontendUrl"] ?? "http://localhost:5173";
                    return Redirect($"{fe}/login?error=Google+authentication+failed");
                }

                var email     = result.Principal.FindFirstValue(ClaimTypes.Email)!;
                var fullName  = result.Principal.FindFirstValue(ClaimTypes.Name) ?? email;
                var avatarUrl = result.Principal.FindFirstValue("urn:google:picture") ??
                                result.Principal.FindFirstValue("picture") ?? string.Empty;

                var response = await _userService.HandleGoogleLoginAsync(email, fullName, avatarUrl);

                // Redirect the browser back to React with the JWT token
                var frontendUrl = _configuration["GoogleOAuth:FrontendUrl"] ?? "http://localhost:5173";
                var redirectUrl = $"{frontendUrl}/auth/google/callback?token={Uri.EscapeDataString(response.Token)}";
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                var frontendUrl = _configuration["GoogleOAuth:FrontendUrl"] ?? "http://localhost:5173";
                return Redirect($"{frontendUrl}/login?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        // ─── Profile ──────────────────────────────────────────────────────────────

        /// <summary>GET api/auth/profile — returns the logged-in user's profile.</summary>
        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var user   = await _userService.GetUserByIdAsync(userId);
                if (user == null) return NotFound();

                return Ok(new UserProfileDto
                {
                    UserId      = user.UserId,
                    FullName    = user.FullName,
                    Email       = user.Email,
                    Currency    = user.Currency,
                    AvatarUrl   = user.AvatarUrl,
                    Role        = user.Role,
                    CreatedAt   = user.CreatedAt,
                    LastLoginAt = user.LastLoginAt
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>PUT api/auth/profile — update full name and avatar URL.</summary>
        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userService.UpdateProfileAsync(userId, dto);
                return Ok(new { message = "Profile updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>PUT api/auth/change-password — change the logged-in user's password.</summary>
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userService.ChangePasswordAsync(userId, dto);
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>PUT api/auth/currency — update preferred currency (ISO 4217).</summary>
        [Authorize]
        [HttpPut("currency")]
        public async Task<IActionResult> UpdateCurrency([FromBody] UpdateCurrencyDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userService.UpdateCurrencyAsync(userId, dto.Currency);
                return Ok(new { message = "Currency updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─── Self-service Account Management ──────────────────────────────────────

        /// <summary>DELETE api/auth/deactivate — user deactivates their own account.</summary>
        [Authorize]
        [HttpDelete("deactivate")]
        public async Task<IActionResult> DeactivateAccount()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _userService.DeactivateAccountAsync(userId);
                return Ok(new { message = "Account deactivated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─── Admin Endpoints ──────────────────────────────────────────────────────

        /// <summary>GET api/auth/admin/users — admin: list ALL users (including suspended).</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>PUT api/auth/admin/suspend/{userId} — admin: suspend a user account.</summary>
        [Authorize(Roles = "Admin")]
        [HttpPut("admin/suspend/{userId:int}")]
        public async Task<IActionResult> SuspendAccount(int userId)
        {
            try
            {
                var actorId    = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var actorEmail = User.FindFirstValue(ClaimTypes.Email) ?? "admin";

                var target = await _userService.GetUserByIdAsync(userId);
                var before = new { target?.IsActive };

                await _userService.SuspendAccountAsync(userId);

                await _auditLog.LogAsync(actorId, actorEmail, "SUSPEND_USER",
                    targetUserId: userId,
                    before: before,
                    after: new { IsActive = false });

                return Ok(new { message = $"User {userId} has been suspended." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>DELETE api/auth/admin/delete/{userId} — admin: permanently delete a user account.</summary>
        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/delete/{userId:int}")]
        public async Task<IActionResult> DeleteAccount(int userId)
        {
            try
            {
                var actorId    = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var actorEmail = User.FindFirstValue(ClaimTypes.Email) ?? "admin";

                var target = await _userService.GetUserByIdAsync(userId);
                var before = new { target?.UserId, target?.Email, target?.FullName, target?.Role };

                await _userService.DeleteAccountAsync(userId);

                await _auditLog.LogAsync(actorId, actorEmail, "DELETE_USER",
                    targetUserId: userId,
                    before: before,
                    after: new { Deleted = true });

                return Ok(new { message = $"User {userId} has been permanently deleted." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>GET api/users/admin/audit-logs — admin: view all audit logs.</summary>
        [Authorize(Roles = "Admin")]
        [HttpGet("admin/audit-logs")]
        public async Task<IActionResult> GetAuditLogs(
            [FromServices] SpendSmart.Auth.API.Data.AuthDbContext db)
        {
            var logs = await db.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
            return Ok(logs);
        }
    }
}