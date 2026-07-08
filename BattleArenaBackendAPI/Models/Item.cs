namespace BattleArenaBackendAPI.Models
{
    public class Item
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int Price { get; set; }

        public string Type { get; set; } = string.Empty;

        // Navigation
        public ICollection<UserInventory> InventoryEntries { get; set; } = new List<UserInventory>();
    }
}
