using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendSmart.Category.API.DTOs;
using SpendSmart.Category.API.Services;
using System.Security.Claims;

namespace SpendSmart.Category.API.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
        {
            var response = await _categoryService.CreateCategoryAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = response.CategoryId }, response);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetByUser()
            => Ok(await _categoryService.GetCategoriesByUserAsync(GetUserId()));

        [HttpGet("defaults")]
        public async Task<IActionResult> GetDefaults()
            => Ok(await _categoryService.GetDefaultCategoriesAsync());

        [HttpGet("all")]
        public async Task<IActionResult> GetAllForUser()
            => Ok(await _categoryService.GetAllForUserAsync(GetUserId()));

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(string type)
            => Ok(await _categoryService.GetByTypeAsync(type));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
        {
            try
            {
                var response = await _categoryService.UpdateCategoryAsync(id, GetUserId(), dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpPatch("{id:int}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _categoryService.DeactivateCategoryAsync(id, GetUserId());
                return Ok(new { message = "Category deactivated successfully." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id, GetUserId());
                return Ok(new { message = "Category deleted successfully." });
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (KeyNotFoundException ex) { return NotFound(new { message = ex.Message }); }
        }

        // Self-service seed: user calls this with their own JWT
        [HttpPost("seed")]
        public async Task<IActionResult> SeedDefaults()
        {
            await _categoryService.SeedDefaultCategoriesAsync(GetUserId());
            return Ok(new { message = "Default categories seeded successfully." });
        }

        // Internal seed endpoint: called by Auth service after user registration
        // AllowAnonymous so it can be called without a JWT (internal traffic only)
        [AllowAnonymous]
        [HttpPost("seed/{userId:int}")]
        public async Task<IActionResult> SeedDefaultsForUser(int userId)
        {
            await _categoryService.SeedDefaultCategoriesAsync(userId);
            return Ok(new { message = $"Default categories seeded for UserId={userId}." });
        }
    }
}
