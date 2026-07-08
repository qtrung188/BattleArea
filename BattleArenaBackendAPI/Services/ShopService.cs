using BattleArenaBackendAPI.Data;
using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Exceptions;
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

        public async Task<PagedResult<ItemDto>> GetItemsAsync(PagedRequest request)
        {
            request.Validate();

            var query = _db.Items.OrderBy(i => i.Id);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Price = i.Price,
                    Type = i.Type
                })
                .ToListAsync();

            return new PagedResult<ItemDto>(items, totalCount, request.Page, request.PageSize);
        }

        public async Task<PagedResult<InventoryItemDto>> GetInventoryAsync(Guid userId, PagedRequest request)
        {
            request.Validate();

            var query = _db.UserInventories
                .Where(ui => ui.UserId == userId)
                .OrderBy(ui => ui.ItemId);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ui => new InventoryItemDto
                {
                    ItemId = ui.ItemId,
                    Name = ui.Item.Name,
                    Type = ui.Item.Type,
                    Quantity = ui.Quantity,
                    IsEquipped = ui.IsEquipped
                })
                .ToListAsync();

            return new PagedResult<InventoryItemDto>(items, totalCount, request.Page, request.PageSize);
        }

        public async Task<BuyResponse> BuyAsync(Guid userId, int itemId, int quantity)
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
                    throw new NotFoundException($"Item {itemId} was not found.");
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user is null)
                {
                    throw new NotFoundException("User was not found.");
                }

                var totalCost = item.Price * quantity;
                if (user.Gold < totalCost)
                {
                    throw new ConflictException("Not enough gold to complete this purchase.");
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

                return new BuyResponse
                {
                    ItemId = item.Id,
                    ItemName = item.Name,
                    QuantityPurchased = quantity,
                    TotalCost = totalCost,
                    RemainingGold = user.Gold,
                    QuantityOwned = inventory.Quantity
                };
            }
            catch
            {
                // Roll back on any failure — whether a business exception (item
                // not found / insufficient gold) or an unexpected one — then let
                // it bubble up to the global handler.
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
