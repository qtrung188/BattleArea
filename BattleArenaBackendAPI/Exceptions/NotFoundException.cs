using Microsoft.AspNetCore.Http;

namespace BattleArenaBackendAPI.Exceptions
{
    /// <summary>
    /// Thrown when a requested resource does not exist. Maps to HTTP 404.
    /// </summary>
    public class NotFoundException : AppException
    {
        public override int StatusCode => StatusCodes.Status404NotFound;

        public override string Title => "Resource not found";

        public NotFoundException(string message) : base(message)
        {
        }
    }
}
