using AuditTrail_API.Models;
using Microsoft.EntityFrameworkCore;


namespace AuditTrail_API.Data
{
    public class AuditDbContext:DbContext
    {
        public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options) { }
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.Timestamp).IsRequired();
                e.Property(a => a.UserId).HasMaxLength(256);
                e.Property(a => a.EntityName).HasMaxLength(256);
                e.Property(a => a.EntityId).HasMaxLength(256);
                e.Property(a => a.CorrelationId).HasMaxLength(256);
                e.Property(a => a.ChangesJson).HasColumnType("TEXT"); // JSON text (SQLite)
                e.HasIndex(a => new { a.EntityName, a.EntityId });
                e.HasIndex(a => a.Timestamp);
            });
        }
    }
}
