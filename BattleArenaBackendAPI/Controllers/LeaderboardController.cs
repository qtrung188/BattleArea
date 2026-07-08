using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private readonly ILeaderboardService _leaderboardService;

        public LeaderboardController(ILeaderboardService leaderboardService)
        {
            _leaderboardService = leaderboardService;
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

            await _leaderboardService.SubmitScoreAsync(userId.Value, request.Score);
            return Ok(new { message = "Score submitted.", score = request.Score });
        }

        [HttpGet("top")]
        public async Task<ActionResult<List<LeaderboardEntryDto>>> GetTop()
        {
            var top = await _leaderboardService.GetTopAsync(10);
            return Ok(top);
        }
    }
}
