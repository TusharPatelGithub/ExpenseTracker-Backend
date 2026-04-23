using Microsoft.AspNetCore.Http;
using SpendSmart.Expense.API.DTOs;

namespace SpendSmart.Expense.API.Services
{
    public interface IExpenseService
    {
        Task<ExpenseResponseDto> AddExpenseAsync(int userId, AddExpenseDto dto, IFormFile? receipt);
        Task<ExpenseResponseDto?> GetExpenseByIdAsync(int expenseId);
        Task<List<ExpenseResponseDto>> GetExpensesByUserAsync(int userId);
        Task<List<ExpenseResponseDto>> GetByCategoryAsync(int userId, int categoryId);
        Task<List<ExpenseResponseDto>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<List<ExpenseResponseDto>> GetByPaymentModeAsync(int userId, string paymentMode);
        Task<ExpenseResponseDto> UpdateExpenseAsync(int expenseId, int userId, UpdateExpenseDto dto);
        Task DeleteExpenseAsync(int expenseId, int userId);
        Task<decimal> GetTotalByUserAsync(int userId);
        Task<decimal> GetTotalByCategoryAsync(int userId, int categoryId);
        Task<List<ExpenseResponseDto>> GetRecurringExpensesAsync(int userId);
        Task<List<ExpenseResponseDto>> SearchExpensesAsync(int userId, string keyword);
    }
}
