using Microsoft.EntityFrameworkCore;
using SpendSmart.Expense.API.Entities;

namespace SpendSmart.Expense.API.Data
{
    public class ExpenseDbContext : DbContext
    {
        public ExpenseDbContext(DbContextOptions<ExpenseDbContext> options) : base(options) { }

        public DbSet<ExpenseEntity> Expenses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ExpenseEntity>(entity =>
            {
                entity.HasKey(e => e.ExpenseId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CategoryId);
                entity.HasIndex(e => e.Date);
            });
        }
    }
}