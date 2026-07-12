using System.ComponentModel.DataAnnotations;

namespace BattleArenaBackendAPI.DTOs
{
    public class SubmitScoreRequest
    {
        public Guid MatchId { get; set; }

        [Range(0, double.MaxValue)]
        public double Score { get; set; }
    }

    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public double Score { get; set; }
    }
}
