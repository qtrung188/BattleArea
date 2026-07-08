namespace BattleArenaBackendAPI.Exceptions
{
    /// <summary>
    /// Base type for expected, business-level exceptions that map to a specific
    /// HTTP status code. These are surfaced to clients as RFC 7807 ProblemDetails
    /// by the GlobalExceptionHandler and are NOT logged as server errors.
    /// </summary>
    public abstract class AppException : Exception
    {
        /// <summary>HTTP status code this exception maps to.</summary>
        public abstract int StatusCode { get; }

        /// <summary>Short, human-readable summary used as the ProblemDetails title.</summary>
        public abstract string Title { get; }

        protected AppException(string message) : base(message)
        {
        }

        protected AppException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
