using Microsoft.AspNetCore.Http;

namespace BattleArenaBackendAPI.Exceptions
{
    /// <summary>
    /// Thrown when a user attempts to access a resource they don't own. Maps to HTTP 403.
    /// </summary>
    public class ForbiddenException : AppException
    {
        public override int StatusCode => StatusCodes.Status403Forbidden;

        public override string Title => "Forbidden";

        public ForbiddenException(string message) : base(message)
        {
        }
    }
}
