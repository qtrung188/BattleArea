using BattleArenaBackendAPI.DTOs;

namespace BattleArenaBackendAPI.Services
{
    public interface ILeaderboardService
    {
        /// <summary>
        /// Records the user's score on the global sorted set (ZADD).
        /// </summary>
        Task SubmitScoreAsync(Guid userId, double score);

        /// <summary>
        /// Returns the top N entries (ZREVRANGE), resolving usernames from the database.
        /// </summary>
        Task<List<LeaderboardEntryDto>> GetTopAsync(int count = 10);
    }
}
