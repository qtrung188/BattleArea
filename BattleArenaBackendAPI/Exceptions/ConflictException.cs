using Microsoft.AspNetCore.Http;

namespace BattleArenaBackendAPI.Exceptions
{
    /// <summary>
    /// Thrown when a request conflicts with the current state of a resource
    /// (e.g. a duplicate username, or insufficient funds). Maps to HTTP 409.
    /// </summary>
    public class ConflictException : AppException
    {
        public override int StatusCode => StatusCodes.Status409Conflict;

        public override string Title => "Conflict";

        public ConflictException(string message) : base(message)
        {
        }
    }
}
