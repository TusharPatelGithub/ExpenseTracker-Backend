using SpendSmart.Report.API.DTOs;
using SpendSmart.Report.API.Entities;

namespace SpendSmart.Report.API.Services
{
    public interface IReportService
    {
        Task<MonthlySummaryDto> GetMonthlySummaryAsync(int userId, int month, int year);
        Task<List<Dictionary<string, object>>> GetCategoryBreakdownAsync(int userId, DateTime start, DateTime end);
        Task<Dictionary<string, decimal>> GetIncomeVsExpenseAsync(int userId, int month, int year);
        Task<List<Dictionary<string, object>>> GetTrendAnalysisAsync(int userId, int months);
        Task<List<Dictionary<string, object>>> GetTopExpenseCategoriesAsync(int userId, int topN);
        Task<List<Dictionary<string, object>>> GetDailySpendingAsync(int userId, int month, int year);
        Task<decimal> GetSavingsRateAsync(int userId, int month, int year);
        Task<byte[]> GeneratePdfReportAsync(int userId, string reportType, Dictionary<string, string> parameters, bool saveRecord = true);
        Task<List<ReportEntity>> GetReportsByUserAsync(int userId);
        Task DeleteReportAsync(int reportId, int userId);
        Task<MonthlySummaryDto> GetYearlySummaryAsync(int userId, int year);
    }
}
