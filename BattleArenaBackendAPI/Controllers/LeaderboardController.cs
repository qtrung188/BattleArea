using Asp.Versioning;
using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;
        private readonly IMatchService _matchService;

        public LeaderboardController(ILeaderboardService leaderboardService, IMatchService matchService)
        {
            _leaderboardService = leaderboardService;
            _matchService = matchService;
        }

        [Authorize]
        [HttpPost("score")]
        public async Task<IActionResult> SubmitScore([FromBody] SubmitScoreRequest request)
        {
            var userId = User.GetUserId();
            if (userId is null)
            {
                return Unauthorized();
            }

            // Using MatchService for validation and SQL saving instead of direct LeaderboardService call
            // Cast double to int as IMatchService SubmitScoreAsync expects an int score
            await _matchService.SubmitScoreAsync(userId.Value, request.MatchId, (int)request.Score);
            return NoContent();
        }

        [HttpGet("top")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> GetTop()
        {
            var top = await _leaderboardService.GetTopAsync(10);
            return Ok(top);
        }
    }
}
