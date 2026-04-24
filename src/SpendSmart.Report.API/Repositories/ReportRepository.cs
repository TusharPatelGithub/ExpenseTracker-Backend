using Microsoft.EntityFrameworkCore;
using SpendSmart.Report.API.Data;
using SpendSmart.Report.API.Entities;

namespace SpendSmart.Report.API.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly ReportDbContext _context;

        public ReportRepository(ReportDbContext context)
        {
            _context = context;
        }

        public async Task<ReportEntity?> FindByReportIdAsync(int reportId)
            => await _context.Reports.FindAsync(reportId);

        public async Task<List<ReportEntity>> FindByUserIdAsync(int userId)
            => await _context.Reports.Where(r => r.UserId == userId).ToListAsync();

        public async Task<List<ReportEntity>> FindByTypeAsync(int userId, string reportType)
            => await _context.Reports
                .Where(r => r.UserId == userId && r.ReportType == reportType)
                .ToListAsync();

        public async Task<ReportEntity> SaveReportAsync(ReportEntity report)
        {
            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task DeleteByReportIdAsync(int reportId)
            => await _context.Reports
                .Where(r => r.ReportId == reportId)
                .ExecuteDeleteAsync();

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
