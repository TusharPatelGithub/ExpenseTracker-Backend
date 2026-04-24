using Microsoft.EntityFrameworkCore;
using SpendSmart.Category.API.Data;
using SpendSmart.Category.API.Entities;

namespace SpendSmart.Category.API.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly CategoryDbContext _context;

        public CategoryRepository(CategoryDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryEntity?> FindByCategoryIdAsync(int categoryId)
            => await _context.Categories.FindAsync(categoryId);

        public async Task<List<CategoryEntity>> FindByUserIdAsync(int userId)
            => await _context.Categories.Where(c => c.UserId == userId).ToListAsync();

        public async Task<List<CategoryEntity>> FindDefaultCategoriesAsync()
            => await _context.Categories.Where(c => c.UserId == null && c.IsDefault).ToListAsync();

        public async Task<List<CategoryEntity>> FindByTypeAsync(string type)
            => await _context.Categories.Where(c => c.Type == type).ToListAsync();

        public async Task<CategoryEntity?> FindByNameAsync(int userId, string name)
            => await _context.Categories
                .FirstOrDefaultAsync(c => c.UserId == userId && c.Name == name);

        public async Task<bool> ExistsByNameAsync(int userId, string name)
            => await _context.Categories
                .AnyAsync(c => c.UserId == null && c.Name == name);

        public async Task<List<CategoryEntity>> FindAllForUserAsync(int userId)
            => await _context.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .ToListAsync();

        public async Task AddAsync(CategoryEntity category)
            => await _context.Categories.AddAsync(category);

        public async Task UpdateAsync(CategoryEntity category)
        {
            _context.Categories.Update(category);
            await Task.CompletedTask;
        }

        public async Task DeleteByCategoryIdAsync(int categoryId)
            => await _context.Categories
                .Where(c => c.CategoryId == categoryId)
                .ExecuteDeleteAsync();

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}
