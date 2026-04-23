using SpendSmart.Income.API.Entities;

namespace SpendSmart.Income.API.Repositories
{
    public interface IIncomeRepository
    {
        Task<IncomeEntity?> FindByIncomeIdAsync(int incomeId);
        Task<List<IncomeEntity>> FindByUserIdAsync(int userId);
        Task<List<IncomeEntity>> FindBySourceAsync(int userId, string source);
        Task<List<IncomeEntity>> FindByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<decimal> SumByUserIdAsync(int userId);
        Task<decimal> SumBySourceAsync(int userId, string source);
        Task<List<IncomeEntity>> FindRecurringAsync(int userId);
        Task AddAsync(IncomeEntity income);
        Task UpdateAsync(IncomeEntity income);
        Task DeleteByIncomeIdAsync(int incomeId);
        Task SaveChangesAsync();
    }
}
