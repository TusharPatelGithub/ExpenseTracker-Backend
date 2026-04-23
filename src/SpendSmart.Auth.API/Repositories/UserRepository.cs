using Microsoft.EntityFrameworkCore;
using SpendSmart.Auth.API.Data;
using SpendSmart.Auth.API.Entities;

namespace SpendSmart.Auth.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> FindByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User?> FindByUserIdAsync(int userId)
            => await _context.Users.FindAsync(userId);

        public async Task<bool> ExistsByEmailAsync(string email)
            => await _context.Users.AnyAsync(u => u.Email == email);

        public async Task<List<User>> FindAllActiveAsync()
            => await _context.Users.Where(u => u.IsActive).ToListAsync();

        public async Task<List<User>> FindAllAsync()
            => await _context.Users.ToListAsync();

        public async Task AddAsync(User user)
            => await _context.Users.AddAsync(user);

        public async Task UpdateLastLoginAsync(int userId, DateTime loginTime)
            => await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.LastLoginAt, loginTime));

        public async Task UpdateCurrencyAsync(int userId, string currency)
            => await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Currency, currency));

        public async Task DeleteByIdAsync(int userId)
            => await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteDeleteAsync();

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}