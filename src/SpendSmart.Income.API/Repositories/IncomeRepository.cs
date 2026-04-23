using Microsoft.EntityFrameworkCore;
using SpendSmart.Income.API.Data;
using SpendSmart.Income.API.Entities;

namespace SpendSmart.Income.API.Repositories
{
    public class IncomeRepository : IIncomeRepository
    {
        private readonly IncomeDbContext _context;

        public IncomeRepository(IncomeDbContext context)
        {
            _context = context;
        }

        public async Task<IncomeEntity?> FindByIncomeIdAsync(int incomeId)
            => await _context.Incomes.FindAsync(incomeId);

        public async Task<List<IncomeEntity>> FindByUserIdAsync(int userId)
            => await _context.Incomes.Where(e => e.UserId == userId).ToListAsync();

        public async Task<List<IncomeEntity>> FindBySourceAsync(int userId, string source)
            => await _context.Incomes.Where(e => e.UserId == userId && e.Source == source).ToListAsync();

        public async Task<List<IncomeEntity>> FindByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
            => await _context.Incomes
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

        public async Task<decimal> SumByUserIdAsync(int userId)
            => await _context.Incomes.Where(e => e.UserId == userId).SumAsync(e => e.Amount);

        public async Task<decimal> SumBySourceAsync(int userId, string source)
            => await _context.Incomes
                .Where(e => e.UserId == userId && e.Source == source)
                .SumAsync(e => e.Amount);

        public async Task<List<IncomeEntity>> FindRecurringAsync(int userId)
            => await _context.Incomes.Where(e => e.UserId == userId && e.IsRecurring).ToListAsync();

        public async Task AddAsync(IncomeEntity income)
            => await _context.Incomes.AddAsync(income);

        public async Task UpdateAsync(IncomeEntity income)
        {
            income.UpdatedAt = DateTime.UtcNow;
            _context.Incomes.Update(income);
            await Task.CompletedTask;
        }

        public async Task DeleteByIncomeIdAsync(int incomeId)
            => await _context.Incomes.Where(e => e.IncomeId == incomeId).ExecuteDeleteAsync();

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
