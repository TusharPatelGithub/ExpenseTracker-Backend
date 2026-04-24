using SpendSmart.Category.API.DTOs;
using SpendSmart.Category.API.Entities;
using SpendSmart.Category.API.Repositories;

namespace SpendSmart.Category.API.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(int userId, CreateCategoryDto dto)
        {
            var category = new CategoryEntity
            {
                UserId = userId,
                Name = dto.Name,
                Icon = dto.Icon,
                Color = dto.Color,
                Type = dto.Type,
                IsDefault = false,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return MapToResponseDto(category);
        }

        public async Task<CategoryResponseDto?> GetCategoryByIdAsync(int categoryId)
        {
            var category = await _categoryRepository.FindByCategoryIdAsync(categoryId);
            return category == null ? null : MapToResponseDto(category);
        }

        public async Task<List<CategoryResponseDto>> GetCategoriesByUserAsync(int userId)
        {
            var categories = await _categoryRepository.FindByUserIdAsync(userId);
            return categories.Select(MapToResponseDto).ToList();
        }

        public async Task<List<CategoryResponseDto>> GetDefaultCategoriesAsync()
        {
            var categories = await _categoryRepository.FindDefaultCategoriesAsync();
            return categories.Select(MapToResponseDto).ToList();
        }

        public async Task<List<CategoryResponseDto>> GetAllForUserAsync(int userId)
        {
            var categories = await _categoryRepository.FindAllForUserAsync(userId);
            return categories.Select(MapToResponseDto).ToList();
        }

        public async Task<List<CategoryResponseDto>> GetByTypeAsync(string type)
        {
            var categories = await _categoryRepository.FindByTypeAsync(type);
            return categories.Select(MapToResponseDto).ToList();
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, int userId, UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.FindByCategoryIdAsync(categoryId)
                ?? throw new KeyNotFoundException("Category not found.");

            if (category.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to update this category.");

            category.Name = dto.Name;
            category.Icon = dto.Icon;
            category.Color = dto.Color;
            category.Type = dto.Type;

            await _categoryRepository.UpdateAsync(category);
            await _categoryRepository.SaveChangesAsync();

            return MapToResponseDto(category);
        }

        public async Task DeactivateCategoryAsync(int categoryId, int userId)
        {
            var category = await _categoryRepository.FindByCategoryIdAsync(categoryId)
                ?? throw new KeyNotFoundException("Category not found.");

            if (category.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to deactivate this category.");

            await _categoryRepository.UpdateAsync(category);
            category.IsActive = false;
            await _categoryRepository.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int categoryId, int userId)
        {
            var category = await _categoryRepository.FindByCategoryIdAsync(categoryId)
                ?? throw new KeyNotFoundException("Category not found.");

            if (category.UserId != userId)
                throw new UnauthorizedAccessException("Unauthorized to delete this category.");

            await _categoryRepository.DeleteByCategoryIdAsync(categoryId);
        }

        public async Task SeedDefaultCategoriesAsync(int userId)
        {
            var defaults = new[]
            {
                new { Name = "Food & Dining", Icon = "??", Color = "#FF6B6B", Type = "EXPENSE" },
                new { Name = "Transport", Icon = "??", Color = "#4ECDC4", Type = "EXPENSE" },
                new { Name = "Shopping", Icon = "???", Color = "#45B7D1", Type = "EXPENSE" },
                new { Name = "Healthcare", Icon = "??", Color = "#96CEB4", Type = "EXPENSE" },
                new { Name = "Entertainment", Icon = "??", Color = "#FFEAA7", Type = "EXPENSE" },
                new { Name = "Utilities", Icon = "??", Color = "#DDA0DD", Type = "EXPENSE" },
                new { Name = "Salary", Icon = "??", Color = "#98D8C8", Type = "INCOME" },
                new { Name = "Freelance", Icon = "??", Color = "#F7DC6F", Type = "INCOME" },
                new { Name = "Investment", Icon = "??", Color = "#82E0AA", Type = "INCOME" },
                new { Name = "Rental", Icon = "??", Color = "#F1948A", Type = "INCOME" },
            };

            foreach (var d in defaults)
            {
                var exists = await _categoryRepository.ExistsByNameAsync(userId, d.Name);
                if (!exists)
                {
                    await _categoryRepository.AddAsync(new CategoryEntity
                    {
                        UserId = null,
                        Name = d.Name,
                        Icon = d.Icon,
                        Color = d.Color,
                        Type = d.Type,
                        IsDefault = true,
                        IsActive = true
                    });
                }
            }

            await _categoryRepository.SaveChangesAsync();
        }

        private static CategoryResponseDto MapToResponseDto(CategoryEntity c) => new()
        {
            CategoryId = c.CategoryId,
            UserId = c.UserId,
            Name = c.Name,
            Icon = c.Icon,
            Color = c.Color,
            Type = c.Type,
            IsDefault = c.IsDefault,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt
        };
    }
}
