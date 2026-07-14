using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [Route("api/v1/matches")]
    [ApiController]
    [Authorize]
    public class MatchController : ControllerBase
    {
        private readonly IMatchService _matchService;

        public MatchController(IMatchService matchService)
        {
            _matchService = matchService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartMatch()
        {
            var userId = User.GetUserId();
            if(userId == null)
            {
                return Unauthorized();
            }

            var matchId = await _matchService.StartMatchAsync(userId.Value);
            return Ok(new { MatchId = matchId });
        }
    }
}
