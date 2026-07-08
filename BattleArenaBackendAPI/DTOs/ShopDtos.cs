using System.ComponentModel.DataAnnotations;

namespace BattleArenaBackendAPI.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Price { get; set; }
        public string Type { get; set; } = string.Empty;
    }

    public class BuyRequest
    {
        [Required]
        public int ItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }

    public class BuyResponse
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int QuantityPurchased { get; set; }
        public int TotalCost { get; set; }
        public int RemainingGold { get; set; }
        public int QuantityOwned { get; set; }
    }

    public class InventoryItemDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public bool IsEquipped { get; set; }
    }
}
