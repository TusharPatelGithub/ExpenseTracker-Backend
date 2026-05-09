using Microsoft.EntityFrameworkCore;
using SpendSmart.Expense.API.Data;
using SpendSmart.Expense.API.DTOs;
using SpendSmart.Expense.API.Entities;

namespace SpendSmart.Expense.API.Repositories
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly ExpenseDbContext _context;

        public ExpenseRepository(ExpenseDbContext context)
        {
            _context = context;
        }

        public async Task<ExpenseEntity?> FindByExpenseIdAsync(int expenseId)
            => await _context.Expenses.FindAsync(expenseId);

        public async Task<List<ExpenseEntity>> FindByUserIdAsync(int userId)
            => await _context.Expenses.Where(e => e.UserId == userId).ToListAsync();

        public async Task<List<ExpenseEntity>> FindByUserIdAndCategoryAsync(int userId, int categoryId)
            => await _context.Expenses
                .Where(e => e.UserId == userId && e.CategoryId == categoryId)
                .ToListAsync();

        public async Task<List<ExpenseEntity>> FindByDateRangeAsync(int userId, DateTime startDate, DateTime endDate)
            => await _context.Expenses
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .ToListAsync();

        public async Task<List<ExpenseEntity>> FindByPaymentModeAsync(int userId, string paymentMode)
            => await _context.Expenses
                .Where(e => e.UserId == userId && e.PaymentMode == paymentMode)
                .ToListAsync();

        public async Task<decimal> SumByUserIdAsync(int userId)
            => await _context.Expenses
                .Where(e => e.UserId == userId)
                .SumAsync(e => e.Amount);

        public async Task<decimal> SumByCategoryAsync(int userId, int categoryId)
            => await _context.Expenses
                .Where(e => e.UserId == userId && e.CategoryId == categoryId)
                .SumAsync(e => e.Amount);

        public async Task<List<ExpenseEntity>> FindRecurringAsync(int userId)
            => await _context.Expenses
                .Where(e => e.UserId == userId && e.IsRecurring)
                .ToListAsync();

        public async Task<List<ExpenseEntity>> SearchExpensesAsync(int userId, string keyword)
        {
            var pattern = $"%{keyword}%";
            return await _context.Expenses
                .Where(e => e.UserId == userId && EF.Functions.Like(e.Description, pattern))
                .ToListAsync();
        }

        public async Task<decimal> SumAllPlatformAsync()
            => await _context.Expenses.SumAsync(e => e.Amount);

        public async Task<List<TopCategoryAdminDto>> GetTopPlatformCategoriesAsync(int topN = 5)
        {
            return await _context.Expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new TopCategoryAdminDto
                {
                    CategoryId    = g.Key,
                    CategoryName  = $"Category {g.Key}",
                    TotalAmount   = g.Sum(e => e.Amount)
                })
                .OrderByDescending(c => c.TotalAmount)
                .Take(topN)
                .ToListAsync();
        }

        public async Task AddAsync(ExpenseEntity expense)
            => await _context.Expenses.AddAsync(expense);

        public async Task UpdateAsync(ExpenseEntity expense)
        {
            expense.UpdatedAt = DateTime.UtcNow;
            _context.Expenses.Update(expense);
            await Task.CompletedTask;
        }

        public async Task DeleteByExpenseIdAsync(int expenseId)
            => await _context.Expenses
                .Where(e => e.ExpenseId == expenseId)
                .ExecuteDeleteAsync();

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}