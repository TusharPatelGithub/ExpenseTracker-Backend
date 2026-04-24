using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Notification.API.DTOs;
using SpendSmart.Notification.API.Services;
using System.Security.Claims;

namespace SpendSmart.Notification.API.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("user")]
        public async Task<IActionResult> GetByUser()
            => Ok(await _notificationService.GetByUserAsync(GetUserId()));

        [HttpGet("unread")]
        public async Task<IActionResult> GetUnread()
            => Ok(await _notificationService.GetUnreadAsync(GetUserId()));

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
            => Ok(new { Count = await _notificationService.GetUnreadCountAsync(GetUserId()) });

        [HttpPut("{id:int}/mark-read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(id, GetUserId());
                return Ok(new { message = "Notification marked as read." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllRead()
        {
            await _notificationService.MarkAllReadAsync(GetUserId());
            return Ok(new { message = "All notifications marked as read." });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                await _notificationService.DeleteNotificationAsync(id, GetUserId());
                return Ok(new { message = "Notification deleted successfully." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPost("send-bulk")]
        public async Task<IActionResult> SendBulk([FromBody] SendBulkDto dto)
        {
            await _notificationService.SendBulkAsync(dto.UserIds, dto.Title, dto.Message, dto.Type);
            return Ok(new { message = $"Notification sent to {dto.UserIds.Count} users." });
        }
    }
}
