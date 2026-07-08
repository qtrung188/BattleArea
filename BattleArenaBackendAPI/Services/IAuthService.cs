using BattleArenaBackendAPI.Models;

namespace BattleArenaBackendAPI.Services
{
    public enum RegisterResult
    {
        Success,
        UsernameTaken
    }

    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user. Returns (UsernameTaken, null) if the username already exists.
        /// </summary>
        Task<(RegisterResult Result, User? User)> RegisterAsync(string username, string password);

        /// <summary>
        /// Validates credentials and returns a signed JWT, or null if authentication fails.
        /// </summary>
        Task<string?> LoginAsync(string username, string password);

        /// <summary>
        /// Generates a signed JWT for an already-authenticated user.
        /// </summary>
        string GenerateToken(User user);
    }
}
