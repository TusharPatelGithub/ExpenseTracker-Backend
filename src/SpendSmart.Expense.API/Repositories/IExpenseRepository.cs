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
        Task AddAsync(ExpenseEntity expense);
        Task UpdateAsync(ExpenseEntity expense);
        Task DeleteByExpenseIdAsync(int expenseId);
        Task SaveChangesAsync();
    }
}