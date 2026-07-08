namespace BattleArenaBackendAPI.Models
{
    public class UserInventory
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int ItemId { get; set; }

        public int Quantity { get; set; } = 1;

        public bool IsEquipped { get; set; } = false;

        // Navigation
        public User User { get; set; } = null!;

        public Item Item { get; set; } = null!;
    }
}
