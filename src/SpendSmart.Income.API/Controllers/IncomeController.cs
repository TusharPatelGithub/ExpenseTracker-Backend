using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Income.API.DTOs;
using SpendSmart.Income.API.Services;
using System.Security.Claims;

namespace SpendSmart.Income.API.Controllers
{
    [ApiController]
    [Route("api/incomes")]
    [Authorize]
    public class IncomeController : ControllerBase
    {
        private readonly IIncomeService _incomeService;

        public IncomeController(IIncomeService incomeService)
        {
            _incomeService = incomeService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> AddIncome([FromBody] AddIncomeDto dto)
        {
            var response = await _incomeService.AddIncomeAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = response.IncomeId }, response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var income = await _incomeService.GetIncomeByIdAsync(id);
            if (income == null || income.UserId != GetUserId()) return NotFound();
            return Ok(income);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetByUser()
            => Ok(await _incomeService.GetIncomesByUserAsync(GetUserId()));

        [HttpGet("source/{source}")]
        public async Task<IActionResult> GetBySource(string source)
            => Ok(await _incomeService.GetBySourceAsync(GetUserId(), source));

        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
            => Ok(await _incomeService.GetByDateRangeAsync(GetUserId(), start, end));

        [HttpGet("recurring")]
        public async Task<IActionResult> GetRecurring()
            => Ok(await _incomeService.GetRecurringIncomesAsync(GetUserId()));

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal()
            => Ok(new { Total = await _incomeService.GetTotalIncomeAsync(GetUserId()) });

        [HttpGet("total/source/{source}")]
        public async Task<IActionResult> GetTotalBySource(string source)
            => Ok(new { Total = await _incomeService.GetTotalBySourceAsync(GetUserId(), source) });

        [HttpGet("net-balance")]
        public async Task<IActionResult> GetNetBalance()
            => Ok(new { NetBalance = await _incomeService.GetNetBalanceAsync(GetUserId()) });

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateIncome(int id, [FromBody] UpdateIncomeDto dto)
        {
            try
            {
                var response = await _incomeService.UpdateIncomeAsync(id, GetUserId(), dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteIncome(int id)
        {
            try
            {
                await _incomeService.DeleteIncomeAsync(id, GetUserId());
                return Ok(new { message = "Income deleted successfully." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // ─── Admin-Only ────────────────────────────────────────────────────────────

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/total")]
        public async Task<IActionResult> GetPlatformTotal()
        {
            var total = await _incomeService.GetPlatformTotalAsync();
            return Ok(new { Total = total });
        }
    }
}
