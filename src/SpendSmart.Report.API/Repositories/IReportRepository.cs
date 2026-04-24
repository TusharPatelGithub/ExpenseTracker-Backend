using SpendSmart.Report.API.Entities;

namespace SpendSmart.Report.API.Repositories
{
    public interface IReportRepository
    {
        Task<ReportEntity?> FindByReportIdAsync(int reportId);
        Task<List<ReportEntity>> FindByUserIdAsync(int userId);
        Task<List<ReportEntity>> FindByTypeAsync(int userId, string reportType);
        Task<ReportEntity> SaveReportAsync(ReportEntity report);
        Task DeleteByReportIdAsync(int reportId);
        Task SaveChangesAsync();
    }
}
