using SpendSmart.Auth.API.Entities;

namespace SpendSmart.Auth.API.Repositories
{
    public interface IUserRepository
    {
        Task<User?> FindByEmailAsync(string email);
        Task<User?> FindByUserIdAsync(int userId);
        Task<bool> ExistsByEmailAsync(string email);
        Task<List<User>> FindAllActiveAsync();
        /// <summary>Returns ALL users regardless of IsActive — for admin use.</summary>
        Task<List<User>> FindAllAsync();
        Task AddAsync(User user);
        Task UpdateLastLoginAsync(int userId, DateTime loginTime);
        Task UpdateCurrencyAsync(int userId, string currency);
        /// <summary>Permanently removes the user row from the database.</summary>
        Task DeleteByIdAsync(int userId);
        Task SaveChangesAsync();
    }
}