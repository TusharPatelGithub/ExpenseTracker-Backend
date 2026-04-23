namespace SpendSmart.Auth.API.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Currency { get; set; } = "INR"; // ISO 4217
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public string Role { get; set; } = "User"; // User / Admin
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}