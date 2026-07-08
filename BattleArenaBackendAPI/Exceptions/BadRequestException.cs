using Microsoft.AspNetCore.Http;

namespace BattleArenaBackendAPI.Exceptions
{
    /// <summary>
    /// Thrown when a request is invalid or cannot be processed as-is. Maps to HTTP 400.
    /// </summary>
    public class BadRequestException : AppException
    {
        public override int StatusCode => StatusCodes.Status400BadRequest;

        public override string Title => "Bad request";

        public BadRequestException(string message) : base(message)
        {
        }
    }
}
