using Microsoft.EntityFrameworkCore;
using SpendSmart.Auth.API.Entities;

namespace SpendSmart.Auth.API.Data
{
    public class AuthDbContext : DbContext
    {
        public AuthDbContext(DbContextOptions<AuthDbContext> options) 
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Unique index on Email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Default value for CreatedAt
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("NOW()");

            // Index for admin audit log queries: by actor and time
            modelBuilder.Entity<AuditLog>()
                .HasIndex(a => new { a.ActorUserId, a.Timestamp });
        }
    }
}