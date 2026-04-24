using SpendSmart.Budget.API.Entities;

namespace SpendSmart.Budget.API.Repositories
{
    public interface IBudgetRepository
    {
        Task<BudgetEntity?> FindByBudgetIdAsync(int budgetId);
        Task<List<BudgetEntity>> FindByUserIdAsync(int userId);
        Task<List<BudgetEntity>> FindActiveByUserIdAsync(int userId);
        Task<BudgetEntity?> FindByCategoryIdAsync(int userId, int categoryId);
        Task<List<BudgetEntity>> FindByPeriodAsync(int userId, string period);
        Task<List<BudgetEntity>> FindOverBudgetAsync(int userId);
        Task UpdateSpentAmountAsync(int budgetId, decimal amount);
        Task AddAsync(BudgetEntity budget);
        Task UpdateAsync(BudgetEntity budget);
        Task DeleteByBudgetIdAsync(int budgetId);
        Task SaveChangesAsync();
    }
}
