namespace BattleArenaBackendAPI.Models
{
    public class Match
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public int Score { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public bool IsValidated { get; set; }

    }
}
