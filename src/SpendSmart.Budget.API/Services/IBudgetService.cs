using SpendSmart.Budget.API.DTOs;

namespace SpendSmart.Budget.API.Services
{
    public interface IBudgetService
    {
        Task<BudgetResponseDto> CreateBudgetAsync(int userId, CreateBudgetDto dto);
        Task<BudgetResponseDto?> GetBudgetByIdAsync(int budgetId);
        Task<List<BudgetResponseDto>> GetBudgetsByUserAsync(int userId);
        Task<List<BudgetResponseDto>> GetActiveBudgetsAsync(int userId);
        Task<BudgetResponseDto?> GetBudgetByCategoryAsync(int userId, int categoryId);
        Task<List<BudgetResponseDto>> GetByPeriodAsync(int userId, string period);
        Task<BudgetResponseDto> UpdateBudgetAsync(int budgetId, int userId, UpdateBudgetDto dto);
        Task DeleteBudgetAsync(int budgetId, int userId);
        Task UpdateSpentAmountAsync(int budgetId, decimal amount);
        Task<List<BudgetResponseDto>> GetOverBudgetAlertsAsync(int userId);
        Task<decimal> GetBudgetUtilizationAsync(int userId);
        Task CheckBudgetOnExpenseAsync(int userId, int categoryId, decimal amount);
    }
}
