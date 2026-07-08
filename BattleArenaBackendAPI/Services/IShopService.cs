using BattleArenaBackendAPI.DTOs;

namespace BattleArenaBackendAPI.Services
{
    public interface IShopService
    {
        Task<PagedResult<ItemDto>> GetItemsAsync(PagedRequest request);

        Task<PagedResult<InventoryItemDto>> GetInventoryAsync(Guid userId, PagedRequest request);

        /// <summary>
        /// Purchases <paramref name="quantity"/> of an item for the user. Throws
        /// <see cref="Exceptions.NotFoundException"/> if the item (or user) does
        /// not exist, or <see cref="Exceptions.ConflictException"/> if the user
        /// cannot afford it.
        /// </summary>
        Task<BuyResponse> BuyAsync(Guid userId, int itemId, int quantity);
    }
}
