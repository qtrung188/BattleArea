using BattleArenaBackendAPI.Models;

namespace BattleArenaBackendAPI.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user. Throws <see cref="Exceptions.ConflictException"/>
        /// if the username is already taken.
        /// </summary>
        Task<User> RegisterAsync(string username, string password);

        /// <summary>
        /// Validates credentials and returns a signed JWT. Throws
        /// <see cref="Exceptions.BadRequestException"/> if authentication fails.
        /// </summary>
        Task<(string AccessToken, string RefreshToken)> LoginAsync(string username, string password);

        Task<string> RefreshAccessTokenAsync(string refreshTokenPlain);

        Task RevokeRefreshTokenAsync(string refreshTokenPlain);
    }
}
