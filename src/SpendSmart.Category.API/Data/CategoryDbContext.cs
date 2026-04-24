using Microsoft.EntityFrameworkCore;
using SpendSmart.Category.API.Entities;

namespace SpendSmart.Category.API.Data
{
    public class CategoryDbContext : DbContext
    {
        public CategoryDbContext(DbContextOptions<CategoryDbContext> options) : base(options) { }

        public DbSet<CategoryEntity> Categories { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CategoryEntity>(entity =>
            {
                entity.HasKey(e => e.CategoryId);
                entity.HasIndex(e => new { e.UserId, e.Name });
                entity.Property(e => e.UserId).IsRequired(false);
            });
        }
    }
}
