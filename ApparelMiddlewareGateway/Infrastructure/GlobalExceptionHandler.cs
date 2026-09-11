using Microsoft.AspNetCore.Diagnostics;

namespace ApparelMiddlewareGateway.Infrastructure
{
    /// <summary>
    /// Catches any unhandled exception, logs it server-side, and returns an
    /// RFC 7807 ProblemDetails payload so internal details never leak to the LCNC client.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetailsService;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(
            IProblemDetailsService problemDetailsService,
            ILogger<GlobalExceptionHandler> logger)
        {
            _problemDetailsService = problemDetailsService;
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails =
                {
                    Title = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
                }
            });
        }
    }
}
