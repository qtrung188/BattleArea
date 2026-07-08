using System.Security.Claims;

namespace BattleArenaBackendAPI.Controllers
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Extracts the authenticated user's Id from the JWT (NameIdentifier / sub claim).
        /// Returns null if absent or unparseable.
        /// </summary>
        public static Guid? GetUserId(this ClaimsPrincipal principal)
        {
            var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue("sub");

            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
