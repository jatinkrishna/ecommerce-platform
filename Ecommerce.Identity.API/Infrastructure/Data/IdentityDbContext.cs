using Ecommerce.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Identity.API.Infrastructure.Data
{
    /// <summary>
    /// Database context for the Identity service
    /// </summary>
    public class IdentityDbContext : DbContext
    {
        public IdentityDbContext(DbContextOptions<IdentityDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                
                entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(256);
                
                entity.HasIndex(u => u.Email)
                    .IsUnique();
                
                entity.Property(u => u.PasswordHash)
                    .IsRequired();
                
                entity.Property(u => u.FirstName)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(u => u.LastName)
                    .IsRequired()
                    .HasMaxLength(50);
                
                entity.Property(u => u.IsActive)
                    .HasDefaultValue(true);
                
                entity.Property(u => u.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }
    }
}
