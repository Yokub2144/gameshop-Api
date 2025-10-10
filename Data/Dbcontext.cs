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
        public DbSet<Cart> Cart { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
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
                entity.Property(e => e.category).HasColumnName("category");
                entity.Property(e => e.price).HasColumnName("price");
                entity.Property(e => e.release_date).HasColumnName("release_date");
                entity.Property(e => e.image_url).HasColumnName("image_url");
                entity.Property(e => e.detail).HasColumnName("detail");

            });
            modelBuilder.Entity<Wallet>(entity =>
            {
                entity.ToTable("Wallet");
                entity.HasKey(e => e.uid);

                entity.Property(e => e.uid).HasColumnName("uid");
                entity.Property(e => e.balance).HasColumnName("balance");
            });
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("Transaction");
                entity.HasKey(e => e.tid);

                entity.Property(e => e.tid).HasColumnName("tId");
                entity.Property(e => e.uid).HasColumnName("uid");
                entity.Property(e => e.transaction_type).HasColumnName("transaction_type");
                entity.Property(e => e.reference_id).HasColumnName("reference_id");
                entity.Property(e => e.amount_value).HasColumnName("amount_value");
                entity.Property(e => e.detail).HasColumnName("detail");
                entity.Property(e => e.status).HasColumnName("status");
                entity.Property(e => e.created_at).HasColumnName("created_at");
            });
            modelBuilder.Entity<Orders>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.oid);

                entity.Property(e => e.oid).HasColumnName("oid");
                entity.Property(e => e.uid).HasColumnName("uid");
                entity.Property(e => e.order_date).HasColumnName("order_date");
                entity.Property(e => e.total_amount).HasColumnName("total_amount");
            });
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");
                entity.HasKey(e => e.did);

                entity.Property(e => e.did).HasColumnName("did");
                entity.Property(e => e.oid).HasColumnName("oid");
                entity.Property(e => e.game_id).HasColumnName("game_id");
                entity.Property(e => e.price).HasColumnName("price");
            });
        }
    }
}
