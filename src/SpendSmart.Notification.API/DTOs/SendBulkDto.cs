using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Notification.API.DTOs
{
    public class SendBulkDto
    {
        [Required]
        public List<int> UserIds { get; set; } = new();

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string Type { get; set; } = "PLATFORM";
    }
}
