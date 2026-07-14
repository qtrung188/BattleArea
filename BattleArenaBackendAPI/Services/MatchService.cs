using BattleArenaBackendAPI.Data;
using BattleArenaBackendAPI.Exceptions;
using BattleArenaBackendAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BattleArenaBackendAPI.Services
{
    public class MatchService : IMatchService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILeaderboardService _leaderboardService;

        public MatchService(AppDbContext db, IConfiguration config, ILeaderboardService leaderboardService)
        {
            _db = db;
            _config = config;
            _leaderboardService = leaderboardService;
        }

        public async Task<Guid> StartMatchAsync(Guid userId)
        {
            var match = new Match
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Score = 0,
                StartedAt = DateTime.UtcNow,
                IsValidated = false
            };

            _db.Matches.Add(match);
            await _db.SaveChangesAsync();

            return match.Id;
        }

        public async Task SubmitScoreAsync(Guid userId, Guid matchId, int score)
        {
            var match = await _db.Matches.FirstOrDefaultAsync(m => m.Id == matchId && m.UserId == userId);

            if (match == null)
            {
                throw new NotFoundException("Match not found.");
            }

            if(match.UserId != userId)
            {
                throw new ForbiddenException("You are not allowed to submit score for this match.");
            }

            if(match.EndedAt != null)
            {
                throw new ConflictException("Match has already ended.");
            }

            var endedAt = DateTime.UtcNow;
            var duration = (endedAt - match.StartedAt).TotalSeconds;

            if(duration <= 0) duration = 1; // Prevent division by zero or negative duration

            var maxScorePerSecond = _config.GetValue<double>("GameSettings:MaxScorePerSecond", 50.0);

            if(score / duration > maxScorePerSecond)
            {
                throw new ConflictException("Score validation failed. Score is too high for the elapsed time.");
            }

            match.Score = score;
            match.EndedAt = endedAt;
            match.IsValidated = true;

            await _db.SaveChangesAsync();
            await _leaderboardService.SubmitScoreAsync(userId, score);
        }
    }
}
