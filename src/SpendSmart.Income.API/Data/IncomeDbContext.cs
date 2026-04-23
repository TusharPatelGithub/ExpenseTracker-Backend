using Microsoft.EntityFrameworkCore;
using SpendSmart.Income.API.Entities;

namespace SpendSmart.Income.API.Data
{
    public class IncomeDbContext : DbContext
    {
        public IncomeDbContext(DbContextOptions<IncomeDbContext> options) : base(options) { }

        public DbSet<IncomeEntity> Incomes { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IncomeEntity>(entity =>
            {
                entity.HasKey(e => e.IncomeId);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Source);
                entity.HasIndex(e => e.Date);
            });
        }
    }
}
