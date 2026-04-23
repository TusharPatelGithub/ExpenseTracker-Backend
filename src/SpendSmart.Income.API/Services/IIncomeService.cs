using SpendSmart.Income.API.DTOs;

namespace SpendSmart.Income.API.Services
{
    public interface IIncomeService
    {
        Task<IncomeResponseDto> AddIncomeAsync(int userId, AddIncomeDto dto);
        Task<IncomeResponseDto?> GetIncomeByIdAsync(int incomeId);
        Task<List<IncomeResponseDto>> GetIncomesByUserAsync(int userId);
        Task<List<IncomeResponseDto>> GetBySourceAsync(int userId, string source);
        Task<List<IncomeResponseDto>> GetByDateRangeAsync(int userId, DateTime startDate, DateTime endDate);
        Task<IncomeResponseDto> UpdateIncomeAsync(int incomeId, int userId, UpdateIncomeDto dto);
        Task DeleteIncomeAsync(int incomeId, int userId);
        Task<decimal> GetTotalIncomeAsync(int userId);
        Task<decimal> GetTotalBySourceAsync(int userId, string source);
        Task<List<IncomeResponseDto>> GetRecurringIncomesAsync(int userId);
        Task<decimal> GetNetBalanceAsync(int userId);
    }
}
