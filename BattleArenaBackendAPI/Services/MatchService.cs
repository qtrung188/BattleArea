using BattleArenaBackendAPI.Data;
using BattleArenaBackendAPI.Exceptions;

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

        public Task<Guid> StartMatchAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task SubmitScoreAsync(Guid userId, Guid matchId, int score)
        {
            var match = _db.Matches.FirstOrDefault(m => m.Id == matchId && m.UserId == userId);

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



            throw new NotImplementedException();
        }
    }
}
