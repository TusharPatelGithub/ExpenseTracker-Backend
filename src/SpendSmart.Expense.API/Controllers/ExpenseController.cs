using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Expense.API.DTOs;
using SpendSmart.Expense.API.Services;
using System.Security.Claims;

namespace SpendSmart.Expense.API.Controllers
{
    [ApiController]
    [Route("api/expenses")]
    [Authorize] // All endpoints require authentication
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;

        public ExpenseController(IExpenseService expenseService)
        {
            _expenseService = expenseService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> AddExpense([FromForm] AddExpenseDto dto, IFormFile? receiptFile)
        {
            try
            {
                var response = await _expenseService.AddExpenseAsync(GetUserId(), dto, receiptFile);
                return CreatedAtAction(nameof(GetById), new { id = response.ExpenseId }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var expense = await _expenseService.GetExpenseByIdAsync(id);
            if (expense == null || expense.UserId != GetUserId()) return NotFound();
            return Ok(expense);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetByUser()
        {
            var expenses = await _expenseService.GetExpensesByUserAsync(GetUserId());
            return Ok(expenses);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetByCategory(int categoryId)
        {
            var expenses = await _expenseService.GetByCategoryAsync(GetUserId(), categoryId);
            return Ok(expenses);
        }

        [HttpGet("date-range")]
        public async Task<IActionResult> GetByDateRange([FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var expenses = await _expenseService.GetByDateRangeAsync(GetUserId(), start, end);
            return Ok(expenses);
        }

        [HttpGet("payment-mode/{mode}")]
        public async Task<IActionResult> GetByPaymentMode(string mode)
        {
            var expenses = await _expenseService.GetByPaymentModeAsync(GetUserId(), mode);
            return Ok(expenses);
        }

        [HttpGet("recurring")]
        public async Task<IActionResult> GetRecurring()
        {
            var expenses = await _expenseService.GetRecurringExpensesAsync(GetUserId());
            return Ok(expenses);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Search keyword 'q' is required.");

            var expenses = await _expenseService.SearchExpensesAsync(GetUserId(), q);
            return Ok(expenses);
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetTotal()
        {
            var total = await _expenseService.GetTotalByUserAsync(GetUserId());
            return Ok(new { Total = total });
        }

        [HttpGet("total/category/{categoryId:int}")]
        public async Task<IActionResult> GetTotalByCategory(int categoryId)
        {
            var total = await _expenseService.GetTotalByCategoryAsync(GetUserId(), categoryId);
            return Ok(new { Total = total });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] UpdateExpenseDto dto)
        {
            try
            {
                var response = await _expenseService.UpdateExpenseAsync(id, GetUserId(), dto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            try
            {
                await _expenseService.DeleteExpenseAsync(id, GetUserId());
                return Ok(new { message = "Expense deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ─── Admin-Only ────────────────────────────────────────────────────────────

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/total")]
        public async Task<IActionResult> GetPlatformTotal()
        {
            var total = await _expenseService.GetPlatformTotalAsync();
            return Ok(new { Total = total });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/top-categories")]
        public async Task<IActionResult> GetTopPlatformCategories([FromQuery] int topN = 5)
        {
            var categories = await _expenseService.GetTopPlatformCategoriesAsync(topN);
            return Ok(categories);
        }
    }
}
