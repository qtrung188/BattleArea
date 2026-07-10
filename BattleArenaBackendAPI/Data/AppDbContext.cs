using BattleArenaBackendAPI.Migrations;
using BattleArenaBackendAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleArenaBackendAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<UserInventory> UserInventories => Set<UserInventory>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Username)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(u => u.Username)
                    .IsUnique();

                entity.Property(u => u.PasswordHash)
                    .IsRequired();

                entity.Property(u => u.Gold)
                    .HasDefaultValue(1000);

                entity.Property(u => u.CreatedAt)
                    .IsRequired();
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(i => i.Id);

                // int PK is identity by default; make it explicit
                entity.Property(i => i.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(i => i.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(i => i.Type)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(i => i.Price)
                    .IsRequired();

                // Price >= 0
                entity.ToTable(t => t.HasCheckConstraint("CK_Item_Price_NonNegative", "\"Price\" >= 0"));
            });

            modelBuilder.Entity<UserInventory>(entity =>
            {
                entity.HasKey(ui => ui.Id);

                entity.Property(ui => ui.Quantity)
                    .HasDefaultValue(1);

                entity.Property(ui => ui.IsEquipped)
                    .HasDefaultValue(false);

                // Quantity > 0
                entity.ToTable(t => t.HasCheckConstraint("CK_UserInventory_Quantity_Positive", "\"Quantity\" > 0"));

                entity.HasOne(ui => ui.User)
                    .WithMany(u => u.Inventory)
                    .HasForeignKey(ui => ui.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ui => ui.Item)
                    .WithMany(i => i.InventoryEntries)
                    .HasForeignKey(ui => ui.ItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                // A user holds a given item in a single stackable row
                entity.HasIndex(ui => new { ui.UserId, ui.ItemId })
                    .IsUnique();
            });
        }
    }
}
