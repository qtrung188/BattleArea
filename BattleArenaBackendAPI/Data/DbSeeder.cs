using BattleArenaBackendAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleArenaBackendAPI.Data
{
    /// <summary>
    /// Seed dữ liệu mẫu khi khởi động. Idempotent — chỉ thêm item khi bảng Items
    /// đang trống, nên chạy lại nhiều lần không tạo bản ghi trùng.
    /// </summary>
    public static class DbSeeder
    {
        private static readonly Item[] SeedItems =
        {
            new() { Name = "Iron Sword",      Price = 100, Type = "Weapon" },
            new() { Name = "Wooden Shield",   Price = 80,  Type = "Armor" },
            new() { Name = "Steel Sword",     Price = 250, Type = "Weapon" },
            new() { Name = "Dragon Blade",    Price = 750, Type = "Weapon" },
            new() { Name = "Leather Armor",   Price = 120, Type = "Armor" },
            new() { Name = "Plate Armor",     Price = 400, Type = "Armor" },
            new() { Name = "Health Potion",   Price = 30,  Type = "Consumable" },
            new() { Name = "Mana Potion",     Price = 35,  Type = "Consumable" },
            new() { Name = "Magic Staff",     Price = 320, Type = "Weapon" },
            new() { Name = "Golden Helmet",   Price = 280, Type = "Armor" },
            new() { Name = "Elixir of Power", Price = 500, Type = "Consumable" },
            new() { Name = "Phoenix Bow",     Price = 680, Type = "Weapon" },
            new() { Name = "Guardian Ring",   Price = 900, Type = "Accessory" },
        };

        public static async Task SeedAsync(AppDbContext db)
        {
            // Đã có item → không seed lại.
            if (await db.Items.AnyAsync())
            {
                return;
            }

            db.Items.AddRange(SeedItems);
            await db.SaveChangesAsync();
        }
    }
}
