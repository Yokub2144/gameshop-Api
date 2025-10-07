using Gameshop_Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;


namespace Gameshop_Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<User> Users { get; set; }

        public DbSet<Game> Games { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");
                entity.HasKey(e => e.uid);

                entity.Property(e => e.uid).HasColumnName("uid");
                entity.Property(e => e.fullname).HasColumnName("fullname");
                entity.Property(e => e.email).HasColumnName("email");
                entity.Property(e => e.password).HasColumnName("password");
                entity.Property(e => e.profile_image).HasColumnName("profile_image");
                entity.Property(e => e.role).HasColumnName("role");
            });
            modelBuilder.Entity<Game>(entity =>
                     {
                         entity.ToTable("Games");
                         entity.HasKey(e => e.game_Id);

                         entity.Property(e => e.game_Id).HasColumnName("game_Id");
                         entity.Property(e => e.title).HasColumnName("title");
                         entity.Property(e => e.rank).HasColumnName("rank");
                         entity.Property(e => e.category).HasColumnName("cetegory");
                         entity.Property(e => e.price).HasColumnName("price");
                         entity.Property(e => e.release_date).HasColumnName("release_date");
                         entity.Property(e => e.image_url).HasColumnName("image_url");
                     });

        }
    }
}
