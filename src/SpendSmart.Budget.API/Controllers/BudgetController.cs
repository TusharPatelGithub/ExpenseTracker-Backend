using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Budget.API.DTOs;
using SpendSmart.Budget.API.Services;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace SpendSmart.Budget.API.Controllers
{
    [ApiController]
    [Route("api/budgets")]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly IBudgetService _budgetService;

        public BudgetController(IBudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetDto dto)
        {
            var response = await _budgetService.CreateBudgetAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = response.BudgetId }, response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var budget = await _budgetService.GetBudgetByIdAsync(id);
            if (budget == null || budget.UserId != GetUserId()) return NotFound();
            return Ok(budget);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetByUser()
            => Ok(await _budgetService.GetBudgetsByUserAsync(GetUserId()));

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
            => Ok(await _budgetService.GetActiveBudgetsAsync(GetUserId()));

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var budget = await _budgetService.GetBudgetByCategoryAsync(GetUserId(), categoryId);
            if (budget == null) return NotFound();
            return Ok(budget);
        }

        [HttpGet("period/{period}")]
        public async Task<IActionResult> GetByPeriod(string period)
            => Ok(await _budgetService.GetByPeriodAsync(GetUserId(), period));

        [HttpGet("alerts")]
        public async Task<IActionResult> GetOverBudgetAlerts()
            => Ok(await _budgetService.GetOverBudgetAlertsAsync(GetUserId()));

        [HttpGet("utilization")]
        public async Task<IActionResult> GetUtilization()
            => Ok(new { Utilization = await _budgetService.GetBudgetUtilizationAsync(GetUserId()) });

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateBudget(int id, [FromBody] UpdateBudgetDto dto)
        {
            try
            {
                var response = await _budgetService.UpdateBudgetAsync(id, GetUserId(), dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            try
            {
                await _budgetService.DeleteBudgetAsync(id, GetUserId());
                return Ok(new { message = "Budget deleted successfully." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ── Internal service-to-service endpoint (no JWT required) ────────────
        [AllowAnonymous]
        [HttpPost("internal/expense-created")]
        public async Task<IActionResult> OnExpenseCreated([FromBody] InternalExpenseCreatedDto dto)
        {
            await _budgetService.CheckBudgetOnExpenseAsync(dto.UserId, dto.CategoryId, dto.Amount);
            return Ok();
        }
    }

    public record InternalExpenseCreatedDto(
        [property: JsonPropertyName("userId")] int UserId,
        [property: JsonPropertyName("categoryId")] int CategoryId,
        [property: JsonPropertyName("amount")] decimal Amount
    );
}
