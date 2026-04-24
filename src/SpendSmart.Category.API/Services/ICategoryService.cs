using SpendSmart.Category.API.DTOs;

namespace SpendSmart.Category.API.Services
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> CreateCategoryAsync(int userId, CreateCategoryDto dto);
        Task<CategoryResponseDto?> GetCategoryByIdAsync(int categoryId);
        Task<List<CategoryResponseDto>> GetCategoriesByUserAsync(int userId);
        Task<List<CategoryResponseDto>> GetDefaultCategoriesAsync();
        Task<List<CategoryResponseDto>> GetAllForUserAsync(int userId);
        Task<List<CategoryResponseDto>> GetByTypeAsync(string type);
        Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, int userId, UpdateCategoryDto dto);
        Task DeactivateCategoryAsync(int categoryId, int userId);
        Task DeleteCategoryAsync(int categoryId, int userId);
        Task SeedDefaultCategoriesAsync(int userId);
    }
}
