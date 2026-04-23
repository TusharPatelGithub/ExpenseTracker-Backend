using System.ComponentModel.DataAnnotations;

namespace SpendSmart.Auth.API.DTOs
{
    /// <summary>
    /// Payload returned by the Google OAuth callback containing the
    /// authenticated user's details extracted from the Google identity.
    /// </summary>
    public class GoogleCallbackResultDto
    {
        public string Token { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsNewUser { get; set; }
    }
}
