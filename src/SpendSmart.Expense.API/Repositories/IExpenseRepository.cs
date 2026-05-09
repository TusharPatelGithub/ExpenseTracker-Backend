using SpendSmart.Expense.API.DTOs;
using SpendSmart.Expense.API.Entities;

namespace SpendSmart.Expense.API.Repositories
{
    public interface IExpenseRepository
    {
        Task<ExpenseEntity?> FindByExpenseIdAsync(int expenseId);
        Task<List<ExpenseEntity>> FindByUserIdAsync(int userId);
        Task<List<ExpenseEntity>> FindByUserIdAndCategoryAsync(int userId, int categoryId);
        Task<List<ExpenseEntity>> FindByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<List<ExpenseEntity>> FindByPaymentModeAsync(int userId, string paymentMode);
        Task<decimal> SumByUserIdAsync(int userId);
        Task<decimal> SumByCategoryAsync(int userId, int categoryId);
        Task<List<ExpenseEntity>> FindRecurringAsync(int userId);
        Task<List<ExpenseEntity>> SearchExpensesAsync(int userId, string keyword);
        /// <summary>Admin — sum of ALL expenses platform-wide (all users).</summary>
        Task<decimal> SumAllPlatformAsync();
        /// <summary>Admin — top N categories by total spend across all users.</summary>
        Task<List<TopCategoryAdminDto>> GetTopPlatformCategoriesAsync(int topN = 5);
        Task AddAsync(ExpenseEntity expense);
        Task UpdateAsync(ExpenseEntity expense);
        Task DeleteByExpenseIdAsync(int expenseId);
        Task SaveChangesAsync();
    }
}