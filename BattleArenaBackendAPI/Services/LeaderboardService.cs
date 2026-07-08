using BattleArenaBackendAPI.Data;
using BattleArenaBackendAPI.DTOs;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace BattleArenaBackendAPI.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private const string LeaderboardKey = "leaderboard:global";

        private readonly IConnectionMultiplexer _redis;
        private readonly AppDbContext _db;

        public LeaderboardService(IConnectionMultiplexer redis, AppDbContext db)
        {
            _redis = redis;
            _db = db;
        }

        public async Task SubmitScoreAsync(Guid userId, double score)
        {
            var db = _redis.GetDatabase();
            // ZADD leaderboard:global <score> <userId>
            await db.SortedSetAddAsync(LeaderboardKey, userId.ToString(), score);
        }

        public async Task<List<LeaderboardEntryDto>> GetTopAsync(int count = 10)
        {
            var redisDb = _redis.GetDatabase();

            // ZREVRANGE leaderboard:global 0 count-1 WITHSCORES (highest first)
            var entries = await redisDb.SortedSetRangeByRankWithScoresAsync(
                LeaderboardKey,
                start: 0,
                stop: count - 1,
                order: Order.Descending);

            if (entries.Length == 0)
            {
                return new List<LeaderboardEntryDto>();
            }

            // Redis stores only UserId as the member — resolve usernames from Postgres.
            var userIds = entries
                .Select(e => Guid.TryParse(e.Element.ToString(), out var id) ? id : (Guid?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToList();

            var usernames = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.Username);

            var result = new List<LeaderboardEntryDto>();
            var rank = 1;
            foreach (var entry in entries)
            {
                Guid.TryParse(entry.Element.ToString(), out var userId);
                usernames.TryGetValue(userId, out var username);

                result.Add(new LeaderboardEntryDto
                {
                    Rank = rank++,
                    UserId = userId,
                    Username = username ?? "(unknown)",
                    Score = entry.Score
                });
            }

            return result;
        }
    }
}
