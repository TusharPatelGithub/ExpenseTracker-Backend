using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Report.API.Services;
using System.Security.Claims;

namespace SpendSmart.Report.API.Controllers
{
    [ApiController]
    [Route("api/reports")]
    [Authorize]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlySummary([FromQuery] int month, [FromQuery] int year)
            => Ok(await _reportService.GetMonthlySummaryAsync(GetUserId(), month, year));

        [HttpGet("category-breakdown")]
        public async Task<IActionResult> GetCategoryBreakdown([FromQuery] DateTime start, [FromQuery] DateTime end)
            => Ok(await _reportService.GetCategoryBreakdownAsync(GetUserId(), start, end));

        [HttpGet("income-vs-expense")]
        public async Task<IActionResult> GetIncomeVsExpense([FromQuery] int month, [FromQuery] int year)
            => Ok(await _reportService.GetIncomeVsExpenseAsync(GetUserId(), month, year));

        [HttpGet("trend")]
        public async Task<IActionResult> GetTrendAnalysis([FromQuery] int months = 6)
            => Ok(await _reportService.GetTrendAnalysisAsync(GetUserId(), months));

        [HttpGet("top-categories")]
        public async Task<IActionResult> GetTopCategories([FromQuery] int topN = 5)
            => Ok(await _reportService.GetTopExpenseCategoriesAsync(GetUserId(), topN));

        [HttpGet("daily-spending")]
        public async Task<IActionResult> GetDailySpending([FromQuery] int month, [FromQuery] int year)
            => Ok(await _reportService.GetDailySpendingAsync(GetUserId(), month, year));

        [HttpGet("savings-rate")]
        public async Task<IActionResult> GetSavingsRate([FromQuery] int month, [FromQuery] int year)
            => Ok(new { SavingsRate = await _reportService.GetSavingsRateAsync(GetUserId(), month, year) });

        [HttpGet("yearly")]
        public async Task<IActionResult> GetYearlySummary([FromQuery] int year)
            => Ok(await _reportService.GetYearlySummaryAsync(GetUserId(), year));

        [HttpPost("generate-pdf")]
        public async Task<IActionResult> GeneratePdf([FromBody] GeneratePdfRequest request)
        {
            try
            {
                var filePath = await _reportService.GeneratePdfReportAsync(GetUserId(), request.ReportType, request.Parameters);
                return Ok(new { message = "Report generated successfully.", filePath });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-reports")]
        public async Task<IActionResult> GetMyReports()
            => Ok(await _reportService.GetReportsByUserAsync(GetUserId()));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReport(int id)
        {
            try
            {
                await _reportService.DeleteReportAsync(id, GetUserId());
                return Ok(new { message = "Report deleted successfully." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }
    }

    public class GeneratePdfRequest
    {
        public string ReportType { get; set; } = "MONTHLY";
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}
