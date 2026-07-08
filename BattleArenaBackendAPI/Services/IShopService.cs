using BattleArenaBackendAPI.DTOs;

namespace BattleArenaBackendAPI.Services
{
    public enum BuyOutcome
    {
        Success,
        ItemNotFound,
        UserNotFound,
        InsufficientGold
    }

    public interface IShopService
    {
        Task<List<ItemDto>> GetItemsAsync();

        Task<List<InventoryItemDto>> GetInventoryAsync(Guid userId);

        Task<(BuyOutcome Outcome, BuyResponse? Response)> BuyAsync(Guid userId, int itemId, int quantity);
    }
}
