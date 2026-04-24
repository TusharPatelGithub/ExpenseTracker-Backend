using Microsoft.EntityFrameworkCore;
using SpendSmart.Report.API.Entities;

namespace SpendSmart.Report.API.Data
{
    public class ReportDbContext : DbContext
    {
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) { }

        public DbSet<ReportEntity> Reports { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ReportEntity>(entity =>
            {
                entity.HasKey(e => e.ReportId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.ReportType);
            });
        }
    }
}
