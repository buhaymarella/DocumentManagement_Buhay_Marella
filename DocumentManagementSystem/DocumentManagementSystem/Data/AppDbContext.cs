using DocumentManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagementSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Document>(e =>
            {
                e.ToTable("Documents");
                e.HasKey(d => d.Id);
                e.Property(d => d.OriginalFileName).IsRequired().HasMaxLength(255);
                e.Property(d => d.StoredFileName).IsRequired().HasMaxLength(100);
                e.Property(d => d.ContentType).IsRequired().HasMaxLength(127);
                e.HasIndex(d => d.UploadedAtUtc);
            });
        }
    }
}
