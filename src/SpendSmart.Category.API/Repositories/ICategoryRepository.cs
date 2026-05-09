using SpendSmart.Category.API.Entities;

namespace SpendSmart.Category.API.Repositories
{
    public interface ICategoryRepository
    {
        Task<CategoryEntity?> FindByCategoryIdAsync(int categoryId);
        Task<List<CategoryEntity>> FindByUserIdAsync(int userId);
        Task<List<CategoryEntity>> FindDefaultCategoriesAsync();
        Task<List<CategoryEntity>> FindByTypeAsync(string type);
        Task<CategoryEntity?> FindByNameAsync(int userId, string name);
        Task<bool> ExistsByNameAsync(int userId, string name);
        Task<List<CategoryEntity>> FindAllForUserAsync(int userId);
        Task AddAsync(CategoryEntity category);
        Task UpdateAsync(CategoryEntity category);
        Task DeactivateByCategoryIdAsync(int categoryId);
        Task DeleteByCategoryIdAsync(int categoryId);
        Task SaveChangesAsync();
    }
}
