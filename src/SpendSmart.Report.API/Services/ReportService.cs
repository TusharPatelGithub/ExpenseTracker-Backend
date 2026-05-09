using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SpendSmart.Report.API.DTOs;
using SpendSmart.Report.API.Entities;
using SpendSmart.Report.API.HttpClients;
using SpendSmart.Report.API.Repositories;
using System.Text.Json;

namespace SpendSmart.Report.API.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly ExpenseHttpClient _expenseClient;
        private readonly IncomeHttpClient _incomeClient;
        private readonly IWebHostEnvironment _env;

        public ReportService(
            IReportRepository reportRepository,
            ExpenseHttpClient expenseClient,
            IncomeHttpClient incomeClient,
            IWebHostEnvironment env)
        {
            _reportRepository = reportRepository;
            _expenseClient = expenseClient;
            _incomeClient = incomeClient;
            _env = env;
        }

        public async Task<MonthlySummaryDto> GetMonthlySummaryAsync(int userId, int month, int year)
        {
            var expenses = await _expenseClient.GetAllByUserAsync();
            var incomes = await _incomeClient.GetAllByUserAsync();

            var totalExpense = expenses
                .Where(e => e.Date.Month == month && e.Date.Year == year)
                .Sum(e => e.Amount);

            var totalIncome = incomes
                .Where(i => i.Date.Month == month && i.Date.Year == year)
                .Sum(i => i.Amount);

            var netSavings = totalIncome - totalExpense;
            var savingsRate = totalIncome == 0 ? 0 : ((totalIncome - totalExpense) / totalIncome) * 100;

            return new MonthlySummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetSavings = netSavings,
                SavingsRate = Math.Round(savingsRate, 2),
                Month = month,
                Year = year
            };
        }

        public async Task<List<Dictionary<string, object>>> GetCategoryBreakdownAsync(int userId, DateTime start, DateTime end)
        {
            var breakdown = await _expenseClient.GetCategoryBreakdownAsync(userId, start, end);
            return breakdown.Select(b => new Dictionary<string, object>
            {
                ["categoryId"] = b.CategoryId,
                ["total"] = b.Total
            }).ToList();
        }

        public async Task<Dictionary<string, decimal>> GetIncomeVsExpenseAsync(int userId, int month, int year)
        {
            var summary = await GetMonthlySummaryAsync(userId, month, year);
            return new Dictionary<string, decimal>
            {
                ["totalIncome"] = summary.TotalIncome,
                ["totalExpense"] = summary.TotalExpense,
                ["netSavings"] = summary.NetSavings
            };
        }

        public async Task<List<Dictionary<string, object>>> GetTrendAnalysisAsync(int userId, int months)
        {
            var result = new List<Dictionary<string, object>>();
            var now = DateTime.UtcNow;

            for (int i = months - 1; i >= 0; i--)
            {
                var date = now.AddMonths(-i);
                var summary = await GetMonthlySummaryAsync(userId, date.Month, date.Year);
                result.Add(new Dictionary<string, object>
                {
                    ["month"] = date.Month,
                    ["year"] = date.Year,
                    ["totalExpense"] = summary.TotalExpense,
                    ["totalIncome"] = summary.TotalIncome,
                    ["netSavings"] = summary.NetSavings
                });
            }

            return result;
        }

        public async Task<List<Dictionary<string, object>>> GetTopExpenseCategoriesAsync(int userId, int topN)
        {
            var expenses = await _expenseClient.GetAllByUserAsync();
            return expenses
                .GroupBy(e => e.CategoryId)
                .Select(g => new Dictionary<string, object>
                {
                    ["categoryId"] = g.Key,
                    ["total"] = g.Sum(e => e.Amount)
                })
                .OrderByDescending(d => (decimal)d["total"])
                .Take(topN)
                .ToList();
        }

        public async Task<List<Dictionary<string, object>>> GetDailySpendingAsync(int userId, int month, int year)
        {
            var expenses = await _expenseClient.GetAllByUserAsync();
            return expenses
                .Where(e => e.Date.Month == month && e.Date.Year == year)
                .GroupBy(e => e.Date.Day)
                .Select(g => new Dictionary<string, object>
                {
                    ["day"] = g.Key,
                    ["total"] = g.Sum(e => e.Amount)
                })
                .OrderBy(d => (int)d["day"])
                .ToList();
        }

        public async Task<decimal> GetSavingsRateAsync(int userId, int month, int year)
        {
            var summary = await GetMonthlySummaryAsync(userId, month, year);
            return summary.SavingsRate;
        }

        public async Task<string> GeneratePdfReportAsync(int userId, string reportType, Dictionary<string, string> parameters)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var month = parameters.ContainsKey("month") ? int.Parse(parameters["month"]) : DateTime.UtcNow.Month;
            var year  = parameters.ContainsKey("year")  ? int.Parse(parameters["year"])  : DateTime.UtcNow.Year;

            var summary  = await GetMonthlySummaryAsync(userId, month, year);
            var fileName = $"report_{userId}_{reportType}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(_env.ContentRootPath, "Reports", fileName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.Content().Column(col =>
                        {
                            col.Item().Text($"SpendSmart - {reportType} Report").FontSize(20).Bold();
                            col.Item().Text($"User: {userId} | Period: {month}/{year}").FontSize(12);
                            col.Item().PaddingTop(10).Text($"Total Income:  {summary.TotalIncome:C}").FontSize(12);
                            col.Item().Text($"Total Expense: {summary.TotalExpense:C}").FontSize(12);
                            col.Item().Text($"Net Savings:   {summary.NetSavings:C}").FontSize(12);
                            col.Item().Text($"Savings Rate:  {summary.SavingsRate}%").FontSize(12);
                        });
                    });
                }).GeneratePdf(filePath);

                // ── Success: persist report record with GENERATED status ──────────
                var report = new ReportEntity
                {
                    UserId     = userId,
                    ReportType = reportType,
                    Title      = $"{reportType} Report - {month}/{year}",
                    FilePath   = filePath,
                    Parameters = JsonSerializer.Serialize(parameters),
                    Status     = "GENERATED"
                };

                await _reportRepository.SaveReportAsync(report);
                return filePath;
            }
            catch (Exception)
            {
                // ── Failure: persist a FAILED record so the user can see it ──────
                var failedReport = new ReportEntity
                {
                    UserId     = userId,
                    ReportType = reportType,
                    Title      = $"{reportType} Report - {month}/{year}",
                    FilePath   = null,           // no file was produced
                    Parameters = JsonSerializer.Serialize(parameters),
                    Status     = "FAILED"
                };

                await _reportRepository.SaveReportAsync(failedReport);
                throw;   // rethrow so the controller returns 400 with the error message
            }
        }

        public async Task<List<ReportEntity>> GetReportsByUserAsync(int userId)
            => await _reportRepository.FindByUserIdAsync(userId);

        public async Task DeleteReportAsync(int reportId, int userId)
        {
            var report = await _reportRepository.FindByReportIdAsync(reportId)
                ?? throw new KeyNotFoundException("Report not found.");

            if (report.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to delete this report.");

            await _reportRepository.DeleteByReportIdAsync(reportId);
        }

        public async Task<MonthlySummaryDto> GetYearlySummaryAsync(int userId, int year)
        {
            var expenses = await _expenseClient.GetAllByUserAsync();
            var incomes = await _incomeClient.GetAllByUserAsync();

            var totalExpense = expenses.Where(e => e.Date.Year == year).Sum(e => e.Amount);
            var totalIncome = incomes.Where(i => i.Date.Year == year).Sum(i => i.Amount);
            var netSavings = totalIncome - totalExpense;
            var savingsRate = totalIncome == 0 ? 0 : ((totalIncome - totalExpense) / totalIncome) * 100;

            return new MonthlySummaryDto
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetSavings = netSavings,
                SavingsRate = Math.Round(savingsRate, 2),
                Month = 0,
                Year = year
            };
        }
    }
}
