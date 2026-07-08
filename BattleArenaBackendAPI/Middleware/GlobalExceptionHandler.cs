using BattleArenaBackendAPI.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BattleArenaBackendAPI.Middleware
{
    /// <summary>
    /// Centralized exception handler that converts unhandled exceptions into
    /// RFC 7807 ProblemDetails responses. Expected business exceptions
    /// (<see cref="AppException"/>) map to their declared status code and are not
    /// logged as errors; anything else falls back to a generic 500.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            int statusCode;
            string title;
            string detail;

            if (exception is AppException appException)
            {
                // Expected business exception — map to its declared status code.
                // These are normal control flow (bad input, conflicts, missing
                // resources), so we do NOT log them as server errors.
                statusCode = appException.StatusCode;
                title = appException.Title;
                detail = appException.Message;
            }
            else
            {
                // Unmapped/unexpected exception — log it and return a generic 500
                // so we never leak a stack trace or internal details to the client.
                statusCode = StatusCodes.Status500InternalServerError;
                title = "An unexpected error occurred";
                detail = "An unexpected error occurred while processing your request.";

                _logger.LogError(
                    exception,
                    "Unhandled exception processing {Method} {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = statusCode;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problemDetails
            });
        }
    }
}
