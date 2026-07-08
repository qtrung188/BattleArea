using BattleArenaBackendAPI.Data;
using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleArenaBackendAPI.Services
{
    public class ShopService : IShopService
    {
        private readonly AppDbContext _db;

        public ShopService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<ItemDto>> GetItemsAsync()
        {
            return await _db.Items
                .OrderBy(i => i.Id)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    Type = i.Type
                })
                .ToListAsync();
        }

        public async Task<List<InventoryItemDto>> GetInventoryAsync(Guid userId)
        {
            return await _db.UserInventories
                .Where(ui => ui.UserId == userId)
                .OrderBy(ui => ui.ItemId)
                .Select(ui => new InventoryItemDto
                {
                    ItemId = ui.ItemId,
                    Name = ui.Item.Name,
                    Type = ui.Item.Type,
                    Quantity = ui.Quantity,
                    IsEquipped = ui.IsEquipped
                })
                .ToListAsync();
        }

        public async Task<(BuyOutcome Outcome, BuyResponse? Response)> BuyAsync(Guid userId, int itemId, int quantity)
        {
            if (quantity < 1)
            {
                quantity = 1;
            }

            // Wrap the read-modify-write of Gold + inventory in a DB transaction so both
            // succeed or both roll back together.
            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == itemId);
                if (item is null)
                {
                    return (BuyOutcome.ItemNotFound, null);
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user is null)
                {
                    return (BuyOutcome.UserNotFound, null);
                }

                var totalCost = item.Price * quantity;
                if (user.Gold < totalCost)
                {
                    return (BuyOutcome.InsufficientGold, null);
                }

                // 1) Deduct gold.
                user.Gold -= totalCost;

                // 2) Add or increment the inventory row.
                var inventory = await _db.UserInventories
                    .FirstOrDefaultAsync(ui => ui.UserId == userId && ui.ItemId == itemId);

                if (inventory is null)
                {
                    inventory = new UserInventory
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        ItemId = itemId,
                        Quantity = quantity,
                        IsEquipped = false
                    };
                    _db.UserInventories.Add(inventory);
                }
                else
                {
                    inventory.Quantity += quantity;
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                return (BuyOutcome.Success, new BuyResponse
                {
                    ItemId = item.Id,
                    ItemName = item.Name,
                    QuantityPurchased = quantity,
                    TotalCost = totalCost,
                    RemainingGold = user.Gold,
                    QuantityOwned = inventory.Quantity
                });
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
