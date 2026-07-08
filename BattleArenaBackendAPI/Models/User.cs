namespace BattleArenaBackendAPI.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public int Gold { get; set; } = 1000;

        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<UserInventory> Inventory { get; set; } = new List<UserInventory>();
    }
}
