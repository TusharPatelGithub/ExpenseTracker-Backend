using Microsoft.EntityFrameworkCore;
using SpendSmart.Budget.API.Data;
using SpendSmart.Budget.API.Entities;

namespace SpendSmart.Budget.API.Repositories
{
    public class BudgetRepository : IBudgetRepository
    {
        private readonly BudgetDbContext _context;

        public BudgetRepository(BudgetDbContext context)
        {
            _context = context;
        }

        public async Task<BudgetEntity?> FindByBudgetIdAsync(int budgetId)
            => await _context.Budgets.FindAsync(budgetId);

        public async Task<List<BudgetEntity>> FindByUserIdAsync(int userId)
            => await _context.Budgets.Where(b => b.UserId == userId).ToListAsync();

        public async Task<List<BudgetEntity>> FindActiveByUserIdAsync(int userId)
            => await _context.Budgets.Where(b => b.UserId == userId && b.IsActive).ToListAsync();

        public async Task<BudgetEntity?> FindByCategoryIdAsync(int userId, int categoryId)
            => await _context.Budgets
                .FirstOrDefaultAsync(b => b.UserId == userId && b.CategoryId == categoryId && b.IsActive);

        public async Task<List<BudgetEntity>> FindByPeriodAsync(int userId, string period)
            => await _context.Budgets
                .Where(b => b.UserId == userId && b.Period == period)
                .ToListAsync();

        public async Task<List<BudgetEntity>> FindOverBudgetAsync(int userId)
            => await _context.Budgets
                .Where(b => b.UserId == userId && b.SpentAmount >= b.LimitAmount)
                .ToListAsync();

        public async Task UpdateSpentAmountAsync(int budgetId, decimal amount)
            => await _context.Budgets
                .Where(b => b.BudgetId == budgetId)
                .ExecuteUpdateAsync(b => b.SetProperty(p => p.SpentAmount, p => p.SpentAmount + amount));

        public async Task AddAsync(BudgetEntity budget)
            => await _context.Budgets.AddAsync(budget);

        public async Task UpdateAsync(BudgetEntity budget)
        {
            _context.Budgets.Update(budget);
            await Task.CompletedTask;
        }

        public async Task DeleteByBudgetIdAsync(int budgetId)
            => await _context.Budgets
                .Where(b => b.BudgetId == budgetId)
                .ExecuteDeleteAsync();

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
