using Gameshop_Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace Gameshop_Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.uid);

                entity.Property(e => e.uid).HasColumnName("uid");
                entity.Property(e => e.fullname).HasColumnName("fullname");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.profile_image).HasColumnName("profile_image");
                entity.Property(e => e.role).HasColumnName("role");
            });

        }
    }
}
