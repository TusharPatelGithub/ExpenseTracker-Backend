using SpendSmart.Auth.API.DTOs;
using SpendSmart.Auth.API.Entities;

namespace SpendSmart.Auth.API.Services
{
    public interface IUserService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task LogoutAsync(string token);
        Task<User?> GetUserByIdAsync(int userId);
        Task UpdateProfileAsync(int userId, UpdateProfileDto dto);
        Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
        Task UpdateCurrencyAsync(int userId, string currency);
        /// <summary>Self-service — user deactivates their own account.</summary>
        Task DeactivateAccountAsync(int userId);
        /// <summary>Admin — suspend any user account (IsActive = false).</summary>
        Task SuspendAccountAsync(int targetUserId);
        /// <summary>Admin — permanently delete any user account.</summary>
        Task DeleteAccountAsync(int targetUserId);
        /// <summary>Admin — returns all users including suspended ones.</summary>
        Task<List<User>> GetAllUsersAsync();

    }
}