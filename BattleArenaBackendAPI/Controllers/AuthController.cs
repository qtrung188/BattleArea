using BattleArenaBackendAPI.DTOs;
using BattleArenaBackendAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var (result, user) = await _authService.RegisterAsync(request.Username, request.Password);

            if (result == RegisterResult.UsernameTaken)
            {
                return Conflict(new { message = "Username is already taken." });
            }

            // 201 Created
            return StatusCode(StatusCodes.Status201Created, new
            {
                id = user!.Id,
                username = user.Username,
                gold = user.Gold,
                createdAt = user.CreatedAt
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var token = await _authService.LoginAsync(request.Username, request.Password);

            if (token is null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(new LoginResponse { Token = token });
        }
    }
}
