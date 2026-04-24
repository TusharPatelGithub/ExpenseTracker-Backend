using Microsoft.EntityFrameworkCore;
using SpendSmart.Budget.API.Entities;

namespace SpendSmart.Budget.API.Data
{
    public class BudgetDbContext : DbContext
    {
        public BudgetDbContext(DbContextOptions<BudgetDbContext> options) : base(options) { }

        public DbSet<BudgetEntity> Budgets { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BudgetEntity>(entity =>
            {
                entity.HasKey(e => e.BudgetId);
                entity.Property(e => e.LimitAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SpentAmount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CategoryId);
            });
        }
    }
}
